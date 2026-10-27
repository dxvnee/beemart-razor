using System.Globalization;
using BeeMart.WebClient.Models;
using BeeMart.WebClient.Services;
using Microsoft.AspNetCore.Mvc;

namespace BeeMart.WebClient.Pages;

public class History(ApiClient api) : BasePage
{
    private readonly ApiClient _api = api;

    [BindProperty(Name = "date", SupportsGet = true)]
    public string? Date { get; set; }

    [BindProperty(Name = "hal", SupportsGet = true)]
    public int PageNumber { get; set; } = 1;

    public PagedData<SummaryItem>? Result { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        if (!IsLoggedIn) return RedirectToPage("/Login");

        if (string.IsNullOrWhiteSpace(Date))
        {
            Date = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, "Asia/Jakarta")
                .ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        }

        var url = $"transactions?date={Uri.EscapeDataString(Date)}&page={Math.Max(PageNumber, 1)}&pageSize=20";
        var response = await _api.GetAsync<PagedData<SummaryItem>>(url);

        if (response.StatusCode == 401) return SessionExpired();

        if (response.Success) Result = response.Data;
        else ToastError(response.Message!);

        return Page();
    }
}