namespace BeeMart.WebClient.Models;

public class ProductItem
{
    public string IdProduct { get; set; } = "";
    public string Name { get; set; } = "";
    public decimal Weight { get; set; }
    public int Stock { get; set; }
    public decimal Price { get; set; }
    public decimal Discount { get; set; }
}
