using System.ComponentModel.DataAnnotations;

namespace BeeMart.Api.DTOs;

public class RegisterRequest
{
    [Required(ErrorMessage = "Username Wajib diisi")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Username harus 3 sampai 50 karakter.")]
    [RegularExpression(@"^[a-zA-Z0-9_.]+$", ErrorMessage = "Username hanya boleh huruf, angka, titik, dan underscore.")]
    public string Username { get; set; } = null!;

    [Required(ErrorMessage = "Nama lengkap wajib diisi.")]
    [StringLength(100, ErrorMessage = "Nama lengkap maksimal 100 karakter.")]
    public string FullName { get; set; } = null!;

    [Required(ErrorMessage = "Password wajib diisi.")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Password minimal 6 karakter.")]
    [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d).+$", ErrorMessage = "Password harus mengandung huruf dan angka.")]
    public string Password { get; set; } = null!;

    [Required(ErrorMessage = "Role wajib dipilih.")]
    [RegularExpression(@"(?i)^(Admin|Kasir)$", ErrorMessage = "Role harus Admin atau Kasir.")]
    public string Role { get; set; } = null!;
}