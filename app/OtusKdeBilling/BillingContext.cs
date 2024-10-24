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
        modelBuilder.Entity<Payments>();
    }

    public DbSet<Payments> Payments { get; set; }
    public DbSet<Wallets> Wallets { get; set; }
}