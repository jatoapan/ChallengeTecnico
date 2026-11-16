namespace InventarioProductos.API.Common;

/// <summary>
/// Se lanza cuando una cuenta está bloqueada por intentos fallidos. Se responde con 429.
/// </summary>
public class DemasiadosIntentosException : Exception
{
    public DemasiadosIntentosException(string mensaje) : base(mensaje) { }
}
