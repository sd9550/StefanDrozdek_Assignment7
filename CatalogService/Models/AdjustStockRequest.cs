using System.ComponentModel.DataAnnotations;

namespace CatalogService.Models;

public sealed class AdjustStockRequest
{
    [Range(int.MinValue, int.MaxValue)]
    public int QuantityChange { get; set; }
}
