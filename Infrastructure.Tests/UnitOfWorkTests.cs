using Domain.Entities;
using FluentAssertions;
using Infrastructure.Data;

namespace Infrastructure.Tests
{
    [Collection(nameof(SqliteCollection))]
    public class UnitOfWorkTests(SqliteDbFixture fx)
    {
        private readonly SqliteDbFixture _fx = fx;

        [Fact]
        public async Task SaveChangesAsync_Commits_Pending_Changes()
        {
            using var db = _fx.CreateContext();
            var uow = new UnitOfWork(db);

            db.Products.Add(new Product { ProductName = "UoW", CreatedBy = "u", CreatedOn = DateTime.UtcNow });
            var affected = await uow.SaveChangesAsync();

            affected.Should().BeGreaterThan(0);
        }
    }
}