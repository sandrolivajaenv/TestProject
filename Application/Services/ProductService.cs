using Application.DTOs;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;
using Domain.Exceptions;

namespace Application.Services;

public class ProductService(
    IProductRepository repo,
    IUnitOfWork uow,
    IMapper mapper) : IProductService
{
    public async Task<(IEnumerable<ProductReadDto> Items, int Total)> GetPagedAsync(int page, int pageSize, CancellationToken ct = default)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 200);

        var (items, total) = await repo.GetPagedAsync(page, pageSize, ct);
        return (mapper.Map<IEnumerable<ProductReadDto>>(items), total);
    }

    public async Task<ProductReadDto> GetAsync(int id, CancellationToken ct = default)
    {
        var entity = await repo.GetByIdAsync(id, ct);

        return entity is null 
            ? throw new NotFound(nameof(Product), id) 
            : mapper.Map<ProductReadDto>(entity);
    }

    public async Task<int> CreateAsync(ProductCreateDto dto, string createdBy, CancellationToken ct = default)
    {
        var entity = mapper.Map<Product>(dto);
        entity.CreatedBy = createdBy;
        entity.CreatedOn = DateTime.UtcNow;

        await repo.AddAsync(entity, ct);
        await uow.SaveChangesAsync(ct);

        return entity.Id;
    }

    public async Task UpdateAsync(int id, ProductUpdateDto dto, string modifiedBy, CancellationToken ct = default)
    {
        var entity = await repo.GetByIdAsync(id, ct) ?? throw new NotFound(nameof(Product), id);
        entity.ProductName = dto.ProductName;
        entity.ModifiedBy = modifiedBy;
        entity.ModifiedOn = DateTime.UtcNow;

        repo.Update(entity);
        await uow.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var entity = await repo.GetByIdAsync(id, ct);
        if (entity is null) 
            return; // this is idempotent

        repo.Delete(entity);
        await uow.SaveChangesAsync(ct);
    }
}
