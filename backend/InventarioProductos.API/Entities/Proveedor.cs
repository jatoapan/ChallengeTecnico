namespace InventarioProductos.API.Entities;

public class Proveedor
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Celular { get; set; }
    public bool Estado { get; set; }
    public DateTime FechaCreacion { get; set; }

    public List<Lote> Lotes { get; set; } = new();
}