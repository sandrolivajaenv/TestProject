using Application.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data.Repositories;

public class ItemRepository(ApplicationDbContext db) : IItemRepository
{
    public async Task<IReadOnlyList<Item>> ListByProductAsync(int productId, CancellationToken ct = default) =>
        await db.Items.AsNoTracking()
            .Where(i => i.ProductId == productId)
            .OrderBy(i => i.Id)
            .ToListAsync(ct);

    public async Task<Item?> GetAsync(int productId, int id, CancellationToken ct = default) =>
        await db.Items.AsNoTracking()
            .FirstOrDefaultAsync(i => i.ProductId == productId && i.Id == id, ct);

    public async Task AddAsync(Item entity, CancellationToken ct = default) =>
        await db.Items.AddAsync(entity, ct);

    public void Delete(Item entity) => db.Items.Remove(entity);
}