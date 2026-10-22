using Microsoft.AspNetCore.Mvc;

namespace BeeMart.WebClient.Pages;

public class Logout : BasePage
{
    public IActionResult OnGet() => RedirectToPage("/Login");

    public IActionResult OnPost()
    {
        HttpContext.Session.Clear();
        ToastSuccess("Anda sudah logout.");
        return RedirectToPage("/Login");
    }
}