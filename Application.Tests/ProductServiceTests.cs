using Application.DTOs;
using Application.Interfaces;
using Application.Services;
using AutoMapper;
using Domain.Entities;
using FluentAssertions;
using Moq;

namespace Application.Tests;

public class ProductServiceTests
{
    private readonly Mock<IProductRepository> _repo = new();
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly IMapper _mapper;

    public ProductServiceTests()
    {
        var cfg = new MapperConfiguration(c =>
        {
            c.CreateMap<Product, ProductReadDto>();
            c.CreateMap<ProductCreateDto, Product>();
        });
        _mapper = cfg.CreateMapper();
    }

    [Fact]
    public async Task GetAsync_returns_dto_when_found()
    {
        _repo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
             .ReturnsAsync(new Product { Id = 1, ProductName = "Ball" });

        var sut = new ProductService(_repo.Object, _uow.Object, _mapper);

        var dto = await sut.GetAsync(1);

        dto.Id.Should().Be(1);
        dto.ProductName.Should().Be("Ball");
    }

    [Fact]
    public async Task CreateAsync_maps_and_saves()
    {
        var sut = new ProductService(_repo.Object, _uow.Object, _mapper);

        var id = await sut.CreateAsync(new ProductCreateDto("New"), "tester");

        _repo.Verify(r => r.AddAsync(It.Is<Product>(p => p.ProductName == "New" && p.CreatedBy == "tester"), It.IsAny<CancellationToken>()), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}