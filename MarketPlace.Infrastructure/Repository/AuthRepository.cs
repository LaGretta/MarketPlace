using MarketPlace.Application.Interfaces.Repository;
using MarketPlace.Domain.Entities;
using MarketPlace.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MarketPlace.Infrastructure.Repository;

public class AuthRepository : IAuthRepository
{
    private readonly AppDbContext _dbContext;
    public AuthRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public async Task<User?> GetUserByEmail(string email, CancellationToken ct)
    {
        var find = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email, ct);
        return find;
    }
    public async Task<bool> ExistUserByEmail(string email, CancellationToken ct)
    {
        var  find = await _dbContext.Users.AnyAsync(u => u.Email == email, ct);
        return find;
    }
    public async Task AddUser(User user, CancellationToken ct)
    {
        await _dbContext.Users.AddAsync(user, ct);
    }
}