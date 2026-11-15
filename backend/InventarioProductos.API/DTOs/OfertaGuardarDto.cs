using System.ComponentModel.DataAnnotations;

namespace InventarioProductos.API.DTOs;

public class OfertaGuardarDto
{
    public int? IdLote { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un proveedor.")]
    public int IdProveedor { get; set; }

    [Required(ErrorMessage = "El número de lote es obligatorio.")]
    [StringLength(20, ErrorMessage = "El número de lote no puede superar 20 caracteres.")]
    public string NumeroLote { get; set; } = string.Empty;

    [Range(0, 99999999.99, ErrorMessage = "El precio debe ser mayor o igual a 0.")]
    public decimal Precio { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "El stock debe ser mayor o igual a 0.")]
    public int Stock { get; set; }
}