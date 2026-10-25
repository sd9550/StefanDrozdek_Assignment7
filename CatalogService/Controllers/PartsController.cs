using CatalogService.Data;
using CatalogService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CatalogService.Controllers;

[ApiController]
[Route("api/parts")]
public sealed class PartsController(CatalogDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Part>>> GetParts(CancellationToken cancellationToken)
    {
        var parts = await dbContext.Parts
            .AsNoTracking()
            .OrderBy(part => part.Id)
            .ToListAsync(cancellationToken);

        return Ok(parts);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Part>> GetPart(int id, CancellationToken cancellationToken)
    {
        var part = await dbContext.Parts
            .AsNoTracking()
            .FirstOrDefaultAsync(part => part.Id == id, cancellationToken);

        return part is null ? NotFound() : Ok(part);
    }

    [HttpPost]
    public async Task<ActionResult<Part>> CreatePart(
        Part part,
        CancellationToken cancellationToken)
    {
        var skuExists = await dbContext.Parts
            .AnyAsync(existingPart => existingPart.SKU == part.SKU, cancellationToken);
        if (skuExists)
        {
            return Conflict(new { message = $"A part with SKU '{part.SKU}' already exists." });
        }

        part.Id = 0;
        dbContext.Parts.Add(part);
        await dbContext.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(GetPart), new { id = part.Id }, part);
    }

    [HttpPatch("{id:int}/adjust-stock")]
    public async Task<ActionResult<Part>> AdjustStock(
        int id,
        AdjustStockRequest request,
        CancellationToken cancellationToken)
    {
        if (request.QuantityChange == 0)
        {
            return BadRequest(new { message = "QuantityChange must not be zero." });
        }

        var part = await dbContext.Parts
            .FirstOrDefaultAsync(existingPart => existingPart.Id == id, cancellationToken);
        if (part is null)
        {
            return NotFound();
        }

        var updatedQuantity = (long)part.QuantityInStock + request.QuantityChange;
        if (updatedQuantity < 0 || updatedQuantity > int.MaxValue)
        {
            return BadRequest(new
            {
                message = "The adjustment would result in an invalid stock quantity."
            });
        }

        part.QuantityInStock = (int)updatedQuantity;
        await dbContext.SaveChangesAsync(cancellationToken);

        return Ok(part);
    }
}
