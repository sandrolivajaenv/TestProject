using Application.Interfaces;

namespace Infrastructure.Data;

public class UnitOfWork(ApplicationDbContext db) : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken ct = default) => db.SaveChangesAsync(ct);
}