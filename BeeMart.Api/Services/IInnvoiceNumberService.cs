namespace BeeMart.Api.Services;

public interface IInvoiceNumberService
{
    Task<string> GenerateAsync(string IdUser);
}