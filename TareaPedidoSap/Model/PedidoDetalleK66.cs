
namespace TareaPedido.Model
{
    public class PedidoDetalleK66
    {
        public int DetalleId { get; set; }

        public long PedidoId { get; set; }

        public PedidoK66 Pedido { get; set; }

        public string ProductoId { get; set; }

        public string WarehouseId { get; set; }

        public string Nombre { get; set; }

        public string Unidad { get; set; }

        public decimal Cantidad { get; set; }

        public decimal PrecioOriginal { get; set; }

        public decimal Precio { get; set; }

        public decimal Descuento { get; set; }

        public bool PrecioCambiado { get; set; }

        public decimal Existencia { get; set; }
    }
}
