using BeeMart.Api.Models;
using Microsoft.EntityFrameworkCore;
namespace BeeMart.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<LtPaymentMethod> LtPaymentMethods => Set<LtPaymentMethod>();
    public DbSet<MsUser> MsUsers => Set<MsUser>();
    public DbSet<MsProduct> MsProducts => Set<MsProduct>();
    public DbSet<TrTransaction> TrTransactions => Set<TrTransaction>();
    public DbSet<TrTransactionDetail> TrTransactionDetails => Set<TrTransactionDetail>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        mb.Entity<LtPaymentMethod>(e =>
        {
            e.ToTable("LtPaymentMethod");
            e.HasKey(x => x.IdPaymentMethod);

            e.Property(x => x.IdPaymentMethod).ValueGeneratedOnAdd();
            e.Property(x => x.MethodName).HasMaxLength(50).IsRequired();

            e.Property(x => x.Stsrc).IsRequired();
            e.Property(x => x.Datein).IsRequired();
            e.Property(x => x.Userin).HasMaxLength(36).IsRequired();
            e.Property(x => x.Userup).HasMaxLength(36);
        });

        mb.Entity<MsUser>(e =>
        {
            e.ToTable("MsUser");
            e.HasKey(x => x.IdUser);

            e.HasIndex(x => x.Username, "IX_MsUser_Username").IsUnique();

            e.Property(x => x.IdUser).HasMaxLength(5).IsRequired();
            e.Property(x => x.Username).HasMaxLength(50).IsRequired();
            e.Property(x => x.FullName).HasMaxLength(300).IsRequired();
            e.Property(x => x.PasswordHash).HasMaxLength(255).IsRequired();
            e.Property(x => x.Role).HasMaxLength(20).IsRequired();

            e.Property(x => x.Stsrc).IsRequired();
            e.Property(x => x.Datein).IsRequired();
            e.Property(x => x.Userin).HasMaxLength(36).IsRequired();
            e.Property(x => x.Userup).HasMaxLength(36);
        });

        mb.Entity<MsProduct>(e =>
        {
            e.ToTable("MsProduct");
            e.HasKey(x => x.IdProduct);

            e.HasIndex(x => x.Name, "IX_MsProduct_Name");

            e.Property(x => x.IdProduct).HasMaxLength(36).IsRequired();
            e.Property(x => x.Name).HasMaxLength(100).IsRequired();
            e.Property(x => x.Weight).HasPrecision(10, 2).IsRequired();
            e.Property(x => x.Stock).IsRequired();
            e.Property(x => x.Price).HasPrecision(18, 2).IsRequired();
            e.Property(x => x.Discount).HasPrecision(5, 2).HasDefaultValue(0m).IsRequired();

            e.Property(x => x.Stsrc).IsRequired();
            e.Property(x => x.Datein).IsRequired();
            e.Property(x => x.Userin).HasMaxLength(36).IsRequired();
            e.Property(x => x.Userup).HasMaxLength(36);
        });

        mb.Entity<TrTransaction>(e =>
        {
            e.ToTable("TrTransaction");
            e.HasKey(x => x.IdTransaction);

            e.HasIndex(x => x.InvoiceNo, "IX_TrTransaction_InvoiceNo").IsUnique();
            e.HasIndex(x => x.TransactionDate, "IX_TrTransaction_TransactionDate");
            e.HasIndex(x => x.IdUser, "IX_TrTransaction_IdUser");
            e.HasIndex(x => x.IdPaymentMethod, "IX_TrTransaction_IdPaymentMethod");

            e.Property(x => x.IdTransaction).HasMaxLength(36).IsRequired();
            e.Property(x => x.InvoiceNo).HasMaxLength(20).IsRequired();
            e.Property(x => x.TransactionDate).IsRequired();
            e.Property(x => x.IdUser).HasMaxLength(5).IsRequired();
            e.Property(x => x.IdPaymentMethod).IsRequired();
            e.Property(x => x.TotalPrice).HasPrecision(18, 2).IsRequired();

            e.Property(x => x.Stsrc).IsRequired();
            e.Property(x => x.Datein).IsRequired();
            e.Property(x => x.Userin).HasMaxLength(36).IsRequired();
            e.Property(x => x.Userup).HasMaxLength(36);

            e.HasOne(x => x.IdUserNavigation)
                .WithMany(u => u.TrTransactions)
                .HasForeignKey(x => x.IdUser)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TrTransaction_MsUser_IdUser");

            e.HasOne(x => x.IdPaymentMethodNavigation)
                .WithMany(p => p.TrTransactions)
                .HasForeignKey(x => x.IdPaymentMethod)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TrTransaction_LtPaymentMethod_IdPaymentMethod");
        });

        mb.Entity<TrTransactionDetail>(e =>
        {
            e.ToTable("TrTransactionDetail");
            e.HasKey(x => x.IdTransactionDetail);

            e.HasIndex(x => x.IdTransaction, "IX_TrTransactionDetail_IdTransaction");
            e.HasIndex(x => x.IdProduct, "IX_TrTransactionDetail_IdProduct");

            e.Property(x => x.IdTransactionDetail).HasMaxLength(36).IsRequired();
            e.Property(x => x.IdTransaction).HasMaxLength(36).IsRequired();
            e.Property(x => x.IdProduct).HasMaxLength(36).IsRequired();
            e.Property(x => x.Qty).IsRequired();
            e.Property(x => x.Price).HasPrecision(18, 2).IsRequired();
            e.Property(x => x.Discount).HasPrecision(5, 2).HasDefaultValue(0m).IsRequired();
            e.Property(x => x.SubTotal).HasPrecision(18, 2).IsRequired();

            e.Property(x => x.Stsrc).IsRequired();
            e.Property(x => x.Datein).IsRequired();
            e.Property(x => x.Userin).HasMaxLength(36).IsRequired();
            e.Property(x => x.Userup).HasMaxLength(36);

            e.HasOne(x => x.IdTransactionNavigation)
                .WithMany(t => t.TrTransactionDetails)
                .HasForeignKey(x => x.IdTransaction)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_TrTransactionDetail_TrTransaction_IdTransaction");

            e.HasOne(x => x.IdProductNavigation)
                .WithMany(p => p.TrTransactionDetails)
                .HasForeignKey(x => x.IdProduct)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TrTransactionDetail_MsProduct_IdProduct");
        });
    }

}