using BeeMart.Api.Common;
using BeeMart.Api.Data;
using BeeMart.Api.DTOs;
using Microsoft.EntityFrameworkCore;

namespace BeeMart.Api.Services;

public class ProductService(AppDbContext db) : IProductService
{
    private readonly AppDbContext _db = db;

    public async Task<List<ProductDto>> SearchAsync(string? search)
    {
        var query = _db.MsProducts.AsNoTracking().Where(p => p.Stsrc);

        if(!string.IsNullOrWhiteSpace(search))
        {
            var keyword = search.Trim();
            query = query.Where(p => p.Name.Contains(keyword));
        }

        return await query
            .OrderBy(p => p.Name)
            .Take(50)
            .Select(p => new ProductDto
            {
                IdProduct = p.IdProduct,
                Name = p.Name,
                Price = p.Price,
                Stock = p.Stock,
                Discount = p.Discount,
                Weight = p.Weight,
            })
            .ToListAsync();
    }

    public async Task<ProductDto> GetByIdAsync(string id)
    {
        var product = await _db.MsProducts.AsNoTracking()
            .Where(p => p.IdProduct == id && p.Stsrc)
            .Select(p => new ProductDto{
                IdProduct = p.IdProduct,
                Name = p.Name,
                Price = p.Price,
                Stock = p.Stock,
                Discount = p.Discount,
                Weight = p.Weight,
            })
            .FirstOrDefaultAsync();

        return product ?? throw new AppException("Produk tidak ditemukan", StatusCodes.Status404NotFound);
    }
}