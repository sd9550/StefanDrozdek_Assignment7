using System.ComponentModel.DataAnnotations;

namespace SalesService.Models;

public sealed class Order
{
    public int Id { get; set; }

    [Range(1, int.MaxValue)]
    public int ListingId { get; set; }

    [Range(typeof(decimal), "0.01", "99999999.99")]
    public decimal SoldPrice { get; set; }

    [Range(typeof(decimal), "0", "99999999.99")]
    public decimal PlatformFee { get; set; }

    public DateTime SoldDate { get; set; } = DateTime.UtcNow;

    [Required]
    [StringLength(150)]
    public string BuyerName { get; set; } = string.Empty;
}
