using System.Globalization;
using BeeMart.Api.Common;
using BeeMart.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace BeeMart.Api.Services;

public class InvoiceNumberServices(AppDbContext db, ITimeService time) : IInvoiceNumberService
{
    private readonly AppDbContext _db = db;
    private readonly ITimeService _time = time;

    public async Task<string> GenerateAsync(int userId)
    {
        var date = _time.LocalNow.ToString("ddMMyy", CultureInfo.InvariantCulture);
        var prefix = $"{userId:D5}{date}";

        var last = await _db.Transactions
        .AsNoTracking()
        .Where(t => t.InvoiceNumber.StartsWith(prefix))
        .OrderByDescending(t => t.InvoiceNumber)
        .Select(t => t.InvoiceNumber)
        .FirstOrDefaultAsync();

        var next = last is null ? 1 : int.Parse(last[^3..]) + 1;

        if (next > 999)
            throw new AppException("Nomor invoice sudah mencapai batas maksimal.", StatusCodes.Status409Conflict);

        return $"{prefix}{next:D3}";
    }
}
