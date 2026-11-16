using System.ComponentModel.DataAnnotations;

namespace InventarioProductos.API.DTOs;

/// <summary>
/// Datos del catálogo de un producto. Los lotes (proveedor, precio, stock) se gestionan aparte:
/// agregar lote, desactivar lote y ajustar inventario.
/// </summary>
public class ProductoActualizarDto
{
    [Required(ErrorMessage = "El código es obligatorio.")]
    [StringLength(10, ErrorMessage = "El código no puede superar 10 caracteres.")]
    public string Codigo { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(60, ErrorMessage = "El nombre no puede superar 60 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(255, ErrorMessage = "La descripción no puede superar 255 caracteres.")]
    public string? Descripcion { get; set; }
}
