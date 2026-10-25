using System.ComponentModel.DataAnnotations;

namespace CatalogService.Models;

public sealed class Part
{
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    public string SKU { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string Category { get; set; } = string.Empty;

    [Range(typeof(decimal), "0", "99999999.99")]
    public decimal UnitCost { get; set; }

    [Range(0, int.MaxValue)]
    public int QuantityInStock { get; set; }
}
