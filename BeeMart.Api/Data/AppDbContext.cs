using BeeMart.Api.Models;
using Microsoft.EntityFrameworkCore;
namespace BeeMart.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<TransactionDetail> TransactionDetails => Set<TransactionDetail>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        mb.Entity<User>(e =>
        {
            e.ToTable("Users");
            e.HasIndex(x => x.Username).IsUnique();
            e.Property(x => x.Username).HasMaxLength(50).IsRequired();
            e.Property(x => x.PasswordHash).HasMaxLength(200).IsRequired();
            e.Property(x => x.Fullname).HasMaxLength(100).IsRequired();
            e.Property(x => x.Role).HasConversion<string>().HasMaxLength(10);
        });

        mb.Entity<Product>(e =>
        {
            e.ToTable("Products");
            e.Property(x => x.Name).HasMaxLength(150).IsRequired();
            e.Property(x => x.Price).HasPrecision(18, 2);
            e.Property(x => x.ImageFileName).HasMaxLength(200);
        });

        mb.Entity<Transaction>(e =>
        {
            e.ToTable("Transactions");
            e.HasIndex(x => x.InvoiceNumber).IsUnique();
            e.Property(x => x.InvoiceNumber).HasMaxLength(14).IsRequired();
            e.Property(x => x.PaymentMethod).HasConversion<string>().HasMaxLength(10);
            e.Property(x => x.TotalAmount).HasPrecision(18, 2);
            e.HasOne(x => x.User).WithMany(u => u.Transactions).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
        });

        mb.Entity<TransactionDetail>(e =>
        {
            e.ToTable("TransactionDetails");
            e.Property(x => x.UnitPrice).HasPrecision(18, 2);
            e.Property(x => x.SubTotal).HasPrecision(18, 2);
            e.HasOne(x => x.Transaction).WithMany(t => t.Details).HasForeignKey(x => x.TransactionId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Product).WithMany(p => p.Details).HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);
        });
    }

}