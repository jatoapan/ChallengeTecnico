using Microsoft.AspNetCore.Diagnostics;

namespace InventarioProductos.API.Common;

/// <summary>
/// Manejador global de errores (patrón IExceptionHandler de ASP.NET Core 8+).
/// El middleware oficial (app.UseExceptionHandler) captura la excepción y llama a este manejador,
/// que la traduce a una respuesta ProblemDetails con su código HTTP y el traceId de la petición.
/// Las reglas de negocio llevan su propio mensaje; los errores inesperados devuelven un texto genérico
/// y el detalle solo queda en el log (nunca se envía al cliente).
/// </summary>
public class ManejadorErroresGlobal : IExceptionHandler
{
    private readonly ILogger<ManejadorErroresGlobal> _logger;

    public ManejadorErroresGlobal(ILogger<ManejadorErroresGlobal> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception ex, CancellationToken cancellationToken)
    {
        var (status, titulo, detalle) = ex switch
        {
            ReglaNegocioException => (StatusCodes.Status400BadRequest, "Solicitud inválida", ex.Message),
            NoEncontradoException => (StatusCodes.Status404NotFound, "No encontrado", ex.Message),
            DemasiadosIntentosException => (StatusCodes.Status429TooManyRequests, "Demasiados intentos", ex.Message),
            _ => (StatusCodes.Status500InternalServerError, "Error interno", "Ocurrió un error inesperado. Intenta nuevamente.")
        };

        if (status == StatusCodes.Status500InternalServerError)
            _logger.LogError(ex, "Error no controlado en {Metodo} {Ruta}", context.Request.Method, context.Request.Path);
        else
            _logger.LogWarning("{Titulo}: {Detalle}", titulo, detalle);

        context.Response.StatusCode = status;
        await context.Response.WriteAsJsonAsync(ProblemaHttp.Crear(context, status, titulo, detalle), cancellationToken);

        // true: la excepción quedó manejada y la respuesta ya está escrita.
        return true;
    }
}
