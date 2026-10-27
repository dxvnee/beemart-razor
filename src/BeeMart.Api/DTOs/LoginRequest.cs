using System.ComponentModel.DataAnnotations;

namespace BeeMart.Api.DTOs;

public class LoginRequest
{
    [Required(ErrorMessage = "Username wajib diisi.")]
    public string Username { get; set; } = null!;

    [Required(ErrorMessage = "Password wajib diisi.")]
    public string Password { get; set; } = null!;

    [Required(ErrorMessage = "Role wajib diisi.")]
    public string Role { get; set; } = null!;
}