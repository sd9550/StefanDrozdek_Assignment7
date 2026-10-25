using System.ComponentModel.DataAnnotations;

namespace RepairService.Models;

public sealed class ConsoleUnit
{
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    public string SerialTag { get; set; } = string.Empty;

    [Range(typeof(decimal), "0", "99999999.99")]
    public decimal AcquisitionCost { get; set; }

    [Required]
    [StringLength(50)]
    public string Status { get; set; } = "Received";

    public DateTime AcquisitionDate { get; set; } = DateTime.UtcNow;
}
