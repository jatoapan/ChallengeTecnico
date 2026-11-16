namespace InventarioProductos.API.DTOs;

public class AuditoriaDto
{
    public int Id { get; set; }
    public string Usuario { get; set; } = string.Empty;
    public string? Rol { get; set; }
    public string Accion { get; set; } = string.Empty;
    public string Entidad { get; set; } = string.Empty;
    public int IdEntidad { get; set; }
    public string Detalle { get; set; } = string.Empty;
    public string? Ip { get; set; }
    public DateTime Fecha { get; set; }
}

/// <summary>
/// Filtros de la auditoría (todos opcionales). Se reciben por query string.
/// </summary>
public class FiltroAuditoriaDto
{
    /// <summary>Texto a buscar en el usuario o el detalle.</summary>
    public string? Buscar { get; set; }

    /// <summary>Acción exacta: Crear, Actualizar, Eliminar, Ajustar, Reactivar, Ingreso, IngresoFallido o Bloqueo.</summary>
    public string? Accion { get; set; }

    /// <summary>Entidad: Producto, Lote, Proveedor o Sesion.</summary>
    public string? Entidad { get; set; }

    /// <summary>Desde esta fecha (incluida).</summary>
    public DateOnly? Desde { get; set; }

    /// <summary>Hasta esta fecha (incluida).</summary>
    public DateOnly? Hasta { get; set; }
}

/// <summary>
/// Cantidad de registros por acción para los filtros aplicados.
/// </summary>
public class EstadisticasAuditoriaDto
{
    public int Total { get; set; }
    public Dictionary<string, int> PorAccion { get; set; } = new();
}
