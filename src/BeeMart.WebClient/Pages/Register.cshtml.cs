

using System.ComponentModel.DataAnnotations;
using BeeMart.WebClient.Pages;
using BeeMart.WebClient.Services;
using Microsoft.AspNetCore.Mvc;

namespace BeeMart.WebClient.Pages;

public class Register(ApiClient api) : BasePage
{
    private readonly ApiClient _api = api;

    [BindProperty]
    [Required(ErrorMessage = "Username wajib diisi.")]
    [StringLength(50, ErrorMessage = "Username maksimal 50 karakter.")]
    public string Username { get; set; } = "";

    [BindProperty]
    [Required(ErrorMessage = "Full Name wajib diisi.")]
    [StringLength(100, ErrorMessage = "Nama lengkap maksimal 100 karakter.")]
    public string FullName { get; set; } = "";

    [BindProperty]
    [Required(ErrorMessage = "Password wajib diisi.")]
    [DataType(DataType.Password)]
    [StringLength(100, ErrorMessage = "Password maksimal 100 karakter.")]
    public string Password { get; set; } = "";

    [BindProperty]
    [Required(ErrorMessage = "Konfirmasi password wajib diisi.")]
    [Compare(nameof(Password), ErrorMessage = "Konfirmasi password tidak sama.")]
    public string ConfirmPassword { get; set; } = "";

    [BindProperty]
    [Required(ErrorMessage = "Role wajib dipilih.")]
    public string Role { get; set; } = "Cashier";

    public IActionResult OnGet() => IsLoggedIn ? RedirectToPage("/NewTransaction") : Page();

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        var result = await _api.PostAsync<object>("auth/register", new { Username, FullName, Password, Role });
        if (!result.Success)
        {
            if (result.Errors is not null)
            {
                foreach (var (field, messages) in result.Errors)
                {
                    foreach (var message in messages)
                    {
                        ModelState.AddModelError(field, message);
                    }
                }
            }
            ToastError(result.Message!);
            return Page();
        }

        ToastSuccess("Registrasi berhasil. Silakan login.");
        return RedirectToPage("/Login");
    }
}