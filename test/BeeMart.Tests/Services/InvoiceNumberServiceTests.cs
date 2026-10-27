using BeeMart.Api.Common;
using BeeMart.Api.Models;
using BeeMart.Api.Services;
using BeeMart.Tests.Helpers;
using Moq;

namespace BeeMart.Tests.Services;

public class InvoiceNumberServiceTests
{
    private readonly Mock<ITimeService> _timeMock = new();

    public InvoiceNumberServiceTests()
    {
        _timeMock.Setup(t => t.LocalNow).Returns(new DateTime(2026, 10, 26, 10, 30, 0));
    }

    [Fact]
    public async Task GenerateAsync_FirstInvoiceOfTheDay_ReturnsFormattedInvoiceWithSequence001()
    {
        // Arrange
        using var db = TestDbContextFactory.Create();
        var service = new InvoiceNumberServices(db, _timeMock.Object);

        // Act
        var invoiceNo = await service.GenerateAsync("12345");

        // Assert: 5 digits IdUser + 6 digits date (261026) + 3 digits seq (001) = 14 chars
        Assert.Equal("12345261026001", invoiceNo);
    }

    [Fact]
    public async Task GenerateAsync_WhenPriorInvoicesExistToday_IncrementsCrossCashierSequence()
    {
        // Arrange
        using var db = TestDbContextFactory.Create();
        
        // Another cashier (88888) created invoice #004 today
        db.TrTransactions.Add(new TrTransaction
        {
            IdTransaction = Guid.NewGuid().ToString(),
            InvoiceNo = "88888261026004",
            IdUser = "88888",
            TransactionDate = DateTime.UtcNow,
            IdPaymentMethod = 1,
            TotalPrice = 10000,
            Stsrc = true,
            Datein = DateTime.UtcNow,
            Userin = "88888"
        });
        await db.SaveChangesAsync();

        var service = new InvoiceNumberServices(db, _timeMock.Object);

        // Act: Cashier 12345 generates next invoice
        var invoiceNo = await service.GenerateAsync("12345");

        // Assert: Should continue from #004 to #005 with Cashier 12345's ID
        Assert.Equal("12345261026005", invoiceNo);
    }

    [Fact]
    public async Task GenerateAsync_WhenMaxSequenceExceeded_ThrowsConflictException()
    {
        // Arrange
        using var db = TestDbContextFactory.Create();
        db.TrTransactions.Add(new TrTransaction
        {
            IdTransaction = Guid.NewGuid().ToString(),
            InvoiceNo = "12345261026999",
            IdUser = "12345",
            TransactionDate = DateTime.UtcNow,
            IdPaymentMethod = 1,
            TotalPrice = 10000,
            Stsrc = true,
            Datein = DateTime.UtcNow,
            Userin = "12345"
        });
        await db.SaveChangesAsync();

        var service = new InvoiceNumberServices(db, _timeMock.Object);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<AppException>(() => service.GenerateAsync("12345"));
        Assert.Equal("Nomor invoice sudah mencapai batas maksimal.", ex.Message);
        Assert.Equal(409, ex.StatusCode);
    }
}
