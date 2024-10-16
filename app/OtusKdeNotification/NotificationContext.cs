using Microsoft.EntityFrameworkCore;

namespace OtusKdeDAL;

public class NotificationContext : DbContext
{
    public NotificationContext()
    {
    }

    public NotificationContext(DbContextOptions<NotificationContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Notification>();
    }

    public DbSet<Notification> Notifications { get; set; }
}