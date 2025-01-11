using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;

public class ReservationCleanupService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;

    public ReservationCleanupService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var autoReturnService = scope.ServiceProvider.GetRequiredService<AutoReturnService>();
                autoReturnService.CleanUpExpiredReservations();
            }

            // Run the cleanup every second for testing purposes
            await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
        }
    }
}
