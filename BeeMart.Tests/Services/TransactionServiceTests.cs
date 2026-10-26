using BeeMart.Api.Common;
using BeeMart.Api.DTOs;
using BeeMart.Api.Models;
using BeeMart.Api.Services;
using BeeMart.Tests.Helpers;
using Moq;

namespace BeeMart.Tests.Services;

public class TransactionServiceTests
{
    private readonly Mock<IInvoiceNumberService> _invoiceMock = new();
    private readonly Mock<ITimeService> _timeMock = new();
    private readonly DateTime _fixedUtc = new(2026, 10, 26, 8, 0, 0, DateTimeKind.Utc);

    public TransactionServiceTests()
    {
        _timeMock.Setup(t => t.UtcNow).Returns(_fixedUtc);
        _timeMock.Setup(t => t.LocalNow).Returns(_fixedUtc.AddHours(7));
        _timeMock.Setup(t => t.ToLocal(It.IsAny<DateTime>()))
            .Returns<DateTime>(dt => dt.AddHours(7));
        _timeMock.Setup(t => t.GetUtcRange(new DateOnly(2026, 10, 26)))
            .Returns((new DateTime(2026, 10, 25, 17, 0, 0, DateTimeKind.Utc), new DateTime(2026, 10, 26, 17, 0, 0, DateTimeKind.Utc)));
        _invoiceMock.Setup(i => i.GenerateAsync(It.IsAny<string>()))
            .ReturnsAsync("10001261026001");
    }

    private static async Task SeedCommonDataAsync(BeeMart.Api.Data.AppDbContext db)
    {
        db.MsUsers.Add(new MsUser
        {
            IdUser = "10001",
            Username = "kasir1",
            FullName = "Kasir Satu",
            PasswordHash = "hash",
            Role = "Cashier",
            Stsrc = true,
            Datein = DateTime.UtcNow,
            Userin = "SYSTEM"
        });

        db.MsUsers.Add(new MsUser
        {
            IdUser = "20002",
            Username = "kasir2",
            FullName = "Kasir Dua",
            PasswordHash = "hash",
            Role = "Cashier",
            Stsrc = true,
            Datein = DateTime.UtcNow,
            Userin = "SYSTEM"
        });

        db.LtPaymentMethods.Add(new LtPaymentMethod
        {
            IdPaymentMethod = 1,
            MethodName = "Cash",
            Stsrc = true,
            Datein = DateTime.UtcNow,
            Userin = "SYSTEM"
        });

        await db.SaveChangesAsync();
    }

    [Fact]
    public async Task CreateAsync_ValidRequest_DeductsStockCalculatesDiscountAndSaves()
    {
        // Arrange
        using var db = TestDbContextFactory.Create();
        await SeedCommonDataAsync(db);

        var product = new MsProduct
        {
            IdProduct = "P01",
            Name = "Sabun Mandi",
            Price = 10000m,
            Stock = 10,
            Discount = 10m, // 10%
            Weight = 100m,
            Stsrc = true,
            Datein = DateTime.UtcNow,
            Userin = "SYSTEM"
        };
        db.MsProducts.Add(product);
        await db.SaveChangesAsync();

        var service = new TransactionService(db, _invoiceMock.Object, _timeMock.Object);
        var request = new CreateTransactionRequest
        {
            IdPaymentMethod = 1,
            Items =
            [
                new TransactionItemRequest { IdProduct = "P01", Qty = 2 }
            ]
        };

        // Act
        var result = await service.CreateAsync("10001", request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("10001261026001", result.InvoiceNo);
        Assert.Equal("Kasir Satu", result.CashierName);
        Assert.Equal("Cash", result.PaymentMethodName);

        // Price calculation: 2 * 10.000 = 20.000, 10% discount = 2.000 -> SubTotal = 18.000
        Assert.Equal(18000m, result.TotalPrice);
        Assert.Single(result.Items);
        Assert.Equal(18000m, result.Items[0].SubTotal);

        // Stock in database must be deducted: 10 - 2 = 8
        var updatedProduct = await db.MsProducts.FindAsync("P01");
        Assert.NotNull(updatedProduct);
        Assert.Equal(8, updatedProduct.Stock);
        Assert.Equal("10001", updatedProduct.Userup);
    }

    [Fact]
    public async Task CreateAsync_WhenQuantityExceedsStock_ThrowsAppException()
    {
        // Arrange
        using var db = TestDbContextFactory.Create();
        await SeedCommonDataAsync(db);

        db.MsProducts.Add(new MsProduct
        {
            IdProduct = "P02",
            Name = "Minyak Goreng",
            Price = 25000m,
            Stock = 3, // only 3 in stock
            Discount = 0m,
            Weight = 1000m,
            Stsrc = true,
            Datein = DateTime.UtcNow,
            Userin = "SYSTEM"
        });
        await db.SaveChangesAsync();

        var service = new TransactionService(db, _invoiceMock.Object, _timeMock.Object);
        var request = new CreateTransactionRequest
        {
            IdPaymentMethod = 1,
            Items =
            [
                new TransactionItemRequest { IdProduct = "P02", Qty = 5 } // 5 > 3
            ]
        };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<AppException>(() => service.CreateAsync("10001", request));
        Assert.Equal("Jumlah barang yang dimasukkan melebihi sisa stok.", ex.Message);
        Assert.Equal(400, ex.StatusCode);

        // Verify stock remains untouched
        var prod = await db.MsProducts.FindAsync("P02");
        Assert.Equal(3, prod!.Stock);
    }

    [Fact]
    public async Task CreateAsync_WhenProductNotFound_ThrowsNotFoundException()
    {
        // Arrange
        using var db = TestDbContextFactory.Create();
        await SeedCommonDataAsync(db);

        var service = new TransactionService(db, _invoiceMock.Object, _timeMock.Object);
        var request = new CreateTransactionRequest
        {
            IdPaymentMethod = 1,
            Items =
            [
                new TransactionItemRequest { IdProduct = "NON-EXISTENT", Qty = 1 }
            ]
        };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<AppException>(() => service.CreateAsync("10001", request));
        Assert.Contains("tidak ditemukan", ex.Message);
        Assert.Equal(404, ex.StatusCode);
    }

    [Fact]
    public async Task GetHistoryAsync_WhenDateIsNull_ThrowsAppException()
    {
        // Arrange
        using var db = TestDbContextFactory.Create();
        var service = new TransactionService(db, _invoiceMock.Object, _timeMock.Object);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<AppException>(() =>
            service.GetHistoryAsync("10001", isAdmin: false, date: null, page: 1, pageSize: 10));

        Assert.Equal("Parameter tanggal wajib diisi.", ex.Message);
        Assert.Equal(400, ex.StatusCode);
    }

    [Fact]
    public async Task GetHistoryAsync_CashierRole_FiltersOnlyOwnTransactions_AndItemCountIsSumOfQty()
    {
        // Arrange
        using var db = TestDbContextFactory.Create();
        await SeedCommonDataAsync(db);

        // Transaction 1 by Cashier 10001 (has 2 details: Qty 3 + Qty 2 = 5 items)
        var t1 = new TrTransaction
        {
            IdTransaction = "T01",
            InvoiceNo = "10001261026001",
            IdUser = "10001",
            IdPaymentMethod = 1,
            TransactionDate = _fixedUtc,
            TotalPrice = 50000m,
            Stsrc = true,
            Datein = _fixedUtc,
            Userin = "10001",
            TrTransactionDetails =
            [
                new TrTransactionDetail { IdTransactionDetail = "D1", IdProduct = "P1", Qty = 3, Price = 10000, SubTotal = 30000, Stsrc = true, Datein = _fixedUtc, Userin = "10001" },
                new TrTransactionDetail { IdTransactionDetail = "D2", IdProduct = "P2", Qty = 2, Price = 10000, SubTotal = 20000, Stsrc = true, Datein = _fixedUtc, Userin = "10001" }
            ]
        };

        // Transaction 2 by Cashier 20002 (has 1 detail: Qty 1)
        var t2 = new TrTransaction
        {
            IdTransaction = "T02",
            InvoiceNo = "20002261026002",
            IdUser = "20002",
            IdPaymentMethod = 1,
            TransactionDate = _fixedUtc.AddMinutes(5),
            TotalPrice = 10000m,
            Stsrc = true,
            Datein = _fixedUtc,
            Userin = "20002",
            TrTransactionDetails =
            [
                new TrTransactionDetail { IdTransactionDetail = "D3", IdProduct = "P1", Qty = 1, Price = 10000, SubTotal = 10000, Stsrc = true, Datein = _fixedUtc, Userin = "20002" }
            ]
        };

        db.TrTransactions.AddRange(t1, t2);
        await db.SaveChangesAsync();

        var service = new TransactionService(db, _invoiceMock.Object, _timeMock.Object);

        // Act: Cashier 10001 asks for history
        var result = await service.GetHistoryAsync("10001", isAdmin: false, date: new DateOnly(2026, 10, 26), page: 1, pageSize: 10);

        // Assert
        Assert.Equal(1, result.TotalItems);
        Assert.Single(result.Items);
        Assert.Equal("10001261026001", result.Items[0].InvoiceNo);
        // Verify ItemCount is sum of Qty (3 + 2 = 5), not number of line items
        Assert.Equal(5, result.Items[0].ItemCount);
    }

    [Fact]
    public async Task GetHistoryAsync_AdminRole_ReturnsAllTransactions()
    {
        // Arrange
        using var db = TestDbContextFactory.Create();
        await SeedCommonDataAsync(db);

        var t1 = new TrTransaction
        {
            IdTransaction = "T01",
            InvoiceNo = "10001261026001",
            IdUser = "10001",
            IdPaymentMethod = 1,
            TransactionDate = _fixedUtc,
            TotalPrice = 50000m,
            Stsrc = true,
            Datein = _fixedUtc,
            Userin = "10001"
        };
        var t2 = new TrTransaction
        {
            IdTransaction = "T02",
            InvoiceNo = "20002261026002",
            IdUser = "20002",
            IdPaymentMethod = 1,
            TransactionDate = _fixedUtc.AddMinutes(10),
            TotalPrice = 20000m,
            Stsrc = true,
            Datein = _fixedUtc,
            Userin = "20002"
        };
        db.TrTransactions.AddRange(t1, t2);
        await db.SaveChangesAsync();

        var service = new TransactionService(db, _invoiceMock.Object, _timeMock.Object);

        // Act: Admin asks for history
        var result = await service.GetHistoryAsync("ADMIN", isAdmin: true, date: new DateOnly(2026, 10, 26), page: 1, pageSize: 10);

        // Assert: Sees all 2 transactions from both cashiers
        Assert.Equal(2, result.TotalItems);
        Assert.Equal(2, result.Items.Count);
    }

    [Fact]
    public async Task GetByIdAsync_WhenCashierAccessesOtherCashierTransaction_ThrowsNotFound()
    {
        // Arrange
        using var db = TestDbContextFactory.Create();
        await SeedCommonDataAsync(db);

        var t = new TrTransaction
        {
            IdTransaction = "TX99",
            InvoiceNo = "20002261026099",
            IdUser = "20002", // Belongs to cashier 20002
            IdPaymentMethod = 1,
            TransactionDate = _fixedUtc,
            TotalPrice = 10000m,
            Stsrc = true,
            Datein = _fixedUtc,
            Userin = "20002"
        };
        db.TrTransactions.Add(t);
        await db.SaveChangesAsync();

        var service = new TransactionService(db, _invoiceMock.Object, _timeMock.Object);

        // Act & Assert: Cashier 10001 cannot view Cashier 20002's transaction
        var ex = await Assert.ThrowsAsync<AppException>(() =>
            service.GetByIdAsync("10001", isAdmin: false, "TX99"));

        Assert.Equal("Transaksi tidak ditemukan.", ex.Message);
        Assert.Equal(404, ex.StatusCode);
    }
}
