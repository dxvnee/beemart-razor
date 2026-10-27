namespace BeeMart.WebClient.Models;

public class SaveTransactionRequest
{
    public int IdPaymentMethod { get; set; }
    public List<SaveTransactionItem> Items { get; set; } = [];
}
