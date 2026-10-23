

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BeeMart.Api.Models;
using Microsoft.IdentityModel.Tokens;

namespace BeeMart.Api.Services;

public class JwtTokenService(IConfiguration config) : IJwtTokenService
{
    private readonly IConfiguration _config = config;

    public (string Token, DateTime ExpiresAt) Generate(MsUser user)
    {
        var jwt = _config.GetSection("Jwt");
        var keyString = jwt["Key"] ?? throw new InvalidOperationException("JWT Key belum dikonfigurasi di appsettings.json.");
        var expires = DateTime.UtcNow.AddMinutes(int.Parse(jwt["ExpireMinutes"] ?? "120"));

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.IdUser),
            new Claim("name", user.Username),
            new Claim("role", user.Role),
            new Claim("fullName", user.FullName),
            new Claim(ClaimTypes.NameIdentifier, user.IdUser),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyString));
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