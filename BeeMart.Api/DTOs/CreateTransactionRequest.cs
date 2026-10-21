using System.ComponentModel.DataAnnotations;

namespace BeeMart.Api.DTOs;

public class CreateTransactionRequest
{
    [Required(ErrorMessage = "Metode pembayaran wajib dipilih.")]
    [RegularExpression(@"(?i)^(Cash|Debit|Credit)$", ErrorMessage = "Metode pembayaran harus Cash, Debit, atau Credit.")]
    public string PaymentMethod { get; set; } = null!;

    [Required(ErrorMessage = "Keranjang tidak boleh kosong.")]
    [MinLength(1, ErrorMessage = "Keranjang tidak boleh kosong.")]
    [MaxLength(50, ErrorMessage = "Maksimal 50 jenis barang per transaksi.")]
    public List<TransactionItemRequest> Items { get; set; } = [];
}

public class TransactionItemRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "Product ID tidak valid.")]
    public int ProductId { get; set; }

    [Range(1, 9999, ErrorMessage = "Quantity harus antara 1 dan 9999.")]
    public int Quantity { get; set; }
}