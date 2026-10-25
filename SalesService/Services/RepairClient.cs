using System.Net.Http.Json;

namespace SalesService.Services;

public sealed class RepairClient(HttpClient httpClient)
{
    public async Task<decimal> GetAcquisitionCostAsync(
        int consoleUnitId,
        CancellationToken cancellationToken)
    {
        using var response = await httpClient.GetAsync(
            $"api/units/{consoleUnitId}",
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"RepairService returned HTTP {(int)response.StatusCode}.",
                null,
                response.StatusCode);
        }

        var unit = await response.Content.ReadFromJsonAsync<ConsoleUnitResponse>(
            cancellationToken: cancellationToken);
        if (unit is null)
        {
            throw new HttpRequestException("RepairService returned an empty unit response.");
        }

        return unit.AcquisitionCost;
    }

    private sealed record ConsoleUnitResponse(decimal AcquisitionCost);
}
