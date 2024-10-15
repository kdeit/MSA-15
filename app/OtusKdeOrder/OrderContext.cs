using Microsoft.EntityFrameworkCore;

namespace OtusKdeDAL;

public class OrderContext : DbContext
{
    public OrderContext()
    {
    }

    public OrderContext(DbContextOptions<OrderContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>();
    }

    public DbSet<Order> Orders { get; set; }
}