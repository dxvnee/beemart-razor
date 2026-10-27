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
        if (await _db.MsUsers.AnyAsync(u => u.Username == username))
        {
            throw new AppException("username has already exists", StatusCodes.Status400BadRequest);
        }

        var role = request.Role.Trim().Equals("Admin", StringComparison.OrdinalIgnoreCase) ? "Admin" : "Cashier";

        // Generate 5-digit numeric IdUser to fulfill: [5 digit userID][ddmmyy][3 digit running number]
        string idUser;
        do
        {
            idUser = Random.Shared.Next(10000, 99999).ToString();
        } while (await _db.MsUsers.AnyAsync(u => u.IdUser == idUser));

        var user = new MsUser
        {
            IdUser = idUser,
            Username = username,
            FullName = request.FullName.Trim(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = role,
            Stsrc = true,
            Datein = DateTime.UtcNow,
            Userin = "SYSTEM"
        };

        _db.MsUsers.Add(user);
        await _db.SaveChangesAsync();

        return ToDto(user);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var username = request.Username.Trim();
        var requestedRole = request.Role?.Trim();
        var normalizedRole = string.Equals(requestedRole, "Kasir", StringComparison.OrdinalIgnoreCase) ? "Cashier" : requestedRole;

        var user = await _db.MsUsers.FirstOrDefaultAsync(u => u.Username == username);

        if (user is null
            || !string.Equals(user.Role, normalizedRole, StringComparison.OrdinalIgnoreCase)
            || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            throw new AppException("Invalid login", StatusCodes.Status401Unauthorized);
        }

        if (!user.Stsrc)
            throw new AppException("Akun Anda telah dinonaktifkan.", StatusCodes.Status403Forbidden);

        var (token, expiresAt) = _jwt.Generate(user);

        return new AuthResponse
        {
            Token = token,
            ExpiresAt = expiresAt,
            User = ToDto(user)
        };
    }
    private static UserDto ToDto(MsUser user) => new()
    {
        IdUser = user.IdUser,
        Username = user.Username,
        FullName = user.FullName,
        Role = user.Role,
        Stsrc = user.Stsrc
    };
}