namespace InventarioProductos.API.DTOs;

/// <summary>
/// Matriz producto × proveedor. Hereda la paginación; las filas son los productos
/// y las columnas, los proveedores activos.
/// </summary>
public class MatrizPreciosDto : PaginadoDto<FilaMatrizPreciosDto>
{
    public List<ProveedorDto> Proveedores { get; set; } = new();
}

/// <summary>
/// Un producto con su oferta por proveedor. Clave: id del proveedor. Si el proveedor no tiene oferta,
/// no aparece la clave.
/// </summary>
public class FilaMatrizPreciosDto
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public Dictionary<int, PrecioProveedorDto> Precios { get; set; } = new();
}

/// <summary>
/// Precio de un proveedor para un producto: el menor entre sus lotes con stock (o el menor de todos si
/// ninguno tiene stock) y el stock total de esos lotes.
/// </summary>
public class PrecioProveedorDto
{
    public decimal Precio { get; set; }
    public int Stock { get; set; }
}
