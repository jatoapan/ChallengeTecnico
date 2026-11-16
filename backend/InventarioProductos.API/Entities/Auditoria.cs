namespace InventarioProductos.API.Entities;

public class Auditoria
{
    public int Id { get; set; }
    public int? IdUsuario { get; set; }
    public string Usuario { get; set; } = string.Empty;
    public string? Rol { get; set; }
    public string Accion { get; set; } = string.Empty;
    public string Entidad { get; set; } = string.Empty;
    public int IdEntidad { get; set; }
    public string Detalle { get; set; } = string.Empty;
    public string? Ip { get; set; }
    public DateTime Fecha { get; set; }
}
