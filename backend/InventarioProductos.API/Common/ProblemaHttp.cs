using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace InventarioProductos.API.Common;

/// <summary>
/// Crea las respuestas de error (ProblemDetails) con el traceId de la petición.
/// El mismo traceId aparece en los logs de Serilog, así un error del front se encuentra en el log del back.
/// </summary>
public static class ProblemaHttp
{
    public static ProblemDetails Crear(HttpContext contexto, int status, string titulo, string detalle)
    {
        var problema = new ProblemDetails
        {
            Status = status,
            Title = titulo,
            Detail = detalle
        };

        problema.Extensions["traceId"] = ObtenerTraceId(contexto);
        return problema;
    }

    public static string ObtenerTraceId(HttpContext contexto) =>
        Activity.Current?.TraceId.ToString() ?? contexto.TraceIdentifier;
}
