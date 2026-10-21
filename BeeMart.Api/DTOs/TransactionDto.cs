namespace BeeMart.Api.DTOs;

public class TransactionDto
{
    public int Id { get; set; }
    public string InvoiceNumber { get; set; } = null!;
    public DateTimeOffset TransactionDate { get; set; }
    public int CashierId { get; set; }
    public string CashierName { get; set; } = null!;
    public string PaymentMethod { get; set; } = null!;
    public decimal TotalAmount { get; set; }
    public List<TransactionItemDto> Items { get; set; } = [];
}

public class TransactionItemDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = null!;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal SubTotal { get; set; }
}

public class NextInvoiceDto
{
    public string InvoiceNumber { get; set; } = null!;
}