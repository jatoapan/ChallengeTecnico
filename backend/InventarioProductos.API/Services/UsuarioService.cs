using InventarioProductos.API.Common;
using InventarioProductos.API.Data;
using InventarioProductos.API.DTOs;
using InventarioProductos.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace InventarioProductos.API.Services;

// Gestión de usuarios. Reglas para no dejar el sistema sin acceso:
// nadie puede desactivarse ni quitarse el rol de Administrador a sí mismo,
// y siempre debe quedar al menos un Administrador activo.
public class UsuarioService : IUsuarioService
{
    private const string RolAdministrador = "Administrador";

    private readonly InventarioContext _context;
    private readonly IAuditoriaService _auditoria;
    private readonly IUsuarioActual _usuarioActual;
    private readonly ILogger<UsuarioService> _logger;

    public UsuarioService(InventarioContext context, IAuditoriaService auditoria,
        IUsuarioActual usuarioActual, ILogger<UsuarioService> logger)
    {
        _context = context;
        _auditoria = auditoria;
        _usuarioActual = usuarioActual;
        _logger = logger;
    }

    public async Task<List<UsuarioDto>> ListarAsync(string? buscar, FiltroEstado estado = FiltroEstado.Activos)
    {
        var query = _context.Usuarios.AsNoTracking();
        query = estado switch
        {
            FiltroEstado.Inactivos => query.Where(u => !u.Estado),
            FiltroEstado.Todos => query,
            _ => query.Where(u => u.Estado)
        };

        if (!string.IsNullOrWhiteSpace(buscar))
        {
            var texto = buscar.Trim();
            query = query.Where(u => u.NombreUsuario.Contains(texto) || u.Nombres.Contains(texto) ||
                                     u.Apellidos.Contains(texto) || u.Email.Contains(texto));
        }

        return await Proyectar(query).OrderBy(u => u.Usuario).ToListAsync();
    }

    public async Task<List<RolDto>> ListarRolesAsync() =>
        await _context.Roles.AsNoTracking()
            .Where(r => r.Estado)
            .OrderBy(r => r.Nombre)
            .Select(r => new RolDto { Id = r.Id, Nombre = r.Nombre })
            .ToListAsync();

    public async Task<UsuarioDto> CrearAsync(UsuarioGuardarDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Password))
            throw new ReglaNegocioException("La contraseña es obligatoria para un usuario nuevo.");

        var (nombreUsuario, email) = (dto.Usuario.Trim().ToLower(), dto.Email.Trim().ToLower());
        await ValidarUnicosAsync(nombreUsuario, email);
        var rol = await ObtenerRolActivoAsync(dto.IdRol);

        var usuario = new Usuario
        {
            Nombres = dto.Nombres.Trim(),
            Apellidos = dto.Apellidos.Trim(),
            NombreUsuario = nombreUsuario,
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            IdRol = rol.Id,
            Estado = true
        };

        // El id se conoce después del primer guardado: transacción para que el usuario y su auditoría
        // queden juntos o no quede ninguno.
        await using var transaccion = await _context.Database.BeginTransactionAsync();

        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync();

        _auditoria.Registrar("Crear", "Usuario", usuario.Id, $"{nombreUsuario} ({rol.Nombre})");
        await _context.SaveChangesAsync();

        await transaccion.CommitAsync();

        _logger.LogInformation("Auditoría: {Usuario} creó el usuario {IdUsuario} ({NombreUsuario}, {Rol})",
            _usuarioActual.Nombre, usuario.Id, nombreUsuario, rol.Nombre);

        return await ObtenerAsync(usuario.Id);
    }

    public async Task<UsuarioDto> ActualizarAsync(int id, UsuarioGuardarDto dto)
    {
        var usuario = await _context.Usuarios.Include(u => u.Rol).FirstOrDefaultAsync(u => u.Id == id && u.Estado)
            ?? throw new NoEncontradoException("El usuario no existe.");

        var (nombreUsuario, email) = (dto.Usuario.Trim().ToLower(), dto.Email.Trim().ToLower());
        await ValidarUnicosAsync(nombreUsuario, email, id);
        var rolNuevo = await ObtenerRolActivoAsync(dto.IdRol);

        // Quitar el rol de Administrador: no a uno mismo y nunca al último Administrador activo.
        if (usuario.Rol.Nombre == RolAdministrador && rolNuevo.Nombre != RolAdministrador)
        {
            if (id == _usuarioActual.Id)
                throw new ReglaNegocioException("No puede quitarse a sí mismo el rol de Administrador.");
            await ValidarQueQuedeOtroAdministradorAsync(id);
        }

        // Detalle de auditoría con el antes y el después. La contraseña nunca se registra, solo que cambió.
        var cambios = new List<string>();
        if (usuario.NombreUsuario != nombreUsuario) cambios.Add($"usuario {usuario.NombreUsuario} -> {nombreUsuario}");
        if (usuario.Nombres != dto.Nombres.Trim() || usuario.Apellidos != dto.Apellidos.Trim()) cambios.Add("nombre actualizado");
        if (usuario.Email != email) cambios.Add($"correo {usuario.Email} -> {email}");
        if (usuario.IdRol != rolNuevo.Id) cambios.Add($"rol {usuario.Rol.Nombre} -> {rolNuevo.Nombre}");
        if (!string.IsNullOrWhiteSpace(dto.Password)) cambios.Add("contraseña restablecida");

        usuario.Nombres = dto.Nombres.Trim();
        usuario.Apellidos = dto.Apellidos.Trim();
        usuario.NombreUsuario = nombreUsuario;
        usuario.Email = email;
        usuario.IdRol = rolNuevo.Id;
        if (!string.IsNullOrWhiteSpace(dto.Password))
            usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

        var detalle = cambios.Count > 0 ? string.Join("; ", cambios) : "sin cambios";
        _auditoria.Registrar("Actualizar", "Usuario", id, $"{nombreUsuario}: {detalle}");
        await _context.SaveChangesAsync();

        _logger.LogInformation("Auditoría: {Usuario} actualizó el usuario {IdUsuario}: {Detalle}",
            _usuarioActual.Nombre, id, detalle);

        return await ObtenerAsync(id);
    }

    public async Task DesactivarAsync(int id)
    {
        if (id == _usuarioActual.Id)
            throw new ReglaNegocioException("No puede desactivar su propio usuario.");

        var usuario = await _context.Usuarios.Include(u => u.Rol).FirstOrDefaultAsync(u => u.Id == id && u.Estado)
            ?? throw new NoEncontradoException("El usuario no existe.");

        if (usuario.Rol.Nombre == RolAdministrador)
            await ValidarQueQuedeOtroAdministradorAsync(id);

        usuario.Estado = false;
        _auditoria.Registrar("Eliminar", "Usuario", id, $"{usuario.NombreUsuario} desactivado");
        await _context.SaveChangesAsync();

        _logger.LogInformation("Auditoría: {Usuario} desactivó el usuario {IdUsuario} ({NombreUsuario})",
            _usuarioActual.Nombre, id, usuario.NombreUsuario);
    }

    public async Task<UsuarioDto> ReactivarAsync(int id)
    {
        var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Id == id)
            ?? throw new NoEncontradoException("El usuario no existe.");

        if (usuario.Estado)
            throw new ReglaNegocioException("El usuario ya está activo.");

        usuario.Estado = true;
        _auditoria.Registrar("Reactivar", "Usuario", id, $"{usuario.NombreUsuario} reactivado");
        await _context.SaveChangesAsync();

        _logger.LogInformation("Auditoría: {Usuario} reactivó el usuario {IdUsuario} ({NombreUsuario})",
            _usuarioActual.Nombre, id, usuario.NombreUsuario);

        return await ObtenerAsync(id);
    }

    private async Task ValidarUnicosAsync(string nombreUsuario, string email, int? idActual = null)
    {
        if (await _context.Usuarios.AnyAsync(u => u.NombreUsuario == nombreUsuario && u.Id != idActual))
            throw new ReglaNegocioException("Ya existe un usuario con ese nombre de usuario.");
        if (await _context.Usuarios.AnyAsync(u => u.Email == email && u.Id != idActual))
            throw new ReglaNegocioException("Ya existe un usuario con ese correo.");
    }

    private async Task<Rol> ObtenerRolActivoAsync(int idRol) =>
        await _context.Roles.AsNoTracking().FirstOrDefaultAsync(r => r.Id == idRol && r.Estado)
        ?? throw new ReglaNegocioException("El rol no existe o está inactivo.");

    private async Task ValidarQueQuedeOtroAdministradorAsync(int idExcluido)
    {
        var otros = await _context.Usuarios.CountAsync(u =>
            u.Id != idExcluido && u.Estado && u.Rol.Nombre == RolAdministrador);
        if (otros == 0)
            throw new ReglaNegocioException("Debe quedar al menos un Administrador activo.");
    }

    private async Task<UsuarioDto> ObtenerAsync(int id) =>
        await Proyectar(_context.Usuarios.AsNoTracking().Where(u => u.Id == id)).FirstAsync();

    private static IQueryable<UsuarioDto> Proyectar(IQueryable<Usuario> query) =>
        query.Select(u => new UsuarioDto
        {
            Id = u.Id,
            Nombres = u.Nombres,
            Apellidos = u.Apellidos,
            Usuario = u.NombreUsuario,
            Email = u.Email,
            IdRol = u.IdRol,
            Rol = u.Rol.Nombre,
            Estado = u.Estado,
            FechaCreacion = u.FechaCreacion
        });
}
