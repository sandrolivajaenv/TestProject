using Application.DTOs;

namespace Application.Services
{
    public interface IItemService
    {
        Task<IEnumerable<ItemReadDto>> GetByProductAsync(int productId, CancellationToken ct = default);
        Task<ItemReadDto> GetAsync(int itemId, int productId, CancellationToken ct = default);
        Task<int> CreateAsync(int productId, ItemCreateDto itemCreateDto, CancellationToken ct = default);
        Task DeleteAsync(int itemId, int productId, CancellationToken ct = default);
    }
}
