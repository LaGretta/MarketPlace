using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using MarketPlace.Application.Interfaces.Security;
using MarketPlace.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace MarketPlace.Infrastructure.Security;

public class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly IConfiguration _configuration;
    public JwtTokenGenerator(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateJwtToken(User user)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
        };
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration ["Jwt:Key"]!));
        var cred = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken
        (
            issuer:_configuration["Jwt:Issuer"],
            audience:_configuration["Jwt:Audience"],
            expires: DateTime.Now.AddHours(3),
            claims: claims,
            signingCredentials: cred
        );
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}