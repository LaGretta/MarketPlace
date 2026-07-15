using MarketPlace.Domain.Entities;

namespace MarketPlace.Application.Interfaces.Security;

public interface IJwtTokenGenerator
{
    string GenerateJwtToken(User user);
}