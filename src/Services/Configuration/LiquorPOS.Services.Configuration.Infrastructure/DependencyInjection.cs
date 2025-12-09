using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using LiquorPOS.Services.Configuration.Infrastructure.Persistence;

namespace LiquorPOS.Services.Configuration.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<ConfigurationDbContext>((sp, options) =>
        {
            options.UseSqlServer(connectionString);
        });

        return services;
    }
}
