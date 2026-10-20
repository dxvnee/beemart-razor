namespace BeeMart.Api.Models;

public class TransactionDetail
{
    public int Id { get; set; }
    public int TransactionId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal SubTotal { get; set; }

    public Transaction Transaction { get; set; } = null!;
    public Product Product { get; set; } = null!;
}