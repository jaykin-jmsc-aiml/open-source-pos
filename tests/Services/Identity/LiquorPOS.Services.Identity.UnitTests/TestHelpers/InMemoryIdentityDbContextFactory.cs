using LiquorPOS.Services.Identity.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LiquorPOS.Services.Identity.UnitTests.TestHelpers;

public static class InMemoryIdentityDbContextFactory
{
    public static LiquorPOSIdentityDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<LiquorPOSIdentityDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()) // Unique DB per test
            .Options;

        var context = new LiquorPOSIdentityDbContext(options);
        context.Database.EnsureCreated(); // Apply schema
        return context;
    }
}