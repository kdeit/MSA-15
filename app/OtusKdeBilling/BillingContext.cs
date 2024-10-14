using Microsoft.EntityFrameworkCore;

namespace OtusKdeDAL;

public class BillingContext : DbContext
{
    public BillingContext()
    {
    }

    public BillingContext(DbContextOptions<BillingContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Wallet>().HasIndex(u => u.UserId).IsUnique();
    }

    public DbSet<Wallet> Wallets { get; set; }
}