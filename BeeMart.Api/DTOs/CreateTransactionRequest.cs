using System.ComponentModel.DataAnnotations;

namespace BeeMart.Api.DTOs;

public class CreateTransactionRequest
{
    [Required(ErrorMessage = "Metode pembayaran wajib dipilih.")]
    [Range(1, int.MaxValue, ErrorMessage = "Metode pembayaran tidak valid.")]
    public int IdPaymentMethod { get; set; }

    [Required(ErrorMessage = "Keranjang tidak boleh kosong.")]
    [MinLength(1, ErrorMessage = "Keranjang tidak boleh kosong.")]
    [MaxLength(50, ErrorMessage = "Maksimal 50 jenis barang per transaksi.")]
    public List<TransactionItemRequest> Items { get; set; } = [];
}

public class TransactionItemRequest
{
    [Required(ErrorMessage = "ID Produk wajib diisi.")]
    [StringLength(36, MinimumLength = 36, ErrorMessage = "Format ID Produk tidak valid (harus 36 karakter GUID).")]
    public string IdProduct { get; set; } = null!;

    [Range(1, 9999, ErrorMessage = "Quantity harus antara 1 dan 9999.")]
    public int Qty { get; set; }
}