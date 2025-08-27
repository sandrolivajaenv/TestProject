using Infrastructure.Data;
using Domain.Entities;

namespace Infrastructure.Tests
{
    public static class SeedHelper
    {
        public static async Task<int> SeedProductAsync(ApplicationDbContext db, string name = "Prod")
        {
            var p = new Product { ProductName = name, CreatedBy = "seed", CreatedOn = DateTime.UtcNow };
            db.Products.Add(p);
            await db.SaveChangesAsync();
            return p.Id;
        }

        public static async Task<int> SeedItemAsync(ApplicationDbContext db, int productId, int qty = 1)
        {
            var it = new Item { ProductId = productId, Quantity = qty };
            db.Items.Add(it);
            await db.SaveChangesAsync();
            return it.Id;
        }
    }
}