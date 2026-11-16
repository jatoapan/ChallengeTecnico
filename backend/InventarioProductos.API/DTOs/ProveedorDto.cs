using System.ComponentModel.DataAnnotations;

namespace InventarioProductos.API.DTOs;

/// <summary>
/// Proveedor activo con sus datos de contacto y cuántos lotes activos tiene.
/// </summary>
public class ProveedorDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Celular { get; set; }
    public int LotesActivos { get; set; }
    public bool Estado { get; set; }
}

/// <summary>
/// Datos para crear o actualizar un proveedor.
/// </summary>
public class ProveedorGuardarDto
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(60, ErrorMessage = "El nombre no puede superar 60 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo es obligatorio.")]
    [EmailAddress(ErrorMessage = "El correo no tiene un formato válido.")]
    [StringLength(50, ErrorMessage = "El correo no puede superar 50 caracteres.")]
    public string Email { get; set; } = string.Empty;

    [RegularExpression(@"^\d{10}$", ErrorMessage = "El celular debe tener 10 dígitos.")]
    public string? Celular { get; set; }
}
