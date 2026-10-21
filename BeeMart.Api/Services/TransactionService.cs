using BeeMart.Api.Common;
using BeeMart.Api.Data;
using BeeMart.Api.DTOs;
using BeeMart.Api.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace BeeMart.Api.Services;

public class TransactionService(AppDbContext db, IInvoiceNumberService invoice, ITimeService time) : ITransactionService
{
    private const int MaxAttempts = 3;

    private readonly AppDbContext _db = db;
    private readonly IInvoiceNumberService _invoice = invoice;
    private readonly ITimeService _time = time;

    public async Task<NextInvoiceDto> GetNextInvoiceAsync(int userId)
    {
        return new NextInvoiceDto { InvoiceNumber = await _invoice.GenerateAsync(userId) };
    }

    public async Task<TransactionDto> CreateAsync(int userId, CreateTransactionRequest request)
    {
        for (var attempt = 1; ; attempt++)
        {
            try
            {
                return await TryCreateAsync(userId, request);
            }
            catch (DbUpdateException ex) when (IsUniqueViolation(ex) && attempt < MaxAttempts)
            {
                _db.ChangeTracker.Clear();
            }
        }
    }

    private async Task<TransactionDto> TryCreateAsync(int userId, CreateTransactionRequest request)
    {
        var cashier = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId)
            ?? throw new AppException("User tidak ditemukan.", StatusCodes.Status401Unauthorized);

        var lines = request.Items
            .GroupBy(i => i.ProductId)
            .Select(g => new { ProductId = g.Key, Quantity = g.Sum(x => x.Quantity) })
            .ToList();

        var ids = lines.ConvertAll(l => l.ProductId);
        var products = await _db.Products.Where(p => ids.Contains(p.Id)).ToDictionaryAsync(p => p.Id);

        var missing = ids.Where(id => !products.ContainsKey(id)).ToList();
        if (missing.Count > 0)
        {
            throw new AppException($"Produk dengan ID {string.Join(", ", missing)} tidak ditemukan.", StatusCodes.Status404NotFound);
        }

        var details = new List<TransactionDetail>();
        foreach (var line in lines)
        {
            var product = products[line.ProductId];

            if (line.Quantity > 9999)
                throw new AppException($"Quantity {product.Name} maksimal 9999.");

            if (product.Stock < line.Quantity)
            {
                throw new AppException($"Stok {product.Name} tidak mencukupi (tersisa {product.Stock}).", StatusCodes.Status409Conflict);
            }

            product.Stock -= line.Quantity;
            details.Add(new TransactionDetail
            {
                ProductId = product.Id,
                Quantity = line.Quantity,
                UnitPrice = product.Price,
                SubTotal = product.Price * line.Quantity
            });
        }

        var transaction = new Transaction
        {
            InvoiceNumber = await _invoice.GenerateAsync(userId),
            UserId = userId,
            TransactionDate = _time.UtcNow,
            PaymentMethod = Enum.Parse<PaymentMethod>(request.PaymentMethod, ignoreCase: true),
            TotalAmount = details.Sum(d => d.SubTotal),
            Details = details
        };

        _db.Transactions.Add(transaction);
        await _db.SaveChangesAsync();

        return new TransactionDto
        {
            Id = transaction.Id,
            InvoiceNumber = transaction.InvoiceNumber,
            TransactionDate = _time.ToLocal(transaction.TransactionDate),
            CashierId = cashier.Id,
            CashierName = cashier.FullName,
            PaymentMethod = transaction.PaymentMethod.ToString(),
            TotalAmount = transaction.TotalAmount,
            Items = [.. details.Select(d => new TransactionItemDto
            {
                ProductId = d.ProductId,
                ProductName = products[d.ProductId].Name,
                Quantity = d.Quantity,
                UnitPrice = d.UnitPrice,
                SubTotal = d.SubTotal
            })]
        };
    }

    private const int MaxPageSize = 20;
    public async Task<PagedResult<TransactionSummaryDto>> GetHistoryAsync(int userId, bool isAdmin, DateOnly? date, int page, int pageSize)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, MaxPageSize);

        var (startUtc, endUtc) = _time.GetUtcRange(date ?? _time.LocalToday);

        var query = _db.Transactions.AsNoTracking()
        .Where(t => t.TransactionDate >= startUtc && t.TransactionDate < endUtc);

        if (!isAdmin)
        {
            query = query.Where(t => t.UserId == userId);
        }

        var totalItems = await query.CountAsync();
        var rows = await query
            .OrderBy(t => t.TransactionDate).ThenBy(t => t.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new
            {
                t.Id,
                t.InvoiceNumber,
                t.TransactionDate,
                CashierName = t.User.FullName,
                t.PaymentMethod,
                t.TotalAmount,
                ItemCount = t.Details.Count
            })
            .ToListAsync();

        return new PagedResult<TransactionSummaryDto>
        {
            Items = [.. rows.Select(r => new TransactionSummaryDto()
            {
                Id = r.Id,
            InvoiceNumber = r.InvoiceNumber,
            TransactionDate = _time.ToLocal(r.TransactionDate),
            CashierName = r.CashierName,
            PaymentMethod = r.PaymentMethod.ToString(),
            ItemCount = r.ItemCount,
            TotalAmount = r.TotalAmount
            })],
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize)
        };
    }

    public async Task<TransactionDto> GetByIdAsync(int userId, bool isAdmin, int id)
    {
        var t = await _db.Transactions.AsNoTracking()
            .Include(x => x.User)
            .Include(x => x.Details)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (t == null || (!isAdmin && t.UserId != userId)) throw new AppException("Transaksi tidak ditemukan.", StatusCodes.Status404NotFound);

        return new TransactionDto
        {
            Id = t.Id,
            InvoiceNumber = t.InvoiceNumber,
            TransactionDate = _time.ToLocal(t.TransactionDate),
            CashierId = t.UserId,
            CashierName = t.User.FullName,
            PaymentMethod = t.PaymentMethod.ToString(),
            TotalAmount = t.TotalAmount,
            Items = [.. t.Details.Select(d => new TransactionItemDto
            {
                ProductId = d.ProductId,
                ProductName = d.Product.Name,
                Quantity = d.Quantity,
                UnitPrice = d.UnitPrice,
                SubTotal = d.SubTotal
            })]
        };
    }

    private static bool IsUniqueViolation(DbUpdateException ex) =>
        ex.InnerException is SqlException { Number: 2601 or 2627 };
}