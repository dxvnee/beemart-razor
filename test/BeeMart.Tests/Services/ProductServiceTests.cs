using BeeMart.Api.Common;
using BeeMart.Api.Models;
using BeeMart.Api.Services;
using BeeMart.Tests.Helpers;

namespace BeeMart.Tests.Services;

public class ProductServiceTests
{
    [Fact]
    public async Task SearchAsync_FiltersByKeyword_ExcludesInactive_AndLimitsTo10()
    {
        // Arrange
        using var db = TestDbContextFactory.Create();

        // Add 12 active matching products
        for (int i = 1; i <= 12; i++)
        {
            db.MsProducts.Add(new MsProduct
            {
                IdProduct = $"PROD-{i:D3}",
                Name = $"Chiki Rasa Keju {i:D2}",
                Price = 5000,
                Stock = 20,
                Discount = 0,
                Weight = 50,
                Stsrc = true,
                Datein = DateTime.UtcNow,
                Userin = "SYSTEM"
            });
        }

        // Add 1 inactive matching product
        db.MsProducts.Add(new MsProduct
        {
            IdProduct = "PROD-INACTIVE",
            Name = "Chiki Rasa Keju Inactive",
            Price = 5000,
            Stock = 20,
            Discount = 0,
            Weight = 50,
            Stsrc = false, // Inactive
            Datein = DateTime.UtcNow,
            Userin = "SYSTEM"
        });

        // Add 1 active non-matching product
        db.MsProducts.Add(new MsProduct
        {
            IdProduct = "PROD-OTHER",
            Name = "Susu UHT",
            Price = 8000,
            Stock = 15,
            Discount = 0,
            Weight = 200,
            Stsrc = true,
            Datein = DateTime.UtcNow,
            Userin = "SYSTEM"
        });

        await db.SaveChangesAsync();

        var service = new ProductService(db);

        // Act
        var result = await service.SearchAsync("Chiki");

        // Assert
        Assert.Equal(10, result.Count); // Should be capped at 10 items
        Assert.All(result, p => Assert.Contains("Chiki", p.Name));
        Assert.DoesNotContain(result, p => p.IdProduct == "PROD-INACTIVE");
        Assert.DoesNotContain(result, p => p.IdProduct == "PROD-OTHER");
    }

    [Fact]
    public async Task GetByIdAsync_WhenActiveProductExists_ReturnsProductDto()
    {
        // Arrange
        using var db = TestDbContextFactory.Create();
        db.MsProducts.Add(new MsProduct
        {
            IdProduct = "P001",
            Name = "Indomie Goreng",
            Price = 3500,
            Stock = 50,
            Discount = 10,
            Weight = 85,
            Stsrc = true,
            Datein = DateTime.UtcNow,
            Userin = "SYSTEM"
        });
        await db.SaveChangesAsync();

        var service = new ProductService(db);

        // Act
        var result = await service.GetByIdAsync("P001");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("P001", result.IdProduct);
        Assert.Equal("Indomie Goreng", result.Name);
        Assert.Equal(3500, result.Price);
        Assert.Equal(10, result.Discount);
    }

    [Fact]
    public async Task GetByIdAsync_WhenProductNotFoundOrInactive_ThrowsNotFoundException()
    {
        // Arrange
        using var db = TestDbContextFactory.Create();
        db.MsProducts.Add(new MsProduct
        {
            IdProduct = "P-INACTIVE",
            Name = "Produk Nonaktif",
            Price = 1000,
            Stock = 10,
            Discount = 0,
            Weight = 10,
            Stsrc = false,
            Datein = DateTime.UtcNow,
            Userin = "SYSTEM"
        });
        await db.SaveChangesAsync();

        var service = new ProductService(db);

        // Act & Assert for non-existent product
        var ex1 = await Assert.ThrowsAsync<AppException>(() => service.GetByIdAsync("NON-EXISTENT"));
        Assert.Equal("Produk tidak ditemukan", ex1.Message);
        Assert.Equal(404, ex1.StatusCode);

        // Act & Assert for inactive product
        var ex2 = await Assert.ThrowsAsync<AppException>(() => service.GetByIdAsync("P-INACTIVE"));
        Assert.Equal("Produk tidak ditemukan", ex2.Message);
        Assert.Equal(404, ex2.StatusCode);
    }
}
