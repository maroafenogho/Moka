using Moka.src.Shared;
using Microsoft.EntityFrameworkCore;

namespace Moka.src.Shared;

public class MigrationService : IHostedService
{
    private readonly IServiceScopeFactory _scopeFactory;
    /// <summary>
    ///
    /// </summary>
    /// <param name="scopeFactory"></param>
    public MigrationService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var serviceScope = _scopeFactory.CreateScope();
            serviceScope.ServiceProvider.GetRequiredService<AppDbContext>().Database.Migrate();
        }
        catch (Exception ex)
        {
            // Log.Error(ex.Message, ex);
        }
    }
    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}