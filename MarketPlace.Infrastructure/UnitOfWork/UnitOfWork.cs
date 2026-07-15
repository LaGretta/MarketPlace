using MarketPlace.Application.Interfaces;

namespace MarketPlace.Infrastructure.UnitOfWork;

public class UnitOfWork : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public Task BeginTransactionAsync(CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public Task CommitTransactionAsync(CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public Task RollbackTransactionAsync(CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}