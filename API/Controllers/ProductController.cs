using API.Filters;
using Application.DTOs;
using Application.Services;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace API.Controllers;

[EnableRateLimiting("ApiThrottle")]
[Authorize]
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class ProductsController(IProductService service, ILogger<ProductsController> logger) : ControllerBase
{
    /// <summary>
    /// Get a paged list of products
    /// </summary>
    /// <param name="page">Page number</param>
    /// <param name="pageSize">How many results you want returned</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get([FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var (items, total) = await service.GetPagedAsync(page, pageSize, ct);
        return Ok(new { page, pageSize, total, items });
    }

    /// <summary>
    /// Gets a single product details
    /// </summary>
    /// <param name="id">Product id</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns></returns>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ProductReadDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct = default)
        => Ok(await service.GetAsync(id, ct));

    /// <summary>
    /// Creates a new product
    /// </summary>
    /// <param name="dto">Product information</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>A new product</returns>
    [HttpPost]
    [IdempotentCreate("product")]
    [ProducesResponseType(typeof(ProductReadDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] ProductCreateDto dto, CancellationToken ct = default)
    {
        var createdBy = User.Identity?.Name ?? "system";
        logger.LogInformation("Creating product {@quantity} by user {user}", dto.ProductName, createdBy);
        var id = await service.CreateAsync(dto, createdBy, ct);

        var version = HttpContext.GetRequestedApiVersion()!.ToString();
        return CreatedAtAction(nameof(GetById), new { id, version }, new { id });
    }

    /// <summary>
    /// Update an existing product
    /// </summary>
    /// <param name="id">ProductId</param>
    /// <param name="dto">ProductDetails</param>
    /// <param name="ct">CancellationToken</param>
    /// <returns></returns>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(int id, [FromBody] ProductUpdateDto dto, CancellationToken ct = default)
    {
        // var modifiedBy = User.Identity?.Name ?? "system";
        await service.UpdateAsync(id, dto, modifiedBy: "system", ct);
        return NoContent();
    }

    /// <summary>
    /// Delete a product
    /// </summary>
    /// <param name="id">Product Id</param>
    /// <param name="ct">Cancellation Token</param>
    /// <returns></returns>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct = default)
    {
        await service.DeleteAsync(id, ct);
        return NoContent();
    }
}