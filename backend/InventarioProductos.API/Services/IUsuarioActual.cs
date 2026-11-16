namespace InventarioProductos.API.Services;

/// <summary>
/// Datos del usuario autenticado en la petición actual.
/// </summary>
public interface IUsuarioActual
{
    int Id { get; }
    string Nombre { get; }
    string? Rol { get; }
}
