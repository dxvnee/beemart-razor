namespace BeeMart.Api.Services;

public interface IInvoiceNumberService
{
    Task<String> GenerateAsync(int userId);
}