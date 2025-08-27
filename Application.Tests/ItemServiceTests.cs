using Application.DTOs;
using Application.Interfaces;
using Application.Services;
using AutoMapper;
using Domain.Entities;
using FluentAssertions;
using Moq;

namespace Application.Tests;

public class ItemServiceTests
{
    private readonly Mock<IItemRepository> _items = new();
    private readonly Mock<IProductRepository> _products = new();
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly IMapper _mapper;

    public ItemServiceTests()
    {
        var cfg = new MapperConfiguration(c =>
        {
            c.CreateMap<Item, ItemReadDto>();
        });
        _mapper = cfg.CreateMapper();
    }

    [Fact]
    public async Task GetByProductAsync_returns_list()
    {
        _products.Setup(p => p.GetByIdAsync(5, It.IsAny<CancellationToken>())).ReturnsAsync(new Product { Id = 5 });
        _items.Setup(r => r.ListByProductAsync(5, It.IsAny<CancellationToken>()))
              .ReturnsAsync([new() { Id = 1, ProductId = 5, Quantity = 2 }]);

        var sut = new ItemService(_items.Object, _products.Object, _uow.Object, _mapper);
        var list = await sut.GetByProductAsync(5);

        list.Should().HaveCount(1);
        list.First().Quantity.Should().Be(2);
    }

    [Fact]
    public async Task CreateAsync_checks_parent_and_saves()
    {
        _products.Setup(p => p.GetByIdAsync(5, It.IsAny<CancellationToken>())).ReturnsAsync(new Product { Id = 5 });

        var sut = new ItemService(_items.Object, _products.Object, _uow.Object, _mapper);
        var id = await sut.CreateAsync(5, new ItemCreateDto(3));

        _items.Verify(r => r.AddAsync(It.Is<Item>(i => i.ProductId == 5 && i.Quantity == 3), It.IsAny<CancellationToken>()), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}