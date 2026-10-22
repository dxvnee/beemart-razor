namespace BeeMart.WebClient.Models;

public class SaveTransactionRequest
{
    public string PaymentMethod { get; set; } = "";
    public List<SaveTransactionItem> Items { get; set; } = [];
}
