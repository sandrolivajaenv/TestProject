using FluentAssertions;
using Infrastructure.Data.Repositories;

namespace Infrastructure.Tests
{
    [Collection(nameof(SqliteCollection))]
    public class ItemRepositoryTests(SqliteDbFixture fx)
    {
        private readonly SqliteDbFixture _fx = fx;

        [Fact]
        public async Task ListByProductAsync_Returns_Only_That_Products_Items()
        {
            using var db = _fx.CreateContext();
            var products = new ProductRepository(db);
            var items = new ItemRepository(db);

            var p1 = await SeedHelper.SeedProductAsync(db, "P1");
            var p2 = await SeedHelper.SeedProductAsync(db, "P2");

            await SeedHelper.SeedItemAsync(db, p1, 1);
            await SeedHelper.SeedItemAsync(db, p1, 2);
            await SeedHelper.SeedItemAsync(db, p2, 3);

            var list = await items.ListByProductAsync(p1);
            list.Should().HaveCount(2);
            list.Should().OnlyContain(i => i.ProductId == p1);
        }

        [Fact]
        public async Task GetAsync_Filters_By_Product_And_Id()
        {
            using var db = _fx.CreateContext();
            var items = new ItemRepository(db);

            var p1 = await SeedHelper.SeedProductAsync(db, "P1");
            var itemId = await SeedHelper.SeedItemAsync(db, p1, 5);

            (await items.GetAsync(p1, itemId)).Should().NotBeNull();
            (await items.GetAsync(productId: 999, itemId)).Should().BeNull();
        }

        [Fact]
        public async Task Delete_Is_Idempotent_When_NotFound()
        {
            using var db = _fx.CreateContext();
            var items = new ItemRepository(db);

            var p1 = await SeedHelper.SeedProductAsync(db, "P1");
            var itemId = await SeedHelper.SeedItemAsync(db, p1, 5);

            db.ChangeTracker.Clear();

            var entity = await items.GetAsync(p1, itemId);
            items.Delete(entity!);
            await db.SaveChangesAsync();

            var entity2 = await items.GetAsync(p1, itemId);
            entity2.Should().BeNull();
        }

    }
}