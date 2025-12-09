using Microsoft.EntityFrameworkCore;
using LiquorPOS.BuildingBlocks.Domain;

namespace LiquorPOS.Services.InventoryPurchasing.Infrastructure.Persistence;

public class InventoryPurchasingDbContext : DbContext
{
    public InventoryPurchasingDbContext(DbContextOptions<InventoryPurchasingDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(InventoryPurchasingDbContext).Assembly);
    }
}
