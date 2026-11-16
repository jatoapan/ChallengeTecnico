using InventarioProductos.API.DTOs;

namespace InventarioProductos.API.Services;

public interface IAuditoriaService
{
    /// <summary>
    /// Agrega el registro al contexto sin guardar. Se guarda en el mismo SaveChangesAsync
    /// que el cambio auditado, así ambos se confirman o fallan juntos.
    /// </summary>
    void Registrar(string accion, string entidad, int idEntidad, string detalle);

    /// <summary>
    /// Registra un acceso (Ingreso, IngresoFallido, Bloqueo) y lo guarda de inmediato.
    /// Se usa en el login, donde todavía no hay un usuario autenticado en la petición.
    /// </summary>
    Task RegistrarAccesoAsync(string accion, int? idUsuario, string usuario, string? rol, string detalle);

    Task<PaginadoDto<AuditoriaDto>> ListarAsync(FiltroAuditoriaDto filtro, int pagina, int tamanoPagina);
    Task<EstadisticasAuditoriaDto> EstadisticasAsync(FiltroAuditoriaDto filtro);
}
