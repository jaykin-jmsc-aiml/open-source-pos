using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using LiquorPOS.Services.Catalog.Infrastructure.Persistence;

namespace LiquorPOS.Services.Catalog.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<CatalogDbContext>((sp, options) =>
        {
            options.UseSqlServer(connectionString);
        });

        return services;
    }
}
