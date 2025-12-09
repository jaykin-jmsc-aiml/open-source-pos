using Microsoft.EntityFrameworkCore;
using LiquorPOS.BuildingBlocks.Domain;

namespace LiquorPOS.Services.ReportingAnalytics.Infrastructure.Persistence;

public class ReportingAnalyticsDbContext : DbContext
{
    public ReportingAnalyticsDbContext(DbContextOptions<ReportingAnalyticsDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ReportingAnalyticsDbContext).Assembly);
    }
}
