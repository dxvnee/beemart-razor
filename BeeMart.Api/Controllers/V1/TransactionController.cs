using Asp.Versioning;
using BeeMart.Api.Common;
using BeeMart.Api.DTOs;
using BeeMart.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BeeMart.Api.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/transactions")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class TransactionController(ITransactionService transaction) : ControllerBase
{
    private readonly ITransactionService _transaction = transaction;

    private string CurrentUserId =>
        User.FindFirst("sub")?.Value
        ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
        ?? throw new AppException("Identitas pengguna tidak ditemukan.", StatusCodes.Status401Unauthorized);

    private bool IsAdmin => User.IsInRole("Admin");

    [HttpGet("payment-methods")]
    public async Task<IActionResult> GetPaymentMethods()
    {
        var result = await _transaction.GetPaymentMethodsAsync();
        return Ok(ApiResponse<List<PaymentMethodDto>>.Ok(result));
    }

    [HttpGet("next-invoice")]
    public async Task<IActionResult> NextInvoice()
    {
        var result = await _transaction.GetNextInvoiceAsync(CurrentUserId);
        return Ok(ApiResponse<NextInvoiceDto>.Ok(result));
    }

    [HttpGet]
    public async Task<IActionResult> GetHistory(
        [FromQuery] DateOnly? date,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _transaction.GetHistoryAsync(CurrentUserId, IsAdmin, date, page, pageSize);
        return Ok(ApiResponse<PagedResult<HistoryDto>>.Ok(result));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var result = await _transaction.GetByIdAsync(CurrentUserId, IsAdmin, id);
        return Ok(ApiResponse<TransactionDto>.Ok(result));
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateTransactionRequest request)
    {
        var result = await _transaction.CreateAsync(CurrentUserId, request);
        return StatusCode(StatusCodes.Status201Created, ApiResponse<TransactionDto>.Ok(result, "Transaksi berhasil disimpan."));
    }
}