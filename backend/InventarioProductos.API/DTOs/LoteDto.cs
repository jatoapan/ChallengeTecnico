using System.ComponentModel.DataAnnotations;

namespace InventarioProductos.API.DTOs;

/// <summary>
/// Filtros de la lista de lotes (todos opcionales). Se reciben por query string.
/// </summary>
public class FiltroLotesDto
{
    /// <summary>Texto a buscar en el código o nombre del producto, el proveedor o el número de lote.</summary>
    public string? Buscar { get; set; }

    /// <summary>Solo los lotes de este proveedor.</summary>
    public int? IdProveedor { get; set; }

    /// <summary>Stock mínimo (incluido).</summary>
    [Range(0, int.MaxValue, ErrorMessage = "El stock mínimo no puede ser negativo.")]
    public int? StockMin { get; set; }

    /// <summary>Stock máximo (incluido).</summary>
    [Range(0, int.MaxValue, ErrorMessage = "El stock máximo no puede ser negativo.")]
    public int? StockMax { get; set; }

    /// <summary>Precio mínimo (incluido).</summary>
    [Range(0, 999999999, ErrorMessage = "El precio mínimo no puede ser negativo.")]
    public decimal? PrecioMin { get; set; }

    /// <summary>Precio máximo (incluido).</summary>
    [Range(0, 999999999, ErrorMessage = "El precio máximo no puede ser negativo.")]
    public decimal? PrecioMax { get; set; }
}

/// <summary>
/// Un lote en la lista plana de inventario: una fila por producto, proveedor y lote.
/// </summary>
public class LoteListadoDto
{
    public int IdLote { get; set; }
    public string NumeroLote { get; set; } = string.Empty;
    public int IdProducto { get; set; }
    public string CodigoProducto { get; set; } = string.Empty;
    public string Producto { get; set; } = string.Empty;
    public int IdProveedor { get; set; }
    public string Proveedor { get; set; } = string.Empty;
    public decimal Costo { get; set; }
    public decimal Precio { get; set; }
    public int Stock { get; set; }

    /// <summary>Última vez que se cambió el precio, costo o stock del lote.</summary>
    public DateTime FechaActualizacion { get; set; }
}

/// <summary>
/// Datos para agregar un lote (proveedor, número de lote, precio y stock) a un producto existente.
/// </summary>
public class LoteGuardarDto
{
    [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un proveedor.")]
    public int IdProveedor { get; set; }

    [Required(ErrorMessage = "El número de lote es obligatorio.")]
    [StringLength(20, ErrorMessage = "El número de lote no puede superar 20 caracteres.")]
    public string NumeroLote { get; set; } = string.Empty;

    [Range(0, 99999999.99, ErrorMessage = "El costo debe ser mayor o igual a 0.")]
    public decimal Costo { get; set; }

    [Range(0.01, 99999999.99, ErrorMessage = "El precio debe ser mayor a 0.")]
    public decimal Precio { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "El stock debe ser mayor o igual a 0.")]
    public int Stock { get; set; }
}
