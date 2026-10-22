using System.ComponentModel.DataAnnotations;
using BeeMart.WebClient.Models;
using BeeMart.WebClient.Services;
using Microsoft.AspNetCore.Mvc;

namespace BeeMart.WebClient.Pages;

public class Login(ApiClient api) : BasePage
{
    private readonly ApiClient _api = api;

    [BindProperty]
    [Required(ErrorMessage = "Username wajib diisi.")]
    public string Username { get; set; } = "";

    [BindProperty]
    [Required(ErrorMessage = "Password wajib diisi.")]
    public string Password { get; set; } = "";

    public IActionResult OnGet() =>
        IsLoggedIn ? RedirectToPage("/NewTransaction") : Page();

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        var result = await _api.PostAsync<AuthData>("auth/login", new { Username, Password });
        if (!result.Success || result.Data is null)
        {
            ToastError(result.Message!);
            return Page();
        }

        HttpContext.Session.SetString("token", result.Data.Token);
        HttpContext.Session.SetString("fullName", result.Data.User.FullName!);
        HttpContext.Session.SetString("role", result.Data.User.Role!);
        return RedirectToPage("/NewTransaction");
    }
}