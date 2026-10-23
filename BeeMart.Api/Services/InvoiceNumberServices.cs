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
        var prefix = $"{IdUser}{date}";

        var last = await _db.TrTransactions
            .AsNoTracking()
            .Where(t => t.InvoiceNo.StartsWith(prefix))
            .OrderByDescending(t => t.InvoiceNo)
            .Select(t => t.InvoiceNo)
            .FirstOrDefaultAsync();

        var next = 1;
        if (last is not null && int.TryParse(last[prefix.Length..], out var lastSeq))
        {
            next = lastSeq + 1;
        }

        if (next > 999)
            throw new AppException("Nomor invoice sudah mencapai batas maksimal.", StatusCodes.Status409Conflict);

        return $"{prefix}{next:D3}";
    }
}
