using CryptoDashboard.Data.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CryptoDashboard.Collectors
{
    public class CollectorWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<CollectorWorker> _logger;

        public CollectorWorker(IServiceScopeFactory scopeFactory, ILogger<CollectorWorker> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = _scopeFactory.CreateScope();
                var news = scope.ServiceProvider.GetRequiredService<NewsRepository>();
                // coleta de preços e de notícias usa 'scope' aqui

                await Task.Delay(TimeSpan.FromMinutes(15), stoppingToken);
            }
        }
    }
}
