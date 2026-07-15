using MarketPlace.Application.DTOs;

namespace MarketPlace.Application.Interfaces.Service;

public interface IAuthService
{
    Task<AuthResponseDto> Register(RegisterDto registerDto, CancellationToken ct);
    Task<AuthResponseDto> Login(LoginDto loginDto, CancellationToken ct);
}