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
        var query = _db.Products.AsNoTracking();

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
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                Stock = p.Stock,
                ImageFileName = p.ImageFileName
            })
            .ToListAsync();
    }

    public async Task<ProductDto> GetByIdAsync(int id)
    {
        var product = await _db.Products.AsNoTracking()
            .Where(p => p.Id == id)
            .Select(p => new ProductDto{
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                Stock = p.Stock,
                ImageFileName = p.ImageFileName
            })
            .FirstOrDefaultAsync();

        return product ?? throw new AppException("Produk tidak ditemukan", StatusCodes.Status404NotFound);
    }
}