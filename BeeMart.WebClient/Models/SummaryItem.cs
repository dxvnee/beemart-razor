namespace BeeMart.WebClient.Models;

public class SummaryItem
{
    public int Id { get; set; }
    public string InvoiceNumber { get; set; } = "";
    public DateTimeOffset TransactionDate { get; set; }
    public string CashierName { get; set; } = "";
    public string PaymentMethod { get; set; } = "";
    public int ItemCount { get; set; }
    public decimal TotalAmount { get; set; }
}
