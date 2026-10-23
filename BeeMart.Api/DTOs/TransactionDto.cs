namespace BeeMart.Api.DTOs;

public class TransactionDto
{
    public string IdTransaction { get; set; } = null!;
    public string InvoiceNo { get; set; } = null!;
    public DateTime TransactionDate { get; set; }
    public string IdUser { get; set; } = null!;
    public string CashierName { get; set; } = null!;
    public int IdPaymentMethod { get; set; }
    public string PaymentMethodName { get; set; } = null!;
    public decimal TotalPrice { get; set; }
    public List<TransactionItemDto> Items { get; set; } = [];
}

public class TransactionItemDto
{
    public string IdTransactionDetail { get; set; } = null!;
    public string IdProduct { get; set; } = null!;
    public string ProductName { get; set; } = null!;
    public int Qty { get; set; }
    public decimal Price { get; set; }
    public decimal Discount { get; set; }
    public decimal SubTotal { get; set; }
}

public class NextInvoiceDto
{
    public string InvoiceNo { get; set; } = null!;
}