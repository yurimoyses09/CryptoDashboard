using CryptoDashboard.Data;
using CryptoDashboard.Data.Models;
using CryptoDashboard.Infra;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CryptoDashboard.Services
{
    // Classifica as notícias ainda sem sentimento, em lotes pequenos, para controlar custo.
    public class SentimentWorker : BackgroundService
    {
        private static readonly TimeSpan StartupDelay = TimeSpan.FromMinutes(1);
        private static readonly TimeSpan Interval = TimeSpan.FromMinutes(30);

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IOptions<SentimentSettings> _sentiment;
        private readonly IOptions<CoinSettings> _coins;
        private readonly ILogger<SentimentWorker> _logger;

        public SentimentWorker(
            IServiceScopeFactory scopeFactory,
            IOptions<SentimentSettings> sentiment,
            IOptions<CoinSettings> coins,
            ILogger<SentimentWorker> logger)
        {
            _scopeFactory = scopeFactory;
            _sentiment = sentiment;
            _coins = coins;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Espera a primeira coleta de notícias terminar, para haver o que classificar
            await Task.Delay(StartupDelay, stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ClassifyPendingAsync(stoppingToken);
                    await LogDailySummaryAsync(stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Falha no ciclo de classificação. Nova tentativa em {Intervalo}.", Interval);
                }

                await Task.Delay(Interval, stoppingToken);
            }
        }

        // Registra no log as moedas mais citadas nas últimas 24h. Serve de base para o bloco do dashboard (Dia 4).
        private async Task LogDailySummaryAsync(CancellationToken ct)
        {
            using var scope = _scopeFactory.CreateScope();
            var summary = scope.ServiceProvider.GetRequiredService<IDailySummaryService>();

            var top = await summary.GetAsync(TimeSpan.FromHours(24), 10, ct);

            if (top.Count == 0)
            {
                _logger.LogInformation("Resumo 24h: nenhuma notícia classificada no período");
                return;
            }

            foreach (var c in top)
            {
                _logger.LogInformation(
                    "Resumo 24h: {Moeda} com {Noticias} notícias (pos {Pos} / neu {Neu} / neg {Neg}), score médio {Score}",
                    c.CoinId, c.NewsCount, c.Positive, c.Neutral, c.Negative, c.AverageScore);
            }
        }

        private async Task ClassifyPendingAsync(CancellationToken ct)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var sentiment = scope.ServiceProvider.GetRequiredService<ISentimentService>();

            var coinIds = _coins.Value.Coins;

            var pending = await db.NewsItems
                .Where(n => n.Sentiment == null)
                // SQLite não ordena DateTimeOffset no banco. Id crescente segue a ordem de inserção.
                .OrderByDescending(n => n.Id)
                .Take(_sentiment.Value.BatchSize)
                .ToListAsync(ct);

            if (pending.Count == 0)
                return;

            var classified = new List<(NewsItem Item, SentimentResult Result)>();

            foreach (var item in pending)
            {
                try
                {
                    var result = await sentiment.AnalyzeAsync(item.Title, item.Text, coinIds, ct);

                    if (result is null)
                    {
                        // Fica sem sentimento e entra de novo no próximo ciclo
                        _logger.LogWarning("Notícia {Id} não classificada: resposta inválida", item.Id);
                        continue;
                    }

                    item.SetSentiment(result.Sentiment, result.Score);
                    classified.Add((item, result));
                }
                catch (OperationCanceledException) when (ct.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Falha ao classificar a notícia {Id}", item.Id);
                }
            }

            if (classified.Count == 0)
                return;

            // Salva o sentimento primeiro: o Id da notícia só existe depois do insert
            await db.SaveChangesAsync(ct);

            // Relaciona as moedas citadas, apenas as que existem em tb_coin (chave estrangeira)
            var knownCoins = await db.Coins
                .Where(c => coinIds.Contains(c.Id))
                .Select(c => c.Id)
                .ToListAsync(ct);

            foreach (var (item, result) in classified)
            {
                foreach (var coinId in result.Coins.Where(knownCoins.Contains))
                {
                    db.NewsCoins.Add(new NewsCoin(item.Id, coinId));
                }
            }

            await db.SaveChangesAsync(ct);

            _logger.LogInformation(
                "{Classificadas} de {Total} notícias classificadas pelo modelo",
                classified.Count, pending.Count);
        }
    }
}
