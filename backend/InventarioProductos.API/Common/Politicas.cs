namespace InventarioProductos.API.Common;

/// <summary>
/// Nombres de las políticas de autorización. Cada una representa una acción del negocio;
/// qué roles la tienen se define en Program.cs.
/// </summary>
public static class Politicas
{
    public const string GestionarProductos = "GestionarProductos";
    public const string AjustarInventario = "AjustarInventario";
    public const string VerAuditoria = "VerAuditoria";
    public const string GestionarProveedores = "GestionarProveedores";
    public const string GestionarUsuarios = "GestionarUsuarios";
}
