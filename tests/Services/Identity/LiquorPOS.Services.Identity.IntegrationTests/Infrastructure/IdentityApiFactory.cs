using LiquorPOS.Services.Identity.Api;
using LiquorPOS.Services.Identity.Domain.Entities;
using LiquorPOS.Services.Identity.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace LiquorPOS.Services.Identity.IntegrationTests.Infrastructure;

// This is the factory class for integration tests
// It's essentially IdentityWebApplicationFactory but with a shorter name
public class IdentityApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(d =>
                d.ServiceType == typeof(DbContextOptions<LiquorPOSIdentityDbContext>));

            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            services.AddDbContext<LiquorPOSIdentityDbContext>(options =>
            {
                options.UseInMemoryDatabase("IntegrationTestDatabase");
            });

            var serviceProvider = services.BuildServiceProvider();

            using (var scope = serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<LiquorPOSIdentityDbContext>();
                dbContext.Database.EnsureCreated();

                var roleManager = scope.ServiceProvider.GetRequiredService<
                    Microsoft.AspNetCore.Identity.RoleManager<ApplicationRole>>();

                EnsureRolesExist(roleManager).Wait();
            }
        });

        builder.UseEnvironment("Development");
    }

    private static async Task EnsureRolesExist(
        Microsoft.AspNetCore.Identity.RoleManager<ApplicationRole> roleManager)
    {
        var roles = new[] { "Manager", "Cashier", "Admin" };

        foreach (var roleName in roles)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                var role = new ApplicationRole
                {
                    Name = roleName,
                    NormalizedName = roleName.ToUpperInvariant(),
                    CreatedAt = DateTime.UtcNow
                };

                await roleManager.CreateAsync(role);
            }
        }
    }
}
