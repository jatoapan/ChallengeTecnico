namespace InventarioProductos.API.Entities;

public class Rol
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public bool Estado { get; set; }
    public DateTime FechaCreacion { get; set; }

    public List<Usuario> Usuarios { get; set; } = new();
}
