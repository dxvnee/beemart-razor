

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BeeMart.Api.Models;
using BeeMart.Api.Services;
using Microsoft.IdentityModel.Tokens;

namespace BeeMart.Api.Services;

public class JwtTokenService(IConfiguration config) : IJwtTokenService
{
    private readonly IConfiguration _config = config;

    public (string Token, DateTime ExpiresAt) Generate(User user)
    {
        var jwt = _config.GetSection("Jwt");
        var expires = DateTime.UtcNow.AddMinutes(int.Parse(jwt["ExpireMinutes"] ?? "120"));

        var claims = new[]
        {
            new Claim("sub", user.Id.ToString()),
            new Claim("name", user.Username),
            new Claim("role", user.Role.ToString()),
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!));
        var token = new JwtSecurityToken(
            issuer: jwt["Issuer"],
            audience: jwt["Audience"],
            claims: claims,
            expires: expires,
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
        );

        return (new JwtSecurityTokenHandler().WriteToken(token), expires);
    }
}