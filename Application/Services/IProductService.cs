using Application.DTOs;


namespace Application.Services;

public interface IProductService
{
    Task<(IEnumerable<ProductReadDto> Items, int Total)> GetPagedAsync(int page, int pageSize, CancellationToken ct = default);
    Task<ProductReadDto> GetAsync(int id, CancellationToken ct = default);
    Task<int> CreateAsync(ProductCreateDto dto, string createdBy, CancellationToken ct = default);
    Task UpdateAsync(int id, ProductUpdateDto dto, string modifiedBy, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}