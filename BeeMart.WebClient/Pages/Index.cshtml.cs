using Microsoft.AspNetCore.Mvc;

namespace BeeMart.WebClient.Pages;

public class IndexModel : BasePage
{
    public IActionResult OnGet() =>
        IsLoggedIn ? RedirectToPage("/NewTransaction") : RedirectToPage("/Login");
}
