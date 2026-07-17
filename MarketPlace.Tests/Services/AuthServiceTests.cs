using AutoMapper;
using FluentAssertions;
using MarketPlace.Application.DTOs;
using MarketPlace.Application.Interfaces;
using MarketPlace.Application.Interfaces.Repository;
using MarketPlace.Application.Interfaces.Security;
using MarketPlace.Application.Service;
using MarketPlace.Domain.Entities;
using Moq;

namespace MarketPlace.Tests.Services;

public class AuthServiceTests
{
    private readonly Mock<IAuthRepository> _authRepositoryMock;
    private readonly Mock<IJwtTokenGenerator> _jwtTokenGeneratorMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _authRepositoryMock = new Mock<IAuthRepository>();
        _jwtTokenGeneratorMock = new Mock<IJwtTokenGenerator>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _mapperMock = new Mock<IMapper>();

        _authService = new AuthService(
            _authRepositoryMock.Object,
            _jwtTokenGeneratorMock.Object,
            _passwordHasherMock.Object,
            _unitOfWorkMock.Object,
            _mapperMock.Object);
    }

    [Fact]
    public async Task Register_WhenEmailExists_ThrowsInvalidOperation()
    {
        _authRepositoryMock
            .Setup(r => r.ExistUserByEmail(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        Func<Task> act = async () =>
            await _authService.Register(new RegisterDto(), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Login_WhenUserNotFound_ThrowsUnauthorized()
    {
        _authRepositoryMock.Setup(r => r.GetUserByEmail(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User)null!);

        Func<Task> act = async () => await _authService.Login(new LoginDto(), CancellationToken.None);
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Login_WhenWrongPassword_ThrowsUnauthorized()
    {
        _authRepositoryMock.Setup(r => r.GetUserByEmail(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User
            {
                Id = 1,
                Email = "sasha21@gmail.com"
            });
        _passwordHasherMock.Setup(n => n.Verify(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(false);

        Func<Task> act = async () => await _authService.Login(new LoginDto(), CancellationToken.None);
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Login_WhenValidCredentials_ReturnsToken()
    {
        _authRepositoryMock.Setup(r => r.GetUserByEmail(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User
            {
                Id = 1,
                Email = "sasha21@gmail.com"
            });
        _passwordHasherMock.Setup(n => n.Verify(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(true);
        _mapperMock.Setup(n => n.Map<AuthResponseDto>(It.IsAny<User>()))
            .Returns(new AuthResponseDto());
        _jwtTokenGeneratorMock.Setup(j => j.GenerateJwtToken(It.IsAny<User>())).Returns("token");

        var result = await _authService.Login(new LoginDto(), CancellationToken.None);
        result.Should().NotBeNull();
        _authRepositoryMock.Verify(r =>
            r.GetUserByEmail(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Register_WhenValidData_CreatesUserAndReturnsToken()
    {
        _authRepositoryMock
            .Setup(r => r.ExistUserByEmail(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _passwordHasherMock
            .Setup(h => h.Hash(It.IsAny<string>()))
            .Returns("hashed");

        _mapperMock
            .Setup(m => m.Map<AuthResponseDto>(It.IsAny<User>()))
            .Returns(new AuthResponseDto());

        _jwtTokenGeneratorMock
            .Setup(j => j.GenerateJwtToken(It.IsAny<User>()))
            .Returns("token");
        var result = await _authService.Register(new RegisterDto(), CancellationToken.None);

        result.Should().NotBeNull();

        _authRepositoryMock.Verify(r =>
            r.AddUser(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u =>
            u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}