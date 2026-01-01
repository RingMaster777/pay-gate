using PayGate.Models;
using Microsoft.EntityFrameworkCore;

namespace PayGate.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Merchant> Merchants => Set<Merchant>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<Refund> Refunds => Set<Refund>();
    public DbSet<WebhookLog> WebhookLogs => Set<WebhookLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure table names to match PostgreSQL naming convention
        modelBuilder.Entity<Merchant>().ToTable("merchants");
        modelBuilder.Entity<Transaction>().ToTable("transactions");
        modelBuilder.Entity<Refund>().ToTable("refunds");
        modelBuilder.Entity<WebhookLog>().ToTable("webhook_logs");

        // Configure indexes
        modelBuilder.Entity<Transaction>()
            .HasIndex(t => t.MerchantId)
            .HasDatabaseName("idx_transactions_merchant");

        modelBuilder.Entity<Transaction>()
            .HasIndex(t => t.Status)
            .HasDatabaseName("idx_transactions_status");

        // Configure decimal precision
        modelBuilder.Entity<Transaction>()
            .Property(t => t.Amount)
            .HasPrecision(10, 2);

        modelBuilder.Entity<Refund>()
            .Property(r => r.Amount)
            .HasPrecision(10, 2);
    }
}
