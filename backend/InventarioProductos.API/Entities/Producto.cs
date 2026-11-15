namespace InventarioProductos.API.Entities;

public class Producto
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public bool Estado { get; set; }
    public DateTime FechaCreacion { get; set; }

    public List<Lote> Lotes { get; set; } = new();
}