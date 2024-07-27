using Microsoft.EntityFrameworkCore;

public class IOMSContext : DbContext
{
    public IOMSContext(DbContextOptions<IOMSContext> options)
        : base(options)
    {
    }

    public DbSet<InventoryItem> InventoryItems { get; set; }
}
