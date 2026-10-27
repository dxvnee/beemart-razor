using System.ComponentModel.DataAnnotations;

namespace BeeMart.Api.DTOs;

public class RegisterRequest
{
    [Required(ErrorMessage = "Username wajib diisi.")]
    [StringLength(50, ErrorMessage = "Username maksimal 50 karakter.")]
    public string Username { get; set; } = null!;

    [Required(ErrorMessage = "Nama lengkap wajib diisi.")]
    [StringLength(300, ErrorMessage = "Nama lengkap maksimal 300 karakter.")]
    public string FullName { get; set; } = null!;

    [Required(ErrorMessage = "Password wajib diisi.")]
    [StringLength(100, ErrorMessage = "Password maksimal 100 karakter.")]
    public string Password { get; set; } = null!;

    [Required(ErrorMessage = "Role wajib dipilih.")]
    [RegularExpression(@"(?i)^(Admin|Cashier)$", ErrorMessage = "Role harus Admin atau Kasir.")]
    public string Role { get; set; } = null!;
}