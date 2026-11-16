using InventarioProductos.API.Data;
using InventarioProductos.API.DTOs;
using InventarioProductos.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace InventarioProductos.API.Services;

public class AuditoriaService : IAuditoriaService
{
    private readonly InventarioContext _context;
    private readonly IUsuarioActual _usuarioActual;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuditoriaService(InventarioContext context, IUsuarioActual usuarioActual, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _usuarioActual = usuarioActual;
        _httpContextAccessor = httpContextAccessor;
    }

    public void Registrar(string accion, string entidad, int idEntidad, string detalle)
    {
        _context.Auditorias.Add(Crear(accion, _usuarioActual.Id, _usuarioActual.Nombre, _usuarioActual.Rol, entidad, idEntidad, detalle));
    }

    public async Task RegistrarAccesoAsync(string accion, int? idUsuario, string usuario, string? rol, string detalle)
    {
        _context.Auditorias.Add(Crear(accion, idUsuario, usuario, rol, "Sesion", idUsuario ?? 0, detalle));
        await _context.SaveChangesAsync();
    }

    private Auditoria Crear(string accion, int? idUsuario, string usuario, string? rol, string entidad, int idEntidad, string detalle) => new()
    {
        IdUsuario = idUsuario,
        Rol = rol,
        // Las columnas tienen largo fijo: usuario 20 y detalle 500.
        Usuario = usuario.Length > 20 ? usuario[..20] : usuario,
        Accion = accion,
        Entidad = entidad,
        IdEntidad = idEntidad,
        Detalle = detalle.Length > 500 ? detalle[..500] : detalle,
        Ip = IpLegible(_httpContextAccessor.HttpContext?.Connection.RemoteIpAddress)
    };

    // "::1" (localhost en IPv6) y "::ffff:172.19.0.4" (IPv4 dentro de IPv6) se guardan como IPv4 legible.
    private static string? IpLegible(System.Net.IPAddress? ip)
    {
        if (ip == null) return null;
        if (System.Net.IPAddress.IsLoopback(ip)) return "127.0.0.1";
        return ip.IsIPv4MappedToIPv6 ? ip.MapToIPv4().ToString() : ip.ToString();
    }

    public async Task<PaginadoDto<AuditoriaDto>> ListarAsync(FiltroAuditoriaDto filtro, int pagina, int tamanoPagina)
    {
        pagina = Math.Max(pagina, 1);
        tamanoPagina = Math.Clamp(tamanoPagina, 1, 100);

        var query = Filtrar(filtro);
        var total = await query.CountAsync();

        var items = await Proyectar(query
                .OrderByDescending(a => a.Fecha).ThenByDescending(a => a.Id)
                .Skip((pagina - 1) * tamanoPagina)
                .Take(tamanoPagina))
            .ToListAsync();

        return new PaginadoDto<AuditoriaDto>
        {
            Items = items,
            Total = total,
            Pagina = pagina,
            TamanoPagina = tamanoPagina
        };
    }

    public async Task<EstadisticasAuditoriaDto> EstadisticasAsync(FiltroAuditoriaDto filtro)
    {
        // Un solo GROUP BY en SQL: no se traen los registros a memoria.
        var porAccion = await Filtrar(filtro)
            .GroupBy(a => a.Accion)
            .Select(g => new { Accion = g.Key, Cantidad = g.Count() })
            .ToDictionaryAsync(x => x.Accion, x => x.Cantidad);

        return new EstadisticasAuditoriaDto { Total = porAccion.Values.Sum(), PorAccion = porAccion };
    }

    private IQueryable<Auditoria> Filtrar(FiltroAuditoriaDto filtro)
    {
        var query = _context.Auditorias.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(filtro.Buscar))
        {
            var texto = filtro.Buscar.Trim();
            query = query.Where(a => a.Usuario.Contains(texto) || a.Detalle.Contains(texto));
        }
        if (!string.IsNullOrWhiteSpace(filtro.Accion))
            query = query.Where(a => a.Accion == filtro.Accion);
        if (!string.IsNullOrWhiteSpace(filtro.Entidad))
            query = query.Where(a => a.Entidad == filtro.Entidad);
        if (filtro.Desde is { } desde)
            query = query.Where(a => a.Fecha >= desde.ToDateTime(TimeOnly.MinValue));
        if (filtro.Hasta is { } hasta)
            query = query.Where(a => a.Fecha < hasta.AddDays(1).ToDateTime(TimeOnly.MinValue));

        return query;
    }

    private static IQueryable<AuditoriaDto> Proyectar(IQueryable<Auditoria> query) =>
        query.Select(a => new AuditoriaDto
        {
            Id = a.Id,
            Usuario = a.Usuario,
            Rol = a.Rol,
            Accion = a.Accion,
            Entidad = a.Entidad,
            IdEntidad = a.IdEntidad,
            Detalle = a.Detalle,
            Ip = a.Ip,
            Fecha = a.Fecha
        });
}
