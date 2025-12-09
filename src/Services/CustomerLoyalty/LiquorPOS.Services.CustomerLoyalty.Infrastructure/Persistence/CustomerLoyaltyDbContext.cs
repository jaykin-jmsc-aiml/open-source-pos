using Microsoft.EntityFrameworkCore;
using LiquorPOS.BuildingBlocks.Domain;

namespace LiquorPOS.Services.CustomerLoyalty.Infrastructure.Persistence;

public class CustomerLoyaltyDbContext : DbContext
{
    public CustomerLoyaltyDbContext(DbContextOptions<CustomerLoyaltyDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CustomerLoyaltyDbContext).Assembly);
    }
}
