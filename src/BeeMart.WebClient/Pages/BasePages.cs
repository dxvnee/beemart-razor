
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BeeMart.WebClient.Pages;
public abstract class BasePage : PageModel
{
    protected bool IsLoggedIn => !string.IsNullOrEmpty(HttpContext.Session.GetString("token"));
    protected void ToastError(string message) => TempData["Toast"] = "error|" + message;
    protected void ToastSuccess(string message) => TempData["Toast"] = "success|" + message;

    protected IActionResult SessionExpired()
    {
        HttpContext.Session.Clear();
        ToastError("Sesi Anda telah berakhir.");
        return RedirectToPage("Login");
    }
}