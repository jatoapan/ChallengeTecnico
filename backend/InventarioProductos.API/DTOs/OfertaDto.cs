namespace InventarioProductos.API.DTOs;

public class OfertaDto
{
    public int IdLote { get; set; }
    public string NumeroLote { get; set; } = string.Empty;
    public int IdProveedor { get; set; }
    public string Proveedor { get; set; } = string.Empty;
    public decimal Precio { get; set; }
    public int Stock { get; set; }
}