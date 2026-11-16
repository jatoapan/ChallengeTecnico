namespace InventarioProductos.API.Entities;

public class Usuario
{
    public int Id { get; set; }
    public string Nombres { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public string NombreUsuario { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public int IdRol { get; set; }
    public bool Estado { get; set; }
    public DateTime FechaCreacion { get; set; }

    public Rol Rol { get; set; } = null!;
}