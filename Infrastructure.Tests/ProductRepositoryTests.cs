using Domain.Entities;
using FluentAssertions;
using Infrastructure.Data.Repositories;

namespace Infrastructure.Tests
{
    [CollectionDefinition(nameof(SqliteCollection))]
    public class SqliteCollection : ICollectionFixture<SqliteDbFixture> { }

    [Collection(nameof(SqliteCollection))]
    public class ProductRepositoryTests(SqliteDbFixture fx)
    {
        private readonly SqliteDbFixture _fx = fx;

        [Fact]
        public async Task Add_Then_GetById_Works()
        {
            using var db = _fx.CreateContext();
            var repo = new ProductRepository(db);

            var p = new Product { ProductName = "Ball", CreatedBy = "u", CreatedOn = DateTime.UtcNow };
            await repo.AddAsync(p);
            await db.SaveChangesAsync();

            var found = await repo.GetByIdAsync(p.Id);
            found.Should().NotBeNull();
            found!.ProductName.Should().Be("Ball");
        }

        [Fact]
        public async Task GetPagedAsync_Returns_Total_And_Page()
        {
            await _fx.ResetAsync();
            using var db = _fx.CreateContext();

            var repo = new ProductRepository(db);

            for (int i = 1; i <= 12; i++)
                db.Products.Add(new Product { ProductName = $"P{i}", CreatedBy = "u", CreatedOn = DateTime.UtcNow });
            await db.SaveChangesAsync();

            var (items, total) = await repo.GetPagedAsync(page: 2, pageSize: 5);
            total.Should().Be(12);
            items.Should().HaveCount(5);
            items[0].ProductName.Should().Be("P6");
        }

        [Fact]
        public async Task Update_Persists_Modified_Fields()
        {
            using var db = _fx.CreateContext();
            var repo = new ProductRepository(db);

            var id = await SeedHelper.SeedProductAsync(db, "Before");

            var entity = await repo.GetByIdAsync(id);
            entity!.ProductName = "After";
            entity.ModifiedBy = "mod";
            entity.ModifiedOn = DateTime.UtcNow;

            repo.Update(entity);
            await db.SaveChangesAsync();

            var again = await repo.GetByIdAsync(id);
            again!.ProductName.Should().Be("After");
            again.ModifiedBy.Should().Be("mod");
        }

        [Fact]
        public async Task Delete_Removes_Row()
        {
            using var db = _fx.CreateContext();
            var repo = new ProductRepository(db);

            var id = await SeedHelper.SeedProductAsync(db, "X");
            var entity = await repo.GetByIdAsync(id);
            repo.Delete(entity!);
            await db.SaveChangesAsync();

            (await repo.GetByIdAsync(id)).Should().BeNull();
        }
    }
}