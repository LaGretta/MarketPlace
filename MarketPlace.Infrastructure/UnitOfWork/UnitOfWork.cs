using MarketPlace.Application.Interfaces;
using MarketPlace.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Storage;

namespace MarketPlace.Infrastructure.UnitOfWork;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _dbContext;
    private IDbContextTransaction? _transaction;
    public UnitOfWork(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public Task<int> SaveChangesAsync(CancellationToken ct)
        => _dbContext.SaveChangesAsync(ct);

    public async Task BeginTransactionAsync(CancellationToken ct)
    {
        _transaction = await _dbContext.Database.BeginTransactionAsync(ct);
    }

    public async Task CommitTransactionAsync(CancellationToken ct)
    {
        if (_transaction != null)
        {
           await _transaction.CommitAsync(ct);
           await _transaction.DisposeAsync();
           _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken ct)
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync(ct);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }
}