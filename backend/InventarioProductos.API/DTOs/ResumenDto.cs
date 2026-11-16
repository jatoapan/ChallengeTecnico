namespace InventarioProductos.API.DTOs;

/// <summary>
/// Estado general del inventario, últimos lotes y lotes que necesitan reposición.
/// </summary>
public class ResumenDto
{
    public int TotalProductos { get; set; }
    public int TotalProveedores { get; set; }

    /// <summary>Unidades disponibles: suma del stock de todos los lotes activos.</summary>
    public int StockTotal { get; set; }

    /// <summary>Lotes con stock crítico: stock menor o igual al umbral.</summary>
    public int LotesStockBajo { get; set; }

    /// <summary>Umbral usado: un lote tiene stock bajo si su stock es menor o igual a este valor.</summary>
    public int Umbral { get; set; }

    /// <summary>Lotes con stock bajo, del menor stock al mayor.</summary>
    public List<LotePorReponerDto> PorReponer { get; set; } = new();

    /// <summary>Últimos lotes modificados (creados o ajustados), del más reciente al más antiguo.</summary>
    public List<LotePorReponerDto> UltimosLotes { get; set; } = new();
}

public class LotePorReponerDto
{
    public int IdLote { get; set; }
    public string NumeroLote { get; set; } = string.Empty;
    public int IdProducto { get; set; }
    public string CodigoProducto { get; set; } = string.Empty;
    public string Producto { get; set; } = string.Empty;
    public string Proveedor { get; set; } = string.Empty;
    public decimal Precio { get; set; }
    public int Stock { get; set; }
    public DateTime FechaActualizacion { get; set; }
}
