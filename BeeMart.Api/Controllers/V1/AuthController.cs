using Asp.Versioning;
using BeeMart.Api.Common;
using BeeMart.Api.DTOs;
using BeeMart.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BeeMart.Api.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/auth")]
public class AuthController(IAuthService auth) : ControllerBase
{
    private readonly IAuthService _auth = auth;

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        var user = await _auth.RegisterAsync(request);
        return StatusCode(StatusCodes.Status201Created, ApiResponse<UserDto>.Ok(user, "Registrasi berhasil."));
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var result = await _auth.LoginAsync(request);
        return Ok(ApiResponse<AuthResponse>.Ok(result, "Login berhasil."));
    }

    [Authorize]
    [HttpGet("me")]
    public IActionResult Me()
    {
        var me = new
        {
            Id = User.FindFirst("sub")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value,
            Username = User.Identity?.Name ?? User.FindFirst("name")?.Value,
            Role = User.FindFirst("role")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value,
            FullName = User.FindFirst("fullName")?.Value
        };
        return Ok(ApiResponse<object>.Ok(me));
    }
}
