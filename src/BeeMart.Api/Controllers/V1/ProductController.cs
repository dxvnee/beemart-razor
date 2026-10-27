using Asp.Versioning;
using BeeMart.Api.Common;
using BeeMart.Api.DTOs;
using BeeMart.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BeeMart.Api.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/products")]
[Authorize]
public class ProductController(IProductService products) : ControllerBase
{
    private readonly IProductService _products = products;

    [HttpGet]
    public async Task<IActionResult> Search([FromQuery] string? search)
    {
        var result = await _products.SearchAsync(search);
        return Ok(ApiResponse<List<ProductDto>>.Ok(result));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var result = await _products.GetByIdAsync(id);
        return Ok(ApiResponse<ProductDto>.Ok(result));
    }
}