using Microsoft.Extensions.Caching.Memory;

namespace InventarioProductos.API.Services;

// Guarda los intentos en IMemoryCache. Se registra como Singleton para que la cuenta
// se mantenga entre peticiones.
public class RegistroIntentosLoginMemoria : IRegistroIntentosLogin
{
    private readonly IMemoryCache _cache;
    private readonly int _maxIntentos;

    public int MinutosBloqueo { get; }

    public RegistroIntentosLoginMemoria(IMemoryCache cache, IConfiguration config)
    {
        _cache = cache;
        _maxIntentos = config.GetValue("Login:MaxIntentos", 3);
        MinutosBloqueo = config.GetValue("Login:MinutosBloqueo", 2);
    }

    public TimeSpan? TiempoBloqueoRestante(string usuario)
    {
        if (_cache.TryGetValue(Clave(usuario), out Intentos? intentos) && intentos!.BloqueadoHasta > DateTime.UtcNow)
            return intentos.BloqueadoHasta - DateTime.UtcNow;

        return null;
    }

    public bool RegistrarFallo(string usuario)
    {
        var intentos = _cache.GetOrCreate(Clave(usuario), _ => new Intentos())!;
        intentos.Fallidos++;

        var bloqueado = intentos.Fallidos >= _maxIntentos;
        if (bloqueado)
        {
            intentos.Fallidos = 0;
            intentos.BloqueadoHasta = DateTime.UtcNow.AddMinutes(MinutosBloqueo);
        }

        // Los intentos se olvidan si pasa el tiempo de bloqueo sin nuevos fallos.
        _cache.Set(Clave(usuario), intentos, TimeSpan.FromMinutes(MinutosBloqueo));
        return bloqueado;
    }

    public void Limpiar(string usuario) => _cache.Remove(Clave(usuario));

    // Sin distinguir mayúsculas, para que "Admin" y "admin" cuenten como el mismo usuario.
    private static string Clave(string usuario) => $"intentos-login:{usuario.Trim().ToLowerInvariant()}";

    private class Intentos
    {
        public int Fallidos { get; set; }
        public DateTime BloqueadoHasta { get; set; }
    }
}
