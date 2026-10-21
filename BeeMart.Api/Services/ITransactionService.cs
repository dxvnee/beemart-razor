using BeeMart.Api.DTOs;

namespace BeeMart.Api.Services;

public interface ITransactionService
{
    Task<NextInvoiceDto> GetNextInvoiceAsync(int userId);
    Task<TransactionDto> CreateAsync(int userId, CreateTransactionRequest request);
}