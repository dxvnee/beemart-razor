namespace BeeMart.Api.Models;
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public decimal Price { get; set; } = 0;
    public int Stock { get; set; } = 0;
    public string? ImageFileName { get; set; }

    public ICollection<TransactionDetail> Details { get; set; } = new List<TransactionDetail>();
}