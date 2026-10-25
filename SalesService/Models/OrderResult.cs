namespace SalesService.Models;

public sealed class OrderResult
{
    public required Order Order { get; init; }

    public decimal AcquisitionCost { get; init; }

    public decimal NetProfit { get; init; }

    public decimal ProfitMarginPercent { get; init; }
}
