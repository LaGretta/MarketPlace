using AutoMapper;
using MarketPlace.Application.DTOs;
using MarketPlace.Application.Interfaces;
using MarketPlace.Application.Interfaces.Repository;
using MarketPlace.Application.Interfaces.Security;
using MarketPlace.Application.Interfaces.Service;
using MarketPlace.Domain.Entities;
using MarketPlace.Domain.Enums;

namespace MarketPlace.Application.Service;

public class AuthService : IAuthService
{
    private readonly IAuthRepository _authRepository;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;


    public AuthService(IAuthRepository authRepository
        , IJwtTokenGenerator jwtTokenGenerator
        , IPasswordHasher passwordHasher
        , IUnitOfWork unitOfWork
        , IMapper mapper)
    {
        _authRepository = authRepository;
        _jwtTokenGenerator = jwtTokenGenerator;
        _passwordHasher = passwordHasher;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<AuthResponseDto> Register(RegisterDto registerDto, CancellationToken ct)
    {
        if (await _authRepository.ExistUserByEmail(registerDto.Email, ct))
            throw new InvalidOperationException("Email already exists");

        var user = new User
        {
            Username = registerDto.Username,
            Email = registerDto.Email,
            PasswordHash = _passwordHasher.Hash(registerDto.Password),
            Role = Role.Buyer,
            CreatedAt = DateTime.UtcNow
        };

        await _authRepository.AddUser(user , ct);
        await _unitOfWork.SaveChangesAsync(ct);

        var response = _mapper.Map<AuthResponseDto>(user);
        response.Token = _jwtTokenGenerator.GenerateJwtToken(user);
        return response;
    }

    public async Task<AuthResponseDto> Login(LoginDto loginDto, CancellationToken ct)
    {
        var find = await _authRepository.GetUserByEmail(loginDto.Email ,ct);
        if (find == null || !_passwordHasher.Verify(loginDto.Password, find.PasswordHash))
            throw new UnauthorizedAccessException("Email or password is incorrect");
        
        var response = _mapper.Map<AuthResponseDto>(find);
        response.Token = _jwtTokenGenerator.GenerateJwtToken(find);
        return response;
    }
}