using System.ComponentModel.DataAnnotations;

namespace InventarioProductos.API.DTOs;

public class ProductoGuardarDto
{
    [Required(ErrorMessage = "El código es obligatorio.")]
    [StringLength(10, ErrorMessage = "El código no puede superar 10 caracteres.")]
    public string Codigo { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(60, ErrorMessage = "El nombre no puede superar 60 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(255, ErrorMessage = "La descripción no puede superar 255 caracteres.")]
    public string? Descripcion { get; set; }

    [MinLength(1, ErrorMessage = "Debe registrar al menos un proveedor con su lote.")]
    public List<OfertaGuardarDto> Ofertas { get; set; } = new();
}