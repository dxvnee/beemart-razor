namespace BeeMart.WebClient.Models;

public class SummaryItem
{
    public string IdTransaction { get; set; } = "";
    public string InvoiceNo { get; set; } = "";
    public DateTime TransactionDate { get; set; }
    public string CashierName { get; set; } = "";
    public string PaymentMethod { get; set; } = "";
    public int ItemCount { get; set; }
    public decimal TotalPrice { get; set; }
}
