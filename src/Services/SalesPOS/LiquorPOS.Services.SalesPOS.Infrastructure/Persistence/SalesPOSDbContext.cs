using Microsoft.EntityFrameworkCore;
using LiquorPOS.BuildingBlocks.Domain;

namespace LiquorPOS.Services.SalesPOS.Infrastructure.Persistence;

public class SalesPOSDbContext : DbContext
{
    public SalesPOSDbContext(DbContextOptions<SalesPOSDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SalesPOSDbContext).Assembly);
    }
}
