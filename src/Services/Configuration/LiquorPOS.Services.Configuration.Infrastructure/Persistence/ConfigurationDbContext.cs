using Microsoft.EntityFrameworkCore;
using LiquorPOS.BuildingBlocks.Domain;

namespace LiquorPOS.Services.Configuration.Infrastructure.Persistence;

public class ConfigurationDbContext : DbContext
{
    public ConfigurationDbContext(DbContextOptions<ConfigurationDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ConfigurationDbContext).Assembly);
    }
}
