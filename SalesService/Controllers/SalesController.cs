using Microsoft.Data.Sqlite;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SalesService.Data;
using SalesService.Models;
using SalesService.Services;
using System.Net;

namespace SalesService.Controllers;

[ApiController]
[Route("api")]
public sealed class SalesController(SalesDbContext dbContext, RepairClient repairClient)
    : ControllerBase
{
    [HttpPost("listings")]
    public async Task<ActionResult<SaleListing>> CreateListing(
        CreateListingRequest request,
        CancellationToken cancellationToken)
    {
        var listing = new SaleListing
        {
            ConsoleUnitId = request.ConsoleUnitId,
            ListingPrice = request.ListingPrice,
            ListedDate = request.ListedDate
        };

        dbContext.SaleListings.Add(listing);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Created($"/api/listings/{listing.Id}", listing);
    }

    [HttpGet("listings/available")]
    public async Task<ActionResult<IEnumerable<SaleListing>>> GetAvailableListings(
        CancellationToken cancellationToken)
    {
        var listings = await dbContext.SaleListings
            .AsNoTracking()
            .Where(listing => !listing.IsSold)
            .OrderBy(listing => listing.Id)
            .ToListAsync(cancellationToken);

        return Ok(listings);
    }

    [HttpPost("orders")]
    public async Task<ActionResult<OrderResult>> CreateOrder(
        CreateOrderRequest request,
        CancellationToken cancellationToken)
    {
        if (request.PlatformFee > request.SoldPrice)
        {
            return BadRequest(new { message = "PlatformFee cannot exceed SoldPrice." });
        }

        var listing = await dbContext.SaleListings
            .FirstOrDefaultAsync(
                existingListing => existingListing.Id == request.ListingId,
                cancellationToken);
        if (listing is null)
        {
            return NotFound(new { message = "The requested listing does not exist." });
        }

        if (listing.IsSold)
        {
            return Conflict(new { message = "The listing has already been sold." });
        }

        decimal acquisitionCost;
        try
        {
            acquisitionCost = await repairClient.GetAcquisitionCostAsync(
                listing.ConsoleUnitId,
                cancellationToken);
        }
        catch (HttpRequestException exception)
        {
            if (exception.StatusCode == HttpStatusCode.NotFound)
            {
                return BadRequest(new
                {
                    message = "The listed console unit does not exist in RepairService."
                });
            }

            return StatusCode(
                StatusCodes.Status502BadGateway,
                new { message = "RepairService could not provide the unit acquisition cost." });
        }

        var order = new Order
        {
            ListingId = listing.Id,
            SoldPrice = request.SoldPrice,
            PlatformFee = request.PlatformFee,
            SoldDate = request.SoldDate,
            BuyerName = request.BuyerName
        };

        listing.IsSold = true;
        dbContext.Orders.Add(order);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (IsUniqueConstraintViolation(exception))
        {
            return Conflict(new { message = "The listing has already been sold." });
        }

        var netProfit = order.SoldPrice - order.PlatformFee - acquisitionCost;
        var result = new OrderResult
        {
            Order = order,
            AcquisitionCost = acquisitionCost,
            NetProfit = netProfit,
            ProfitMarginPercent = netProfit / order.SoldPrice * 100
        };

        return StatusCode(StatusCodes.Status201Created, result);
    }

    private static bool IsUniqueConstraintViolation(DbUpdateException exception)
    {
        return exception.InnerException is SqliteException
        {
            SqliteExtendedErrorCode: 2067
        };
    }
}
