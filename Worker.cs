using CbrCurrency.Services;

namespace CbrCurrency;

public class Worker(
    IServiceScopeFactory scopeFactory, 
    ILogger<Worker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

        logger.LogInformation("Worker запущен. Проверка начальной загрузки...");

        await PerformSyncAsync(async syncService => 
            await syncService.SyncLastMonthIfNeededAsync(stoppingToken), stoppingToken);

        // Таймер для ежедневного запуска. 
        using var timer = new PeriodicTimer(TimeSpan.FromHours(24));
        
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            logger.LogInformation("Запуск ежедневной синхронизации: {Time}", DateTimeOffset.Now);
            
            await PerformSyncAsync(async syncService => 
                await syncService.SyncForDateAsync(DateTime.Today, stoppingToken), stoppingToken);
        }
    }

    private async Task PerformSyncAsync(Func<CurrencySyncService, Task> action, CancellationToken cancellationToken)
    {
        try
        {
            using var scope = scopeFactory.CreateScope();
            var syncService = scope.ServiceProvider.GetRequiredService<CurrencySyncService>();
            
            await action(syncService);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Произошла ошибка при синхронизации курсов валют.");
        }
    }
}