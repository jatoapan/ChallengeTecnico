using InventarioProductos.API.Common;
using Microsoft.AspNetCore.Mvc;

namespace InventarioProductos.API.Middlewares;

public class ManejoErroresMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ManejoErroresMiddleware> _logger;

    public ManejoErroresMiddleware(RequestDelegate next, ILogger<ManejoErroresMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            var (status, titulo, detalle) = ex switch
            {
                ReglaNegocioException => (StatusCodes.Status400BadRequest, "Solicitud inválida", ex.Message),
                NoEncontradoException => (StatusCodes.Status404NotFound, "No encontrado", ex.Message),
                _ => (StatusCodes.Status500InternalServerError, "Error interno", "Ocurrió un error inesperado. Intenta nuevamente.")
            };

            if (status == StatusCodes.Status500InternalServerError)
                _logger.LogError(ex, "Error no controlado en {Metodo} {Ruta}", context.Request.Method, context.Request.Path);
            else
                _logger.LogWarning("{Titulo}: {Detalle}", titulo, detalle);

            context.Response.StatusCode = status;
            await context.Response.WriteAsJsonAsync(new ProblemDetails
            {
                Status = status,
                Title = titulo,
                Detail = detalle
            });
        }
    }
}