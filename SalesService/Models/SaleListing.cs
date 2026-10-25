using System.ComponentModel.DataAnnotations;

namespace SalesService.Models;

public sealed class SaleListing
{
    public int Id { get; set; }

    [Range(1, int.MaxValue)]
    public int ConsoleUnitId { get; set; }

    [Range(typeof(decimal), "0.01", "99999999.99")]
    public decimal ListingPrice { get; set; }

    public bool IsSold { get; set; }

    public DateTime ListedDate { get; set; } = DateTime.UtcNow;
}
