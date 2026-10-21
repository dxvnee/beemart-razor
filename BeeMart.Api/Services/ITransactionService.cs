using BeeMart.Api.DTOs;

namespace BeeMart.Api.Services;

public interface ITransactionService
{
    Task<NextInvoiceDto> GetNextInvoiceAsync(int userId);
    Task<TransactionDto> CreateAsync(int userId, CreateTransactionRequest request);
    Task<PagedResult<TransactionSummaryDto>> GetHistoryAsync(int userId, bool isAdmin, DateOnly? date, int page, int pageSize);
    Task<TransactionDto> GetByIdAsync(int userId, bool isAdmin, int id);
}
