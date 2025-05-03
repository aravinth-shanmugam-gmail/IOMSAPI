using IOMSAPI.Models;
using Microsoft.EntityFrameworkCore;

public class IOMSContext : DbContext
{
    public IOMSContext(DbContextOptions<IOMSContext> options)
        : base(options)
    {
    }

    public DbSet<InventoryItem> InventoryItems { get; set; }
    public DbSet<AdditionalInvImage> AdditionalInvImages { get; set; }

    public DbSet<Customer> Customers { get; set; }
    public DbSet<Cart> Carts { get; set; }
    public DbSet<Payment> Payments { get; set; }
}
