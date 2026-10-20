namespace BeeMart.Api.Models;

public class Transaction
{
    public int Id { get; set; }
    public string InvoiceNumber { get; set; } = null!;
    public int UserId { get; set; } = 0;
    public DateTime TransactionDate { get; set; } = DateTime.Now;
    public PaymentMethod PaymentMethod { get; set; }
    public decimal TotalAmount { get; set; } = 0;
    public User User { get; set; } = null!;

    public ICollection<TransactionDetail> Details { get; set; } = new List<TransactionDetail>();
}