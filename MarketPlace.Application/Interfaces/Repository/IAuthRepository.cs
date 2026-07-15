using MarketPlace.Domain.Entities;

namespace MarketPlace.Application.Interfaces.Repository;

public interface IAuthRepository
{
    Task<User?>  GetUserByEmail(string email , CancellationToken ct);
    Task<bool> ExistUserByEmail(string email , CancellationToken ct);
    Task AddUser(User user , CancellationToken ct);
}