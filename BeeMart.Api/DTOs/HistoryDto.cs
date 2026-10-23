namespace BeeMart.Api.DTOs;

public class HistoryDto
{
    public string IdTransaction { get; set; } = null!;
    public string InvoiceNo { get; set; } = null!;
    public DateTime TransactionDate { get; set; }
    public string IdUser { get; set; } = null!;
    public string CashierName { get; set; } = null!;
    public string PaymentMethod { get; set; } = null!;
    public int ItemCount { get; set; }
    public decimal TotalPrice { get; set; }
}