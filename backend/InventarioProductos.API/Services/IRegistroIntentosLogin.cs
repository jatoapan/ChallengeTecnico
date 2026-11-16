namespace InventarioProductos.API.Services;

/// <summary>
/// Lleva la cuenta de intentos de login fallidos por usuario.
/// La implementación actual guarda los datos en memoria; en producción con varias instancias
/// se cambiaría por una que use Redis o la base de datos, sin tocar AuthService.
/// </summary>
public interface IRegistroIntentosLogin
{
    /// <summary>Tiempo que le queda de bloqueo al usuario, o null si no está bloqueado.</summary>
    TimeSpan? TiempoBloqueoRestante(string usuario);

    /// <summary>Suma un intento fallido. Devuelve true si con este intento la cuenta quedó bloqueada.</summary>
    bool RegistrarFallo(string usuario);

    /// <summary>Borra los intentos fallidos después de un login correcto.</summary>
    void Limpiar(string usuario);

    int MinutosBloqueo { get; }
}
