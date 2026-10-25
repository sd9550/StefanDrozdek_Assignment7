using System.Net;
using System.Net.Http.Json;

namespace RepairService.Services;

public sealed class CatalogClient(HttpClient httpClient)
{
    public async Task DecrementStockAsync(
        int partId,
        int quantity,
        CancellationToken cancellationToken)
    {
        using var response = await httpClient.PatchAsJsonAsync(
            $"api/parts/{partId}/adjust-stock",
            new AdjustStockRequest(-quantity),
            cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            return;
        }

        throw new HttpRequestException(
            $"CatalogService returned HTTP {(int)response.StatusCode}.",
            null,
            response.StatusCode);
    }

    private sealed record AdjustStockRequest(int QuantityChange);
}
