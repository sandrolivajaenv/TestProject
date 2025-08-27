using System.Data.Common;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Infrastructure.Data;

namespace Infrastructure.Tests
{
    public class SqliteDbFixture : IAsyncLifetime
    {
        private DbConnection? _conn;
        private DbContextOptions<ApplicationDbContext>? _options;

        public ApplicationDbContext CreateContext() =>
            new(_options!);

        public async Task InitializeAsync()
        {
            _conn = new SqliteConnection("DataSource=:memory:;Mode=Memory;Cache=Shared");
            await _conn.OpenAsync();

            _options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(_conn!)
                .Options;

            using var ctx = new ApplicationDbContext(_options);
            await ctx.Database.OpenConnectionAsync();
            await ctx.Database.EnsureCreatedAsync();
        }

        public async Task ResetAsync()
        {
            using var ctx = CreateContext();
            await ctx.Database.EnsureDeletedAsync();
            await ctx.Database.EnsureCreatedAsync();
        }

        public async Task DisposeAsync()
        {
            await (_conn?.DisposeAsync() ?? ValueTask.CompletedTask);
        }
    }
}