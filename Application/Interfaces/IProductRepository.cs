using Domain.Entities;

namespace Application.Interfaces;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(int id, CancellationToken ct = default);
    Task AddAsync(Product entity, CancellationToken ct = default);
    void Update(Product entity);
    void Delete(Product entity);
    Task<IReadOnlyList<Product>> ListAsync(CancellationToken ct = default);
    Task<(IReadOnlyList<Product> Items, int Total)> GetPagedAsync(int page, int pageSize, CancellationToken ct = default);
}