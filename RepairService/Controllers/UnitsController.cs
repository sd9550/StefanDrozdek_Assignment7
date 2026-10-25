using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RepairService.Data;
using RepairService.Models;
using RepairService.Services;
using System.Net;

namespace RepairService.Controllers;

[ApiController]
[Route("api/units")]
public sealed class UnitsController(RepairDbContext dbContext, CatalogClient catalogClient)
    : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<ConsoleUnit>> CreateUnit(
        ConsoleUnit unit,
        CancellationToken cancellationToken)
    {
        var serialTagExists = await dbContext.ConsoleUnits
            .AnyAsync(existingUnit => existingUnit.SerialTag == unit.SerialTag, cancellationToken);
        if (serialTagExists)
        {
            return Conflict(new { message = $"A unit with serial tag '{unit.SerialTag}' already exists." });
        }

        unit.Id = 0;
        dbContext.ConsoleUnits.Add(unit);
        await dbContext.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(GetUnit), new { id = unit.Id }, unit);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ConsoleUnit>>> GetUnits(
        CancellationToken cancellationToken)
    {
        var units = await dbContext.ConsoleUnits
            .AsNoTracking()
            .OrderBy(unit => unit.Id)
            .ToListAsync(cancellationToken);

        return Ok(units);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ConsoleUnit>> GetUnit(
        int id,
        CancellationToken cancellationToken)
    {
        var unit = await dbContext.ConsoleUnits
            .AsNoTracking()
            .FirstOrDefaultAsync(existingUnit => existingUnit.Id == id, cancellationToken);

        return unit is null ? NotFound() : Ok(unit);
    }

    [HttpPost("{id:int}/repair-log")]
    public async Task<ActionResult<RepairLog>> CreateRepairLog(
        int id,
        CreateRepairLogRequest request,
        CancellationToken cancellationToken)
    {
        var unit = await dbContext.ConsoleUnits
            .FirstOrDefaultAsync(existingUnit => existingUnit.Id == id, cancellationToken);
        if (unit is null)
        {
            return NotFound();
        }

        try
        {
            await catalogClient.DecrementStockAsync(
                request.PartId,
                request.QuantityUsed,
                cancellationToken);
        }
        catch (HttpRequestException exception)
        {
            if (exception.StatusCode == HttpStatusCode.BadRequest)
            {
                return Conflict(new { message = "There is not enough stock for this repair." });
            }

            if (exception.StatusCode == HttpStatusCode.NotFound)
            {
                return BadRequest(new { message = "The requested part does not exist in CatalogService." });
            }

            return StatusCode(
                StatusCodes.Status502BadGateway,
                new { message = "CatalogService could not update part stock." });
        }

        var repairLog = new RepairLog
        {
            ConsoleUnitId = unit.Id,
            PartId = request.PartId,
            QuantityUsed = request.QuantityUsed,
            WorkNotes = request.WorkNotes,
            CompletedDate = request.CompletedDate
        };

        dbContext.RepairLogs.Add(repairLog);
        await dbContext.SaveChangesAsync(cancellationToken);

        return StatusCode(StatusCodes.Status201Created, repairLog);
    }
}
