using System.ComponentModel.DataAnnotations;

namespace InventarioProductos.API.DTOs;

/// <summary>
/// Usuario del sistema. Nunca incluye la contraseña ni su hash.
/// </summary>
public class UsuarioDto
{
    public int Id { get; set; }
    public string Nombres { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public string Usuario { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int IdRol { get; set; }
    public string Rol { get; set; } = string.Empty;
    public bool Estado { get; set; }
    public DateTime FechaCreacion { get; set; }
}

public class RolDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
}

/// <summary>
/// Datos para crear o actualizar un usuario. Al crear, la contraseña es obligatoria; al actualizar es
/// opcional y solo se cambia si se envía.
/// </summary>
public class UsuarioGuardarDto
{
    [Required(ErrorMessage = "Los nombres son obligatorios.")]
    [StringLength(60, ErrorMessage = "Los nombres no pueden superar 60 caracteres.")]
    public string Nombres { get; set; } = string.Empty;

    [Required(ErrorMessage = "Los apellidos son obligatorios.")]
    [StringLength(60, ErrorMessage = "Los apellidos no pueden superar 60 caracteres.")]
    public string Apellidos { get; set; } = string.Empty;

    [Required(ErrorMessage = "El usuario es obligatorio.")]
    [StringLength(20, MinimumLength = 3, ErrorMessage = "El usuario debe tener entre 3 y 20 caracteres.")]
    [RegularExpression(@"^[a-zA-Z0-9._-]+$", ErrorMessage = "El usuario solo admite letras, números, punto, guion y guion bajo.")]
    public string Usuario { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo es obligatorio.")]
    [EmailAddress(ErrorMessage = "El correo no tiene un formato válido.")]
    [StringLength(50, ErrorMessage = "El correo no puede superar 50 caracteres.")]
    public string Email { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un rol.")]
    public int IdRol { get; set; }

    // Política de contraseña: mínimo 8 caracteres con mayúscula, minúscula, número y símbolo.
    [StringLength(100, MinimumLength = 8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres.")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z\d]).+$",
        ErrorMessage = "La contraseña debe incluir mayúscula, minúscula, número y símbolo.")]
    public string? Password { get; set; }
}
