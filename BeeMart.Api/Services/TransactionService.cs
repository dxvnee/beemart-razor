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

    public async Task<NextInvoiceDto> GetNextInvoiceAsync(string IdUser)
    {
        return new NextInvoiceDto { InvoiceNo = await _invoice.GenerateAsync(IdUser) };
    }

    public async Task<TransactionDto> CreateAsync(string IdUser, CreateTransactionRequest request)
    {
        for (var attempt = 1; ; attempt++)
        {
            try
            {
                return await TryCreateAsync(IdUser, request);
            }
            catch (DbUpdateException ex) when (IsUniqueViolation(ex) && attempt < MaxAttempts)
            {
                _db.ChangeTracker.Clear();
            }
        }
    }

    private async Task<TransactionDto> TryCreateAsync(string IdUser, CreateTransactionRequest request)
    {
        var cashier = await _db.MsUsers.AsNoTracking().FirstOrDefaultAsync(u => u.IdUser == IdUser && u.Stsrc)
            ?? throw new AppException("User tidak ditemukan atau tidak aktif.", StatusCodes.Status401Unauthorized);

        var paymentMethod = await _db.LtPaymentMethods.AsNoTracking()
            .FirstOrDefaultAsync(p => p.IdPaymentMethod == request.IdPaymentMethod && p.Stsrc)
            ?? throw new AppException("Metode pembayaran tidak valid.", StatusCodes.Status400BadRequest);

        var lines = request.Items
            .GroupBy(i => i.IdProduct)
            .Select(g => new { ProductId = g.Key, Quantity = g.Sum(x => x.Qty) })
            .ToList();

        var ids = lines.ConvertAll(l => l.ProductId);
        var products = await _db.MsProducts
            .Where(p => ids.Contains(p.IdProduct) && p.Stsrc)
            .ToDictionaryAsync(p => p.IdProduct);

        var missing = ids.Where(id => !products.ContainsKey(id)).ToList();
        if (missing.Count > 0)
        {
            throw new AppException($"Produk dengan ID {string.Join(", ", missing)} tidak ditemukan.", StatusCodes.Status404NotFound);
        }

        var now = _time.UtcNow;
        var transactionId = Guid.NewGuid().ToString();

        var details = new List<TrTransactionDetail>();
        foreach (var line in lines)
        {
            var product = products[line.ProductId];

            if (line.Quantity > 9999)
                throw new AppException($"Quantity {product.Name} maksimal 9999.");

            if (product.Stock < line.Quantity)
            {
                throw new AppException("Jumlah barang yang dimasukkan melebihi sisa stok.", StatusCodes.Status400BadRequest);
            }

            product.Stock -= line.Quantity;
            product.Dateup = now;
            product.Userup = IdUser;


            var gross = product.Price * line.Quantity;
            var discountNominal = gross * (product.Discount / 100m);
            var subTotal = gross - discountNominal;

            details.Add(new TrTransactionDetail
            {
                IdTransactionDetail = Guid.NewGuid().ToString(),
                IdTransaction = transactionId,
                IdProduct = product.IdProduct,
                Qty = line.Quantity,
                Price = product.Price,
                Discount = product.Discount,
                SubTotal = subTotal,
                Stsrc = true,
                Datein = now,
                Userin = IdUser
            });
        }

        var transaction = new TrTransaction
        {
            IdTransaction = transactionId,
            InvoiceNo = await _invoice.GenerateAsync(IdUser),
            IdUser = IdUser,
            TransactionDate = now,
            IdPaymentMethod = request.IdPaymentMethod,
            TotalPrice = details.Sum(d => d.SubTotal),
            Stsrc = true,
            Datein = now,
            Userin = IdUser,
            TrTransactionDetails = details
        };

        _db.TrTransactions.Add(transaction);
        await _db.SaveChangesAsync();

        return new TransactionDto
        {
            IdTransaction = transaction.IdTransaction,
            InvoiceNo = transaction.InvoiceNo,
            TransactionDate = _time.ToLocal(transaction.TransactionDate),
            IdUser = cashier.IdUser,
            CashierName = cashier.FullName,
            IdPaymentMethod = transaction.IdPaymentMethod,
            PaymentMethodName = paymentMethod.MethodName,
            TotalPrice = transaction.TotalPrice,
            Items = [.. details.Select(d => new TransactionItemDto
            {
                IdTransactionDetail = d.IdTransactionDetail,
                IdProduct = d.IdProduct,
                ProductName = products[d.IdProduct].Name,
                Qty = d.Qty,
                Price = d.Price,
                Discount = d.Discount,
                SubTotal = d.SubTotal
            })]
        };
    }

    private const int MaxPageSize = 20;
    public async Task<PagedResult<HistoryDto>> GetHistoryAsync(string IdUser, bool isAdmin, DateOnly? date, int page, int pageSize)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, MaxPageSize);

        if (!date.HasValue)
        {
            throw new AppException("Parameter tanggal wajib diisi.", StatusCodes.Status400BadRequest);
        }

        var (startUtc, endUtc) = _time.GetUtcRange(date.Value);

        var query = _db.TrTransactions.AsNoTracking()
        .Where(t => t.TransactionDate >= startUtc && t.TransactionDate < endUtc);

        if (!isAdmin)
        {
            query = query.Where(t => t.IdUser == IdUser);
        }

        var totalItems = await query.CountAsync();
        var rows = await query
            .OrderBy(t => t.TransactionDate)
            .ThenBy(t => t.InvoiceNo)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new
            {
                t.IdTransaction,
                t.InvoiceNo,
                t.TransactionDate,
                t.IdUser,
                CashierName = t.IdUserNavigation.FullName,
                PaymentMethod = t.IdPaymentMethodNavigation.MethodName,
                t.TotalPrice,
                ItemCount = t.TrTransactionDetails.Sum(d => d.Qty)
            })
            .ToListAsync();

        return new PagedResult<HistoryDto>
        {
            Items = [.. rows.Select(r => new HistoryDto()
            {
                IdTransaction = r.IdTransaction,
                InvoiceNo = r.InvoiceNo,
                TransactionDate = _time.ToLocal(r.TransactionDate),
                IdUser = r.IdUser,
                CashierName = r.CashierName,
                PaymentMethod = r.PaymentMethod,
                ItemCount = r.ItemCount,
                TotalPrice = r.TotalPrice
            })],
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize)
        };
    }

    public async Task<TransactionDto> GetByIdAsync(string IdUser, bool isAdmin, string idTransaction)
    {
        var t = await _db.TrTransactions.AsNoTracking()
            .Include(x => x.IdUserNavigation)
            .Include(x => x.IdPaymentMethodNavigation)
            .Include(x => x.TrTransactionDetails)
            .ThenInclude(d => d.IdProductNavigation)
            .FirstOrDefaultAsync(t => t.IdTransaction == idTransaction && t.Stsrc);

        if (t == null || (!isAdmin && t.IdUser != IdUser))
        {
            throw new AppException("Transaksi tidak ditemukan.", StatusCodes.Status404NotFound);
        }

        return new TransactionDto
        {
            IdTransaction = t.IdTransaction,
            InvoiceNo = t.InvoiceNo,
            TransactionDate = _time.ToLocal(t.TransactionDate),
            IdUser = t.IdUser,
            CashierName = t.IdUserNavigation.FullName,
            IdPaymentMethod = t.IdPaymentMethod,
            PaymentMethodName = t.IdPaymentMethodNavigation.MethodName,
            TotalPrice = t.TotalPrice,
            Items = [.. t.TrTransactionDetails.Select(d => new TransactionItemDto
            {
                IdTransactionDetail = d.IdTransactionDetail,
                IdProduct = d.IdProduct,
                ProductName = d.IdProductNavigation.Name,
                Qty = d.Qty,
                Price = d.Price,
                Discount = d.Discount,
                SubTotal = d.SubTotal
            })]
        };
    }

    public Task<List<PaymentMethodDto>> GetPaymentMethodsAsync()
    {
        return _db.LtPaymentMethods.AsNoTracking()
            .Where(p => p.Stsrc)
            .OrderBy(p => p.IdPaymentMethod)
            .Select(p => new PaymentMethodDto
            {
                IdPaymentMethod = p.IdPaymentMethod,
                MethodName = p.MethodName
            })
            .ToListAsync();
    }

    private static bool IsUniqueViolation(DbUpdateException ex) =>
        ex.InnerException is SqlException { Number: 2601 or 2627 };
}