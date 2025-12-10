using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LiquorPOS.Services.Identity.Infrastructure.Seeding;

public sealed class IdentitySeederHostedService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<IdentitySeederHostedService> _logger;

    public IdentitySeederHostedService(
        IServiceProvider serviceProvider,
        ILogger<IdentitySeederHostedService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        var seeder = scope.ServiceProvider.GetRequiredService<IIdentitySeeder>();

        try
        {
            await seeder.SeedAsync(cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Failed to apply identity seed data");
            throw;
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
