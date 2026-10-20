using BeeMart.Api.DTOs;

namespace BeeMart.Api.Services;

public interface IAuthService
{
    Task<UserDto> RegisterAsync(RegisterRequest request);
    Task<AuthResponse> LoginAsync(LoginRequest request);
}
