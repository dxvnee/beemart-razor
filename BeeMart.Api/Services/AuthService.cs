using BeeMart.Api.Common;
using BeeMart.Api.Data;
using BeeMart.Api.DTOs;
using BeeMart.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace BeeMart.Api.Services;

public class AuthService(AppDbContext db, IJwtTokenService jwt) : IAuthService
{
    private readonly AppDbContext _db = db;
    private readonly IJwtTokenService _jwt = jwt;


    public async Task<UserDto> RegisterAsync(RegisterRequest request)
    {
        var username = request.Username.Trim();
        if (await _db.Users.AnyAsync(u => u.Username == username))
        {
            throw new AppException("Username already exists", StatusCodes.Status400BadRequest);
        }

        var user = new User
        {
            Username = username,
            FullName = request.FullName.Trim(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = Enum.Parse<UserRole>(request.Role, ignoreCase: true)
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        return ToDto(user);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var username = request.Username.Trim();
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == username);

        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new AppException("Username atau password salah.", StatusCodes.Status401Unauthorized);

        var (token, expiresAt) = _jwt.Generate(user);

        return new AuthResponse
        {
            Token = token,
            ExpiresAt = expiresAt,
            User = ToDto(user)
        };
    }
    private static UserDto ToDto(User user) => new()
    {
        Id = user.Id,
        Username = user.Username,
        FullName = user.FullName,
        Role = user.Role.ToString()
    };
}