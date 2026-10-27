using System.Globalization;
using BeeMart.Api.Common;
using BeeMart.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace BeeMart.Api.Services;

public class InvoiceNumberServices(AppDbContext db, ITimeService time) : IInvoiceNumberService
{
    private readonly AppDbContext _db = db;
    private readonly ITimeService _time = time;

    public async Task<string> GenerateAsync(string IdUser)
    {
        var date = _time.LocalNow.ToString("ddMMyy", CultureInfo.InvariantCulture);

        var todayInvoices = await _db.TrTransactions
            .AsNoTracking()
            .Where(t => t.InvoiceNo.Length == 14 && t.InvoiceNo.Substring(5, 6) == date)
            .Select(t => t.InvoiceNo)
            .ToListAsync();

        var maxSeq = 0;
        foreach (var inv in todayInvoices)
        {
            if (int.TryParse(inv.AsSpan(11, 3), out var s))
            {
                if (s > maxSeq) maxSeq = s;
            }
        }

        var next = maxSeq + 1;
        if (next > 999)
            throw new AppException("Nomor invoice sudah mencapai batas maksimal.", StatusCodes.Status409Conflict);

        return $"{IdUser}{date}{next:D3}";
    }
}
