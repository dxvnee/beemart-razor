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
            throw new AppException("Username already exists", StatusCodes.Status400BadRequest);
        }

        var role = request.Role.Trim().Equals("Admin", StringComparison.OrdinalIgnoreCase) ? "Admin" : "Cashier";
        var prefix = role == "Admin" ? "ADM" : "CSH";

        var lastUser = await _db.MsUsers
            .Where(u => u.IdUser.StartsWith(prefix))
            .OrderByDescending(u => u.IdUser)
            .Select(u => u.IdUser)
            .FirstOrDefaultAsync();

        var nextSeq = 1;
        if (lastUser is not null && int.TryParse(lastUser[prefix.Length..], out var lastSeq))
        {
            nextSeq = lastSeq + 1;
        }

        if (nextSeq > 99)
        {
            throw new AppException($"Kapasitas ID untuk role {role} sudah penuh (maksimal 99).", StatusCodes.Status400BadRequest);
        }

        var idUser = $"{prefix}{nextSeq:D2}";

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
        var user = await _db.MsUsers.FirstOrDefaultAsync(u => u.Username == username);

        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new AppException("Username atau password salah.", StatusCodes.Status401Unauthorized);

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