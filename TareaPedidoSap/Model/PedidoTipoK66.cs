using System;

namespace TareaPedido.Model
{
    public class PedidoTipoK66
    {
        public Guid TipoId { get; set; }

        public long EmpresaId { get; set; }

        public string Nombre { get; set; }

        public string Descripcion { get; set; }

        public string CodigoIntregracion1 { get; set; }

        public string CodigoIntregracion2 { get; set; }

        public long ResponsableId { get; set; }  

        public DateTime Fecha { get; set; }      
    }
}
