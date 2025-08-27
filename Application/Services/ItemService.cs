using Application.DTOs;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;
using Domain.Exceptions;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Services;

public class ItemService(
    IItemRepository itemRepository,
    IProductRepository productRepo,
    IUnitOfWork uow,
    IMapper mapper) : IItemService
{
    public async Task<IEnumerable<ItemReadDto>> GetByProductAsync(int productId, CancellationToken ct = default)
    {
        // ensure parent exists (optional fast-fail; comment out if you prefer empty list when missing)
        if (await productRepo.GetByIdAsync(productId, ct) is null)
            throw new NotFound(nameof(Product), productId);

        var items = await itemRepository.ListByProductAsync(productId, ct);
        return mapper.Map<IEnumerable<ItemReadDto>>(items);
    }

    public async Task<ItemReadDto> GetAsync(int itemId, int productId, CancellationToken ct = default)
    {
        var entity = await itemRepository.GetAsync(itemId, productId, ct);
        return entity is null 
            ? throw new NotFound(nameof(Item), productId) 
            : mapper.Map<ItemReadDto>(entity);
    }

    public async Task<int> CreateAsync(int productId, ItemCreateDto dto, CancellationToken ct = default)
    {
        // make sure the FK target exists to return 404 instead of a DB FK error
        if (await productRepo.GetByIdAsync(productId, ct) is null)
            throw new NotFound(nameof(Product), productId);

        var entity = new Item
        {
            ProductId = productId,
            Quantity = dto.Quantity
        };

        await itemRepository.AddAsync(entity, ct);
        await uow.SaveChangesAsync(ct);
        return entity.Id;
    }

    public async Task DeleteAsync(int itemId, int productId, CancellationToken ct = default)
    {
        var entity = await itemRepository.GetAsync(itemId, productId, ct);
        if (entity is null)
            return; 

        itemRepository.Delete(entity);
        await uow.SaveChangesAsync(ct);
    }
}