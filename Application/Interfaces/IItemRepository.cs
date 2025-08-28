using Domain.Entities;

namespace Application.Interfaces;

public interface IItemRepository
{
    /// <summary>
    /// Get all items that belong to a product.
    /// </summary>
    Task<IReadOnlyList<Item>> ListByProductAsync(int productId, CancellationToken ct = default);

    /// <summary>
    /// Get a single item by its Id and productId.
    /// </summary>
    Task<Item?> GetAsync(int productId, int id, CancellationToken ct = default);

    /// <summary>
    /// Add a new item to the database.
    /// </summary>
    Task AddAsync(Item entity, CancellationToken ct = default);

    /// <summary>
    /// Delete an item from the database.
    /// </summary>
    void Delete(Item entity);
}
