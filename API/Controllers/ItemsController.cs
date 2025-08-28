using API.Filters;
using Application.DTOs;
using Application.Services;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace API.Controllers;

[EnableRateLimiting("ApiThrottle")]
[ApiController]
[Authorize]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/products/{productId:int}/[controller]")]
public class ItemsController(IItemService service, ILogger<ItemsController> logger) : ControllerBase
{
    /// <summary>
    /// Gets all items tied to a product.
    /// </summary>
    /// <param name="productId">Product Id</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(int productId, CancellationToken ct = default)
    {
        var items = await service.GetByProductAsync(productId, ct);
        return Ok(items);
    }

    /// <summary>
    /// Get an item tied to a product
    /// </summary>
    /// <param name="productId">Product Id</param>
    /// <param name="itemId">Item Id</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns></returns>
    [HttpGet("{itemId:int}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(int productId, int itemId, CancellationToken ct = default)
        => Ok(await service.GetAsync(productId, itemId, ct));

    /// <summary>
    /// Create an item under a product
    /// </summary>
    /// <param name="productId">Product</param>
    /// <param name="itemDto">Item</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns></returns>
    [HttpPost]
    [IdempotentCreate("item")]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create(int productId, [FromBody] ItemCreateDto itemDto, CancellationToken ct = default)
    {
        logger.LogInformation("Creating item {@quantity} for product {product}", itemDto.Quantity, productId);
        var id = await service.CreateAsync(productId, itemDto, ct);
        var version = HttpContext.GetRequestedApiVersion()!.ToString();
        return CreatedAtAction(
            nameof(Get),
            new { productId, itemId = id, version },
            new { id }
        );
    }

    /// <summary>
    /// Deletes an item from a product
    /// </summary>
    /// <param name="productId">Product id</param>
    /// <param name="itemId">Item id</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns></returns>
    [HttpDelete("{itemId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(int productId, int itemId, CancellationToken ct = default)
    {
        logger.LogInformation("Deleting {productId} and his item {itemId}", productId, itemId);
        await service.DeleteAsync(productId, itemId, ct);
        return NoContent();
    }
}
