using CryptoDashboard.Data;
using CryptoDashboard.Data.Models;
using CryptoDashboard.Infra;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CryptoDashboard.Collectors
{
    public class CollectorWorker : BackgroundService
    {
        private static readonly TimeSpan PriceInterval = TimeSpan.FromMinutes(15);
        private static readonly TimeSpan NewsInterval = TimeSpan.FromMinutes(30);
        private static readonly TimeSpan CheckInterval = TimeSpan.FromSeconds(30);

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IOptions<CoinSettings> _settings;
        private readonly ILogger<CollectorWorker> _logger;

        public CollectorWorker(
            IServiceScopeFactory scopeFactory,
            IOptions<CoinSettings> settings,
            ILogger<CollectorWorker> logger)
        {
            _scopeFactory = scopeFactory;
            _settings = settings;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var coinIds = _settings.Value.Coins;

            // Ambos rodam já na inicialização (MinValue ≤ agora)
            var nextPrice = DateTimeOffset.MinValue;
            var nextNews = DateTimeOffset.MinValue;

            _logger.LogInformation("CollectorWorker iniciado para {Count} moedas", coinIds.Count);

            while (!stoppingToken.IsCancellationRequested)
            {
                var now = DateTimeOffset.UtcNow;

                if (now >= nextPrice)
                {
                    await RunStepAsync("preços", ct => CollectPricesAsync(coinIds, ct), stoppingToken);
                    nextPrice = DateTimeOffset.UtcNow + PriceInterval;
                }

                if (now >= nextNews)
                {
                    await RunStepAsync("notícias", ct => CollectNewsAsync(coinIds, ct), stoppingToken);
                    nextNews = DateTimeOffset.UtcNow + NewsInterval;
                }

                await Task.Delay(CheckInterval, stoppingToken);
            }
        }

        // Uma etapa que falha não derruba as outras nem o worker
        private async Task RunStepAsync(string etapa, Func<CancellationToken, Task> passo, CancellationToken ct)
        {
            try
            {
                await passo(ct);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falha na coleta de {Etapa}. Nova tentativa no próximo ciclo.", etapa);
            }
        }

        private async Task CollectPricesAsync(IReadOnlyList<string> coinIds, CancellationToken ct)
        {
            using var scope = _scopeFactory.CreateScope();
            var collector = scope.ServiceProvider.GetRequiredService<IPriceCollector>();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var quotes = await collector.CollectAsync(coinIds, ct);

            if (quotes.Count == 0)
            {
                _logger.LogWarning("A API de preços não devolveu nenhuma moeda");
                return;
            }

            await EnsureCoinsExistAsync(db, quotes, ct);

            // Mesmo instante para todas as moedas desta coleta
            var collectedAt = DateTimeOffset.UtcNow;

            foreach (var q in quotes)
            {
                db.PricePoints.Add(new PricePoint(
                    q.Id,
                    q.Price,
                    q.MarketCap,
                    q.Variation24h,
                    collectedAt));
            }

            await db.SaveChangesAsync(ct);
            _logger.LogInformation("{Count} preços salvos", quotes.Count);
        }

        // PricePoint depende de Coin (chave estrangeira), então a moeda precisa existir antes
        private static async Task EnsureCoinsExistAsync(AppDbContext db, IReadOnlyList<CoinQuote> quotes, CancellationToken ct)
        {
            var ids = quotes.Select(q => q.Id).ToList();

            var existentes = await db.Coins
                .Where(c => ids.Contains(c.Id))
                .Select(c => c.Id)
                .ToListAsync(ct);

            var existingSet = new HashSet<string>(existentes);

            foreach (var q in quotes.Where(q => !existingSet.Contains(q.Id)))
            {
                db.Coins.Add(new Coin(q.Id, q.Symbol.ToUpperInvariant(), q.Name));
            }
        }

        private async Task CollectNewsAsync(IReadOnlyList<string> coinIds, CancellationToken ct)
        {
            using var scope = _scopeFactory.CreateScope();
            var collectors = scope.ServiceProvider.GetServices<INewsCollector>();
            var repo = scope.ServiceProvider.GetRequiredService<CryptoDashboard.Data.Repositories.NewsRepository>();

            foreach (var collector in collectors)
            {
                try
                {
                    var items = await collector.CollectAsync(coinIds, ct);
                    var novos = await repo.SaveNewNewsAsync(items, ct);

                    _logger.LogInformation(
                        "{Source}: {Lidos} notícias lidas, {Novos} novas salvas",
                        collector.SourceName, items.Count, novos);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    _logger.LogWarning(ex, "Falha ao coletar notícias de {Source}", collector.SourceName);
                }
            }
        }
    }
}
