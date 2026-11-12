
namespace TareaPedido.Model
{
    public class PedidoDocumentoImportanteK66
    {
        public int DocumentoId { get; set; }

        public long PedidoId { get; set; }

        public PedidoK66 Pedido { get; set; }

        public string Nombre { get; set; }

        public string FotografiaApp { get; set; }
    }
}