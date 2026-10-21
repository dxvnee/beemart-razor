namespace BeeMart.Api.DTOs;

public class PagedResult<T>
{
    public List<T> Items { get; set; } = [];
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalItems { get; set; }
    public int TotalPages { get; set; }
}

public class TransactionSummaryDto
{
    public int Id { get; set; }
    public string InvoiceNumber { get; set; } = null!;
    public DateTimeOffset TransactionDate { get; set; }
    public string CashierName { get; set; } = null!;
    public string PaymentMethod { get; set; } = null!;
    public int ItemCount { get; set; }
    public decimal TotalAmount { get; set; }
}