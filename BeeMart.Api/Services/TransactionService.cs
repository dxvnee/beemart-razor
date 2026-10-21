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

    private static bool IsUniqueViolation(DbUpdateException ex) =>
        ex.InnerException is SqlException { Number: 2601 or 2627 };
}