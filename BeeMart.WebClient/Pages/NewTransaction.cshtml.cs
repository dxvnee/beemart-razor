using BeeMart.WebClient.Models;
using BeeMart.WebClient.Services;
using Microsoft.AspNetCore.Mvc;

namespace BeeMart.WebClient.Pages;

public class NewTransaction(ApiClient api) : BasePage
{
    private readonly ApiClient _api = api;

    public IActionResult OnGet() =>
        IsLoggedIn ? Page() : RedirectToPage("/Login");

    public async Task<IActionResult> OnGetSearchAsync(string? q)
    {
        if (!IsLoggedIn) return Unauthorized();
        var result = await _api.GetAsync<List<ProductItem>>("products?search=" + Uri.EscapeDataString(q ?? ""));
        return new JsonResult(result);
    }

    public async Task<IActionResult> OnGetNextInvoiceAsync()
    {
        if (!IsLoggedIn) return Unauthorized();
        var result = await _api.GetAsync<NextInvoice>("transaction/next-invoice");
        return new JsonResult(result);
    }

    public async Task<IActionResult> OnPostSaveAsync([FromBody] SaveTransactionRequest request)
    {
        if (!IsLoggedIn) return Unauthorized();
        var result = await _api.PostAsync<object>("transaction", request);
        return new JsonResult(result);
    }
}