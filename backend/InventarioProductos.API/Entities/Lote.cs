namespace InventarioProductos.API.Entities;

public class Lote
{
    public int Id { get; set; }
    public int IdProducto { get; set; }
    public int IdProveedor { get; set; }
    public string NumeroLote { get; set; } = string.Empty;
    public bool Estado { get; set; }
    public DateTime FechaCreacion { get; set; }

    public Producto Producto { get; set; } = null!;
    public Proveedor Proveedor { get; set; } = null!;
    public Inventario? Inventario { get; set; }
}