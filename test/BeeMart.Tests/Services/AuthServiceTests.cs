using BeeMart.Api.Common;
using BeeMart.Api.DTOs;
using BeeMart.Api.Models;
using BeeMart.Api.Services;
using BeeMart.Tests.Helpers;
using Moq;

namespace BeeMart.Tests.Services;

public class AuthServiceTests
{
    private readonly Mock<IJwtTokenService> _jwtMock = new();

    [Fact]
    public async Task RegisterAsync_WithValidCashierRequest_CreatesUserWith5DigitId()
    {
        // Arrange
        using var db = TestDbContextFactory.Create();
        var service = new AuthService(db, _jwtMock.Object);

        var request = new RegisterRequest
        {
            Username = "kasir_baru",
            FullName = "Kasir Baru",
            Password = "Password123",
            Role = "Cashier"
        };

        // Act
        var result = await service.RegisterAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("kasir_baru", result.Username);
        Assert.Equal("Cashier", result.Role);
        Assert.True(result.Stsrc);
        Assert.Equal(5, result.IdUser.Length);
        Assert.True(int.TryParse(result.IdUser, out _));

        var savedUser = await db.MsUsers.FindAsync(result.IdUser);
        Assert.NotNull(savedUser);
        Assert.True(BCrypt.Net.BCrypt.Verify("Password123", savedUser.PasswordHash));
    }

    [Fact]
    public async Task RegisterAsync_WithDuplicateUsername_ThrowsAppException()
    {
        // Arrange
        using var db = TestDbContextFactory.Create();
        db.MsUsers.Add(new MsUser
        {
            IdUser = "11111",
            Username = "existing_user",
            FullName = "Existing",
            PasswordHash = "hash",
            Role = "Cashier",
            Stsrc = true,
            Datein = DateTime.UtcNow,
            Userin = "SYSTEM"
        });
        await db.SaveChangesAsync();

        var service = new AuthService(db, _jwtMock.Object);
        var request = new RegisterRequest
        {
            Username = "existing_user",
            FullName = "Duplicate User",
            Password = "Password123",
            Role = "Cashier"
        };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<AppException>(() => service.RegisterAsync(request));
        Assert.Equal("username has already exists", ex.Message);
        Assert.Equal(400, ex.StatusCode);
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ReturnsTokenAndUser()
    {
        // Arrange
        using var db = TestDbContextFactory.Create();
        var passwordHash = BCrypt.Net.BCrypt.HashPassword("Secret123");
        var user = new MsUser
        {
            IdUser = "22222",
            Username = "kasir1",
            FullName = "Kasir Satu",
            PasswordHash = passwordHash,
            Role = "Cashier",
            Stsrc = true,
            Datein = DateTime.UtcNow,
            Userin = "SYSTEM"
        };
        db.MsUsers.Add(user);
        await db.SaveChangesAsync();

        _jwtMock.Setup(j => j.Generate(It.Is<MsUser>(u => u.IdUser == "22222")))
            .Returns(("mock_jwt_token", DateTime.UtcNow.AddHours(8)));

        var service = new AuthService(db, _jwtMock.Object);
        var request = new LoginRequest
        {
            Username = "kasir1",
            Password = "Secret123",
            Role = "Kasir" // Test Indonesian localized dropdown role input
        };

        // Act
        var result = await service.LoginAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("mock_jwt_token", result.Token);
        Assert.Equal("22222", result.User.IdUser);
        Assert.Equal("Cashier", result.User.Role);
    }

    [Theory]
    [InlineData("wrong_user", "Secret123", "Cashier")]
    [InlineData("kasir1", "WrongPassword", "Cashier")]
    [InlineData("kasir1", "Secret123", "Admin")] // wrong role
    public async Task LoginAsync_WithInvalidCredentialsOrRole_ThrowsInvalidLogin(string username, string password, string role)
    {
        // Arrange
        using var db = TestDbContextFactory.Create();
        db.MsUsers.Add(new MsUser
        {
            IdUser = "33333",
            Username = "kasir1",
            FullName = "Kasir Satu",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Secret123"),
            Role = "Cashier",
            Stsrc = true,
            Datein = DateTime.UtcNow,
            Userin = "SYSTEM"
        });
        await db.SaveChangesAsync();

        var service = new AuthService(db, _jwtMock.Object);
        var request = new LoginRequest
        {
            Username = username,
            Password = password,
            Role = role
        };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<AppException>(() => service.LoginAsync(request));
        Assert.Equal("Invalid login", ex.Message);
        Assert.Equal(401, ex.StatusCode);
    }

    [Fact]
    public async Task LoginAsync_WhenAccountIsInactive_ThrowsForbiddenException()
    {
        // Arrange
        using var db = TestDbContextFactory.Create();
        db.MsUsers.Add(new MsUser
        {
            IdUser = "44444",
            Username = "inactive_user",
            FullName = "Inactive User",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123"),
            Role = "Cashier",
            Stsrc = false, // Inactive
            Datein = DateTime.UtcNow,
            Userin = "SYSTEM"
        });
        await db.SaveChangesAsync();

        var service = new AuthService(db, _jwtMock.Object);
        var request = new LoginRequest
        {
            Username = "inactive_user",
            Password = "Password123",
            Role = "Cashier"
        };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<AppException>(() => service.LoginAsync(request));
        Assert.Equal("Akun Anda telah dinonaktifkan.", ex.Message);
        Assert.Equal(403, ex.StatusCode);
    }
}
