using BeeMart.Api.DTOs;

namespace BeeMart.Api.Services;

public interface IProductService
{
    Task<List<ProductDto>> SearchAsync(string? keyword);
    Task<ProductDto> GetByIdAsync(int id);
}