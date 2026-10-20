using System.ComponentModel.DataAnnotations;

namespace BeeMart.Api.DTOs;

public class LoginRequest
{
    [Required(ErrorMessage = "Username wajib diisi.")]
    public string Username { get; set; } = null!;

    [Required(ErrorMessage = "Password wajib diisi.")]
    public string Password { get; set; } = null!;
}