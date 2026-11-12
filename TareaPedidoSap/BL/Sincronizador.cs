using K66_API.Models;
using K66_API.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TareaPedido.Model;
using static K66_API.Models.ResponseModel;

namespace TareaPedido.BL
{
    public class Sincronizador
    {
        public void procesarPedidos(string cadena, string urlApi)
        {
            try
            {
                string cadenaConexion = cadena;
                string url = urlApi;
                List<PedidoK66> pedidos = ObtenerPedidos(cadenaConexion);
                foreach (var pedido in pedidos)
                {
                    Console.WriteLine(pedido.PedidoId);
                    List<PedidoDetalleK66> detalles = ObtenerDetallesPedido(cadenaConexion, pedido.PedidoId);
                    pedido.Detalles = detalles;
                    try
                    {
                        var response = InsertarPedido(pedido);
                        if (response.result)
                        {
                            UpdateDocumentExito(response, cadenaConexion, pedido.PedidoId);
                        }
                        else
                        {
                            UpdateDocumentError(response, cadenaConexion, pedido.PedidoId);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error inesperado: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }
        }

        public void UpdateDocumentExito(Response document, string cadenaConexion, long PedidoId)
        {
            string query = @"
            UPDATE Pedido_K66
            SET DocEntry_Order = @DOCNUM,CUSTOMER_ORDER_ID = @DOCNUM,
                Sincronizado = 1,
                Fecha_Hora_Sincronizacion = getdate(),
                mensaje_sdk = null,
                estado_id = 4
            WHERE Pedido_Id = @ID";

            using (SqlConnection connection = new SqlConnection(cadenaConexion))
            {
                try
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        // Asignar los parámetros
                        command.Parameters.AddWithValue("@ID", PedidoId);
                        command.Parameters.AddWithValue("@DOCNUM", document.DOCNUM);

                        // Ejecutar la consulta
                        int rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            Console.WriteLine("La actualización se realizó con éxito.");
                        }
                        else
                        {
                            Console.WriteLine("No se encontró un registro con el ID proporcionado.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error al actualizar el documento: {ex.Message}");
                }
            }
        }

        public void UpdateDocumentError(Response document, string cadenaConexion, long PedidoId)
        {
            string query = @"
            UPDATE Pedido_K66
            SET mensaje_sdk = @DescripcionError,
            Fecha_Hora_ultimo_Intento = getdate()
            WHERE Pedido_Id = @ID";

            using (SqlConnection connection = new SqlConnection(cadenaConexion))
            {
                try
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        // Asignar los parámetros
                        command.Parameters.AddWithValue("@ID", PedidoId);
                        command.Parameters.AddWithValue("@DescripcionError", document.descripcion_error ?? (object)DBNull.Value);

                        // Ejecutar la consulta
                        int rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            Console.WriteLine("La actualización se realizó con éxito.");
                        }
                        else
                        {
                            Console.WriteLine("No se encontró un registro con el ID proporcionado.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error al actualizar el documento: {ex.Message}");
                }
            }
        }

        internal Response InsertarPedido(PedidoK66 pedido)
        {
            Response respuesta = new Response();
            
            try
            {
                string empresa = "";
                if (pedido.EmpresaId == 20210705001)
                {
                    empresa = "BOLIK";
                }
                else if (pedido.EmpresaId == 20210705002)
                {
                    empresa = "EMPAQUES";
                }
                if (pedido.EmpresaId == 20210705003)
                {
                    empresa = "ESCOCESA";
                }
                if (pedido.EmpresaId == 20210705004)
                {
                    empresa = "GRACO";
                }
                ApplicationContext.SetApplication(empresa);
                SAPbobsCOM.Documents oOrder;
                SAPbobsCOM.Recordset oRecord;
                SAPbobsCOM.Recordset oRecordSAR;
                SAPbobsCOM.BusinessPartners bp;

                string Query2 = $@"select 1 from ordr where ""U_PEDIDO_K66"" = '{pedido.PedidoId}'";

                oOrder = (SAPbobsCOM.Documents)ApplicationContext.SBOCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oOrders);
                oRecord = (SAPbobsCOM.Recordset)ApplicationContext.SBOCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
                oRecordSAR = (SAPbobsCOM.Recordset)ApplicationContext.SBOCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
                bp = (SAPbobsCOM.BusinessPartners)ApplicationContext.SBOCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oBusinessPartners);
                oRecordSAR.DoQuery(Query2);
                bool hasResults = !oRecordSAR.EoF;
                if (hasResults)
                {
                    throw new Exception("Ya fue ingresado a sap el pedido = " + pedido.PedidoId);
                }
                bool existe = bp.GetByKey(pedido.IDK66);
                
                oOrder.CardCode = pedido.IDK66;
                Console.WriteLine(bp.SalesPersonCode);
                Console.WriteLine(bp.CardCode);
                Console.WriteLine(ApplicationContext.SBOCompany.CompanyDB);
                oOrder.SalesPersonCode = bp.SalesPersonCode;
                DateTime fechaAEnviar = DateTime.SpecifyKind(pedido.Fecha, DateTimeKind.Utc);
                oOrder.DocDate = fechaAEnviar;
                oOrder.DocDueDate = pedido.FechaPrometida.Value;
                oOrder.DocCurrency = bp.DefaultCurrency;
                oOrder.Series = Int32.Parse(pedido.serie_sap);
                
                if(pedido.ObservacionesGenerales.Length > 254) {
                    oOrder.Comments = pedido.ObservacionesGenerales.Substring(0,253);
                }
                else
                {
                    oOrder.Comments = pedido.ObservacionesGenerales;
                }

                string pattern = @"\((.*?)\)";
                if (!pedido.DireccionEntrega.Equals("Guatemala"))
                {
                    Match match = Regex.Match(pedido.DireccionEntrega, pattern);

                    string result = match.Groups[1].Value;
                    oOrder.ShipToCode = result;
                }
                oOrder.PayToCode = bp.BilltoDefault;

                Console.WriteLine(bp.VatLiable == SAPbobsCOM.BoVatStatus.vLiable);
                Console.WriteLine(bp.Currency);
                oOrder.UserFields.Fields.Item("U_TipoPedido").Value = pedido.TipoPedidoId;
                oOrder.UserFields.Fields.Item("U_Usuario").Value = pedido.Vendedor;
                oOrder.UserFields.Fields.Item("U_PEDIDO_K66").Value = pedido.PedidoId.ToString();
                oOrder.UserFields.Fields.Item("U_Orden_Compra").Value = pedido.OrdenCompraCliente != null ? pedido.OrdenCompraCliente : "";
                
                oOrder.UserFields.Fields.Item("U_FacNit").Value = pedido.Nit.Equals("") ? bp.FederalTaxID : pedido.Nit;
                oOrder.UserFields.Fields.Item("U_FacNom").Value = pedido.Nombre.Equals("") ? bp.CardName : pedido.Nombre;
                oOrder.UserFields.Fields.Item("U_FacFecha").Value = pedido.Fecha.ToString("dd-MM-yyyy");
                oOrder.UserFields.Fields.Item("U_Tienda").Value = "01";
                if (pedido.IDK66 == "CL0400" ||
                    pedido.IDK66 == "CL0401" ||
                    pedido.IDK66 == "CL0162" ||
                    pedido.IDK66 == "CL0163" ||
                    pedido.IDK66 == "CL0148" ||
                    pedido.IDK66 == "CL0149")
                {//bloque para clientes genéricos
                    oOrder.UserFields.Fields.Item("U_NIT_SN_FEL").Value = pedido.Nit;
                    oOrder.UserFields.Fields.Item("U_NOMBRE_SN_FEL").Value = pedido.Nombre;
                    oOrder.UserFields.Fields.Item("U_DIRECCION_SN_FEL").Value = pedido.Direccion;
                }
                if (pedido.Direccion.Length > 70)
                {
                    oOrder.UserFields.Fields.Item("U_direccion_consignatario").Value = pedido.Direccion.Substring(0, 69);
                }
                else
                {
                    oOrder.UserFields.Fields.Item("U_direccion_consignatario").Value = pedido.Direccion;
                }

                oOrder.UserFields.Fields.Item("U_OC_Respaldo").Value = pedido.DocumentoOrdenCompraRespaldo;
                if (!pedido.Anexo.Equals(""))
                {
                    oOrder.UserFields.Fields.Item("U_Importante").Value = pedido.Anexo;
                }

                string resolucion2989 = bp.UserFields.Fields.Item("U_Resolucion2989").Value;
                if (resolucion2989.Equals("Y"))
                {
                    oOrder.UserFields.Fields.Item("U_Decreto2989").Value = "Y";
                    string resolucion = bp.UserFields.Fields.Item("U_DescResolucion").Value;
                    oOrder.UserFields.Fields.Item("U_Resolucion").Value = resolucion;
                    oOrder.UserFields.Fields.Item("U_FRASE_EXENTO").Value = "11";
                }
                int i = 0;
                foreach (var value in pedido.Detalles)
                {
                    if (i > 0)
                    {
                        oOrder.Lines.Add();
                    }
                    oOrder.Lines.ItemCode = value.ProductoId;
                    double descuento = (double)value.Descuento;
                    if (descuento > 0)
                    {
                        if(descuento == 100)
                        {
                            oOrder.Lines.UnitPrice = (Double)value.PrecioOriginal/1.12;
                            oOrder.Lines.PriceAfterVAT = 0;
                            oOrder.Lines.LineTotal = 0;
                            oOrder.Lines.DiscountPercent = descuento;
                        }
                        else 
                        {
                            oOrder.Lines.UnitPrice = (Double)value.PrecioOriginal/1.12;
                            oOrder.Lines.LineTotal = (Double)value.Cantidad * ((Double)value.PrecioOriginal / 1.12) * (100-descuento) / 100;
                            oOrder.Lines.DiscountPercent = descuento;
                        }
                    }
                    else
                    {
                        if (value.PrecioCambiado)
                        {
                            oOrder.Lines.PriceAfterVAT = (Double)value.Precio;
                        }
                        else
                        {
                            oOrder.Lines.PriceAfterVAT = (Double)value.PrecioOriginal;
                        }

                    }

                    oOrder.Lines.Quantity = (Double)value.Cantidad;
                    oOrder.Lines.WarehouseCode = value.WarehouseId;
                    if(bp.VatLiable== SAPbobsCOM.BoVatStatus.vLiable)
                    {
                        oOrder.Lines.TaxCode = "IVA";
                    }
                    else
                    {
                        oOrder.Lines.TaxCode = "EXE";
                    }
                    

                    i++;
                }
                oOrder.SaveToFile("C:/temp/" + pedido.PedidoId + ".xml");
                if (oOrder.Add() != 0)
                {
                    ResponseModel CreateResponse = new ResponseModel();
                    ResponseModel.Response jsonrespuesta = CreateResponse.RespuestaSap(false, "", ApplicationContext.SBOCompany.GetLastErrorCode().ToString(), ApplicationContext.SBOCompany.GetLastErrorDescription().ToString().Replace("\"", ""), "");
                    respuesta = jsonrespuesta;
                }
                else
                {
                    ResponseModel CreateResponse = new ResponseModel();
                    SAPbobsCOM.Documents oOrderCreated = (SAPbobsCOM.Documents)ApplicationContext.SBOCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oOrders);
                    string newID = ApplicationContext.SBOCompany.GetNewObjectKey();
                    oOrderCreated.GetByKey(Int32.Parse(newID));

                    ResponseModel.Response jsonrespuesta = CreateResponse.RespuestaSap(true, newID, "", "", oOrderCreated.DocNum.ToString());

                    respuesta = jsonrespuesta;
                }
                Marshal.ReleaseComObject(oRecordSAR);
                GC.Collect();

            }
            catch (Exception ex)
            {
                ResponseModel CreateResponse = new ResponseModel();
                ResponseModel.Response jsonrespuesta = CreateResponse.RespuestaSap(false, "", "-101", ex.ToString().Replace("\"", ""), "");
                respuesta = jsonrespuesta;
            }
            finally
            {
                ApplicationContext.UnsetApplication();
            }

            return respuesta;
        }

        public static List<PedidoDetalleK66> ObtenerDetallesPedido(string connectionString, long pedidoId)
        {
            var detalles = new List<PedidoDetalleK66>();

            string query = @"
            SELECT 
                [Detalle_Id],
                [Pedido_Id],
                [Producto_Id],
                [Nombre],
                [Unidad],
                [Cantidad],
                [Precio],
                [Precio_Original],
                [Precio_Cambiado],
                [Descuento],
                [WarehouseId]
            FROM [Pedido_Detalle_K66]
            WHERE Pedido_Id = @pedido_id";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@pedido_id", pedidoId);
                    connection.Open();

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var detalle = new PedidoDetalleK66();

                            detalle.DetalleId = (int)reader["Detalle_Id"];
                            detalle.PedidoId = (long)reader["Pedido_Id"];
                            detalle.ProductoId = (string)reader["Producto_Id"];
                            detalle.Nombre = (string)reader["Nombre"];
                            detalle.Unidad = (string)reader["Unidad"];
                            detalle.Cantidad = (decimal)reader["Cantidad"];
                            detalle.Precio = (decimal)reader["Precio"];
                            detalle.PrecioOriginal = (decimal)reader["Precio_Original"];
                            detalle.PrecioCambiado = (bool)reader["Precio_Cambiado"];
                            detalle.Descuento = (decimal)reader["Descuento"];
                            detalle.WarehouseId = (string)reader["WarehouseId"];

                            detalles.Add(detalle);
                        }
                    }
                }
            }

            return detalles;
        }

        public string GetFotografiaApp(string connectionString, long pedidoId, int id)
        {
            string query = @"SELECT ISNULL(FotografiaApp, '') AS Anexo 
                        FROM Pedido_Documento_Importante_K66
                        WHERE Pedido_Id = @PedidoId AND Documento_Id = @documentoid";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@PedidoId", pedidoId);
                    command.Parameters.AddWithValue("@documentoid", id);
                    try
                    {
                        connection.Open();
                        var result = command.ExecuteScalar();
                        return result?.ToString() ?? string.Empty;
                    }
                    catch (Exception ex)
                    {
                        // Manejar la excepción según sea necesario
                        Console.WriteLine($"Error: {ex.Message}");
                        return "";
                    }
                }
            }
        }

        public List<PedidoK66> ObtenerPedidos(string connectionString)
        {
            var pedidos = new List<PedidoK66>();

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string formatoFecha = "yyyy-MM-dd";
                string query = @"SELECT							t0.[Pedido_Id],
                                t0.[Empresa_Id],
                                t0.[ID_K66],
                                ISNULL(t0.[Nit],'') [Nit],
                                isnull(t0.[Nombre],'') [Nombre],
                                isnull(t0.[Direccion],'Guatemala') [Direccion],
                                t0.[Responsable_Id],
                                CONVERT(VARCHAR, t0.[Fecha], 23) [Fecha],
                                t0.[Correlativo],
                                isnull(t0.[Direccion_Id],-1)  [Direccion_Id],
                                isnull(t0.[CUSTOMER_ORDER_ROWID],-1) [CUSTOMER_ORDER_ROWID],
                                isnull([CUSTOMER_ORDER_ID],'') CUSTOMER_ORDER_ID,
                                [Sincronizado],
                                [Fecha_Hora_Pedido],
                                [Fecha_Hora_Ultimo_Intento],
                                [Fecha_Hora_Sincronizacion],
                                [Tipo_Pedido_Id],
                                isnull([Orden_Compra_Cliente],'') Orden_Compra_Cliente,
                                isnull([Observaciones_Generales],'') Observaciones_Generales,
                                [Fecha_Prometida],
                                [Estado_Id],
                                isnull([Responsable_Aprobacion_Id],'') Responsable_Aprobacion_Id,
                                isnull([Comentario_Aprobacion],'') Comentario_Aprobacion,
                                isnull([Documento_Orden_Compra_Respaldo],'') Documento_Orden_Compra_Respaldo,
                                [Termino_Entrega],
                                [Vendedor],
                                [Impuesto_TAX],
                                [Moneda],
                                isnull([Direccion_Entrega],'Guatemala') [Direccion_Entrega],
								isnull(max(t1.serie_sap),'-1') [serie_sap]
                        FROM [Pedido_K66] t0
						inner join Usuario_Empresa t1 on t0.Responsable_Id = t1.Usuario_Id and t0.Empresa_Id = t1.Empresa_Id
						and t1.SERIE_SAP !=''
                        where Estado_Id = 2
						and Sincronizado = 0
						group by t0.[Pedido_Id],
                                t0.[Empresa_Id],
                                t0.[ID_K66],
                                t0.[Nit],
                                t0.[Nombre],
                                isnull(t0.[Direccion],'Guatemala') ,
                                t0.[Responsable_Id],
                                CONVERT(VARCHAR, t0.[Fecha], 23),
                                t0.[Correlativo],
                                isnull(t0.[Direccion_Id],-1),
                                isnull(t0.[CUSTOMER_ORDER_ROWID],-1),
                                isnull([CUSTOMER_ORDER_ID],'') ,
                                [Sincronizado],
                                [Fecha_Hora_Pedido],
                                [Fecha_Hora_Ultimo_Intento],
                                [Fecha_Hora_Sincronizacion],
                                [Tipo_Pedido_Id],
                                isnull([Orden_Compra_Cliente],'') ,
                                isnull([Observaciones_Generales],'') ,
                                [Fecha_Prometida],
                                [Estado_Id],
                                isnull([Responsable_Aprobacion_Id],'') ,
                                isnull([Comentario_Aprobacion],'') ,
                                isnull([Documento_Orden_Compra_Respaldo],'') ,
                                [Termino_Entrega],
                                [Vendedor],
                                [Impuesto_TAX],
                                [Moneda],
                                isnull([Direccion_Entrega],'Guatemala')
							order by Pedido_Id asc";

                using (var command = new SqlCommand(query, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var pedido = new PedidoK66();

                            pedido.PedidoId = reader.GetInt64(reader.GetOrdinal("Pedido_Id"));
                            pedido.EmpresaId = reader.GetInt64(reader.GetOrdinal("Empresa_Id"));
                            pedido.IDK66 = reader.GetString(reader.GetOrdinal("ID_K66"));
                            pedido.Nit = reader.GetString(reader.GetOrdinal("Nit"));
                            pedido.Nombre = reader.GetString(reader.GetOrdinal("Nombre"));
                            pedido.Direccion = reader.GetString(reader.GetOrdinal("Direccion"));
                            pedido.ResponsableId = reader.GetInt64(reader.GetOrdinal("Responsable_Id"));
                            pedido.Fecha = DateTime.ParseExact(reader.GetString(reader.GetOrdinal("Fecha")), formatoFecha, CultureInfo.InvariantCulture); 
                            pedido.Correlativo = reader.GetInt32(reader.GetOrdinal("Correlativo"));
                            pedido.DireccionId = reader.IsDBNull(reader.GetOrdinal("Direccion_Id")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("Direccion_Id"));
                            pedido.CUSTOMERORDERROWID = reader.IsDBNull(reader.GetOrdinal("CUSTOMER_ORDER_ROWID")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("CUSTOMER_ORDER_ROWID"));
                            pedido.CUSTOMERORDERID = reader.GetString(reader.GetOrdinal("CUSTOMER_ORDER_ID"));
                            pedido.Sincronizado = reader.GetBoolean(reader.GetOrdinal("Sincronizado"));
                            pedido.FechaHoraPedido = reader.IsDBNull(reader.GetOrdinal("Fecha_Hora_Pedido")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("Fecha_Hora_Pedido"));
                            pedido.FechaHoraUltimoIntento = reader.IsDBNull(reader.GetOrdinal("Fecha_Hora_Ultimo_Intento")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("Fecha_Hora_Ultimo_Intento"));
                            pedido.FechaHoraSincronizacion = reader.IsDBNull(reader.GetOrdinal("Fecha_Hora_Sincronizacion")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("Fecha_Hora_Sincronizacion"));
                            pedido.TipoPedidoId = reader.GetString(reader.GetOrdinal("Tipo_Pedido_Id"));
                            pedido.OrdenCompraCliente = reader.GetString(reader.GetOrdinal("Orden_Compra_Cliente"));
                            pedido.ObservacionesGenerales = reader.GetString(reader.GetOrdinal("Observaciones_Generales"));
                            pedido.FechaPrometida = reader.IsDBNull(reader.GetOrdinal("Fecha_Prometida")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("Fecha_Prometida"));
                            pedido.EstadoId = reader.IsDBNull(reader.GetOrdinal("Estado_Id")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("Estado_Id"));
                            pedido.ResponsableAprobacionId = reader.IsDBNull(reader.GetOrdinal("Responsable_Aprobacion_Id")) ? (long?)null : reader.GetInt64(reader.GetOrdinal("Responsable_Aprobacion_Id"));
                            pedido.ComentarioAprobacion = reader.GetString(reader.GetOrdinal("Comentario_Aprobacion"));
                            pedido.DocumentoOrdenCompraRespaldo = GetFotografiaApp(connectionString, pedido.PedidoId, 1);//reader.GetString(reader.GetOrdinal("Documento_Orden_Compra_Respaldo"));
                            pedido.TerminoEntrega = reader.GetString(reader.GetOrdinal("Termino_Entrega"));
                            pedido.Vendedor = reader.GetString(reader.GetOrdinal("Vendedor"));
                            pedido.ImpuestoTAX = reader.GetString(reader.GetOrdinal("Impuesto_TAX"));
                            pedido.Moneda = reader.GetString(reader.GetOrdinal("Moneda"));
                            pedido.DireccionEntrega = reader.GetString(reader.GetOrdinal("Direccion_Entrega"));
                            pedido.serie_sap = reader.GetString(reader.GetOrdinal("serie_sap"));
                            pedido.Anexo = GetFotografiaApp(connectionString, pedido.PedidoId, 2);
                            pedidos.Add(pedido);
                        }
                    }
                }
            }

            return pedidos;
        }
    }
}
