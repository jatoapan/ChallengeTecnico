using System.ComponentModel.DataAnnotations;

namespace InventarioProductos.API.DTOs;

public class LoginDto
{
    [Required(ErrorMessage = "El usuario es obligatorio.")]
    [StringLength(20, ErrorMessage = "El usuario no puede superar 20 caracteres.")]
    public string Usuario { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    public string Password { get; set; } = string.Empty;
}