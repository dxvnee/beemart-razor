using BeeMart.Api.DTOs;

namespace BeeMart.Api.Services;

public interface ITransactionService
{
    Task<NextInvoiceDto> GetNextInvoiceAsync(string IdUser);
    Task<TransactionDto> CreateAsync(string IdUser, CreateTransactionRequest request);
    Task<PagedResult<HistoryDto>> GetHistoryAsync(string IdUser, bool isAdmin, DateOnly? date, int page, int pageSize);
    Task<TransactionDto> GetByIdAsync(string IdUser, bool isAdmin, string IdTransaction);
    Task<List<PaymentMethodDto>> GetPaymentMethodsAsync();
}
