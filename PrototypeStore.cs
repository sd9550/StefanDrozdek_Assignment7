using System.Net.Http.Json;

public class PrototypeStore
{
	private static async Task<int> Main()
	{
		using var catalog = CreateClient("CATALOG_SERVICE_URL", "http://localhost:5007/");
		using var repair = CreateClient("REPAIR_SERVICE_URL", "http://localhost:5086/");
		using var sales = CreateClient("SALES_SERVICE_URL", "http://localhost:5289/");

		var demoId = Guid.NewGuid().ToString("N");
		Console.WriteLine("Store services integration demo");

		var part = await PostAsync<PartResponse>(
			catalog,
			"api/parts",
			new
			{
				SKU = $"DEMO-{demoId}",
				Name = "Game Boy replacement screen",
				Category = "Repair part",
				UnitCost = 18.50m,
				QuantityInStock = 5
			},
			"CatalogService");
		Console.WriteLine($"CatalogService created part #{part.Id} ({part.Name}).");

		var unit = await PostAsync<ConsoleUnitResponse>(
			repair,
			"api/units",
			new
			{
				SerialTag = $"DEMO-{demoId}",
				AcquisitionCost = 45.00m,
				Status = "Received",
				AcquisitionDate = DateTime.UtcNow
			},
			"RepairService");
		Console.WriteLine($"RepairService registered unit #{unit.Id}.");

		await PostAsync<RepairLogResponse>(
			repair,
			$"api/units/{unit.Id}/repair-log",
			new
			{
				PartId = part.Id,
				QuantityUsed = 1,
				WorkNotes = "Replaced the screen.",
				CompletedDate = DateTime.UtcNow
			},
			"RepairService");
		Console.WriteLine("RepairService logged the repair and requested a stock adjustment from CatalogService.");

		var listing = await PostAsync<SaleListingResponse>(
			sales,
			"api/listings",
			new
			{
				ConsoleUnitId = unit.Id,
				ListingPrice = 100.00m,
				ListedDate = DateTime.UtcNow
			},
			"SalesService");
		Console.WriteLine($"SalesService created listing #{listing.Id}.");

		var order = await PostAsync<OrderResultResponse>(
			sales,
			"api/orders",
			new
			{
				ListingId = listing.Id,
				SoldPrice = 100.00m,
				PlatformFee = 10.00m,
				SoldDate = DateTime.UtcNow,
				BuyerName = "Demo buyer"
			},
			"SalesService");
		Console.WriteLine(
			$"SalesService confirmed order #{order.Order.Id}; acquisition cost: " +
			$"{order.AcquisitionCost:C}; net profit: {order.NetProfit:C} " +
			$"({order.ProfitMarginPercent:F2}%).");

		return 0;
	}

	private static HttpClient CreateClient(string environmentVariable, string defaultBaseUrl)
	{
		var baseUrl = Environment.GetEnvironmentVariable(environmentVariable) ?? defaultBaseUrl;
		if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out var baseUri)
			|| baseUri.Scheme is not "http" and not "https")
		{
			throw new InvalidOperationException(
				$"{environmentVariable} must be an absolute HTTP or HTTPS URL.");
		}

		return new HttpClient { BaseAddress = baseUri };
	}

	private static async Task<T> PostAsync<T>(
		HttpClient client,
		string requestUri,
		object content,
		string serviceName)
	{
		using var response = await client.PostAsJsonAsync(requestUri, content);
		if (!response.IsSuccessStatusCode)
		{
			var responseBody = await response.Content.ReadAsStringAsync();
			throw new HttpRequestException(
				$"{serviceName} returned HTTP {(int)response.StatusCode} " +
				$"({response.StatusCode}): {responseBody}",
				null,
				response.StatusCode);
		}

		return await response.Content.ReadFromJsonAsync<T>()
			?? throw new InvalidOperationException($"{serviceName} returned an empty response.");
	}

	private sealed record PartResponse(int Id, string Name);
	private sealed record ConsoleUnitResponse(int Id);
	private sealed record RepairLogResponse(int Id);
	private sealed record SaleListingResponse(int Id);
	private sealed record OrderResultResponse(
		OrderResponse Order,
		decimal AcquisitionCost,
		decimal NetProfit,
		decimal ProfitMarginPercent);
	private sealed record OrderResponse(int Id);
}
