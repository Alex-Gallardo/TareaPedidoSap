using System;
using System.Collections.Generic;

namespace TareaPedido.Model
{
    public class PedidoK66
    {
        public long PedidoId { get; set; }

        public long EmpresaId { get; set; }

        public string TipoPedidoId { get; set; }

        public string serie_sap { get; set; }
        public string Anexo { get; set; }

        public int? EstadoId { get; set; }

        public int? CUSTOMERORDERROWID { get; set; }

        public string CUSTOMERORDERID { get; set; }

        public string IDK66 { get; set; }

        public string Nit { get; set; }

        public string Nombre { get; set; }

        public int? DireccionId { get; set; }

        public string Direccion { get; set; }

        public string DireccionEntrega { get; set; }

        public string OrdenCompraCliente { get; set; }

        public string ObservacionesGenerales { get; set; }

        public string ComentarioAprobacion { get; set; }

        public string DocumentoOrdenCompraRespaldo { get; set; }

        public string TerminoEntrega { get; set; }

        public string Vendedor { get; set; }

        public string ImpuestoTAX { get; set; }

        public string Moneda { get; set; }

        public long ResponsableId { get; set; }

        public long? ResponsableAprobacionId { get; set; }


        public bool Sincronizado { get; set; }

        public DateTime? FechaHoraPedido { get; set; }

        public DateTime? FechaHoraUltimoIntento { get; set; }

        public DateTime? FechaHoraSincronizacion { get; set; }

        public DateTime? FechaPrometida { get; set; }

        public DateTime Fecha { get; set; }

        public int Correlativo { get; set; }

        public List<PedidoDetalleK66> Detalles { get; set; }

        public List<PedidoDocumentoImportanteK66> DImportantes { get; set; }

        public string EstadoK66 { get; set; }

    }
}