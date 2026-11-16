using System.ComponentModel.DataAnnotations;

namespace InventarioProductos.API.DTOs;

public class AjusteInventarioDto
{
    [Range(0, 99999999.99, ErrorMessage = "El costo debe ser mayor o igual a 0.")]
    public decimal Costo { get; set; }

    [Range(0.01, 99999999.99, ErrorMessage = "El precio debe ser mayor a 0.")]
    public decimal Precio { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "El stock debe ser mayor o igual a 0.")]
    public int Stock { get; set; }
}