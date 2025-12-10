using System.Threading;
using System.Threading.Tasks;

namespace LiquorPOS.Services.Identity.Infrastructure.Seeding;

public interface IIdentitySeeder
{
    Task SeedAsync(CancellationToken cancellationToken);
}
