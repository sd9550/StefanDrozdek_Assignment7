using System.ComponentModel.DataAnnotations;

namespace RepairService.Models;

public sealed class RepairLog
{
    public int Id { get; set; }

    public int ConsoleUnitId { get; set; }

    [Range(1, int.MaxValue)]
    public int PartId { get; set; }

    [Range(1, int.MaxValue)]
    public int QuantityUsed { get; set; }

    [StringLength(2000)]
    public string WorkNotes { get; set; } = string.Empty;

    public DateTime CompletedDate { get; set; } = DateTime.UtcNow;
}
