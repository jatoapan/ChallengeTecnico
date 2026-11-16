namespace InventarioProductos.API.DTOs;

public class ProductoListadoDto
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public bool Estado { get; set; }
    public List<OfertaDto> Ofertas { get; set; } = new();
}