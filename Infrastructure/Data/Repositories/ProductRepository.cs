using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data.Repositories;

public class ProductRepository(ApplicationDbContext db) : IProductRepository
{
    public async Task<Product?> GetByIdAsync(int id, CancellationToken ct = default)
        => await db.Products.Include(x => x.Items).FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task AddAsync(Product entity, CancellationToken ct = default)
        => await db.Products.AddAsync(entity, ct);

    public void Update(Product entity) => db.Products.Update(entity);
    public void Delete(Product entity) => db.Products.Remove(entity);

    public async Task<IReadOnlyList<Product>> ListAsync(CancellationToken ct = default)
        => await db.Products.Include(x => x.Items).AsNoTracking().ToListAsync(ct);

    public async Task<(IReadOnlyList<Product> Items, int Total)> GetPagedAsync(int page, int pageSize, CancellationToken ct = default)
    {
        var query = db.Products.AsNoTracking().OrderBy(p => p.Id);
        var total = await query.CountAsync(ct);
        var items = await query.Skip((page - 1) * pageSize)
                               .Take(pageSize)
                               .Include(x => x.Items).ToListAsync(ct);

        return (items, total);
    }
}
