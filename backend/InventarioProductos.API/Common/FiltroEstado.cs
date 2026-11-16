namespace InventarioProductos.API.Common;

/// <summary>
/// Qué registros listar según su estado (borrado lógico). Se recibe por query string: ?estado=Inactivos
/// </summary>
public enum FiltroEstado
{
    Activos,
    Inactivos,
    Todos
}
