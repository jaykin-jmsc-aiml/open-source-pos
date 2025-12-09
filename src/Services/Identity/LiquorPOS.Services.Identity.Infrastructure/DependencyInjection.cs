using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using LiquorPOS.Services.Identity.Infrastructure.Persistence;

namespace LiquorPOS.Services.Identity.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<IdentityDbContext>((sp, options) =>
        {
            options.UseSqlServer(connectionString);
        });

        return services;
    }
}
