namespace InventarioProductos.API.Entities;

public class Inventario
{
    public int Id { get; set; }
    public int IdLote { get; set; }
    public decimal Costo { get; set; }
    public decimal Precio { get; set; }
    public int Stock { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime FechaActualizacion { get; set; }

    public Lote Lote { get; set; } = null!;
}