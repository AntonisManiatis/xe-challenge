using Xe.Ratings.Service;

namespace Xe.Ratings.API;

sealed class CleanUpService : BackgroundService
{
    private static readonly TimeSpan Delay = TimeSpan.FromSeconds(60);

    private readonly IServiceProvider provider;

    public CleanUpService(IServiceProvider provider) => this.provider = provider;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(Delay, stoppingToken);

            using var scope = provider.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<IRatingService>();

            await service.CleanUpAsync();
        }
    }
}