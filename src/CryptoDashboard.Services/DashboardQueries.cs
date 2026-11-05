using CryptoDashboard.Data;
using Microsoft.EntityFrameworkCore;

namespace CryptoDashboard.Services
{
    public record CoinRow(
        string CoinId,
        string Symbol,
        string Name,
        decimal? Price,
        decimal? Variation24h,
        decimal? MarketCap,
        int NewsCount24h,
        decimal? AverageScore24h);

    public record PricePointDto(string CoinId, DateTimeOffset At, decimal Price);

    public record NewsRow(
        long Id,
        string Title,
        string Url,
        string Source,
        DateTimeOffset PublishedAt,
        string? Sentiment,
        decimal? Score,
        IReadOnlyList<string> Coins);

    // Consultas de leitura para o dashboard. Não gravam nada.
    public class DashboardQueries
    {
        private readonly AppDbContext _db;
        private readonly IDailySummaryService _summary;

        public DashboardQueries(AppDbContext db, IDailySummaryService summary)
        {
            _db = db;
            _summary = summary;
        }

        public async Task<IReadOnlyList<CoinRow>> GetCoinTableAsync(IEnumerable<string> coinIds, CancellationToken ct = default)
        {
            var ids = coinIds.ToList();

            var coins = await _db.Coins
                .Where(c => ids.Contains(c.Id))
                .ToDictionaryAsync(c => c.Id, ct);

            // Último ponto de cada moeda = maior Id. O SQLite não ordena DateTimeOffset, e o Id segue a ordem de inserção.
            var latestIds = await _db.PricePoints
                .Where(p => ids.Contains(p.CoinId))
                .GroupBy(p => p.CoinId)
                .Select(g => g.Max(p => p.Id))
                .ToListAsync(ct);

            var latest = await _db.PricePoints
                .Where(p => latestIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.CoinId, ct);

            var sentiment = (await _summary.GetAsync(TimeSpan.FromHours(24), 100, ct))
                .ToDictionary(s => s.CoinId);

            return ids.Select(id =>
            {
                coins.TryGetValue(id, out var coin);
                latest.TryGetValue(id, out var point);
                sentiment.TryGetValue(id, out var news);

                return new CoinRow(
                    id,
                    coin?.Symbol ?? id,
                    coin?.Name ?? id,
                    point?.Price,
                    point?.Variation24h,
                    point?.MarketCap,
                    news?.NewsCount ?? 0,
                    news?.AverageScore);
            }).ToList();
        }

        public async Task<IReadOnlyList<PricePointDto>> GetHistoryAsync(IEnumerable<string> coinIds, TimeSpan window, CancellationToken ct = default)
        {
            var ids = coinIds.ToList();
            var since = DateTimeOffset.UtcNow - window;

            // Filtro de data em memória: o SQLite compara DateTimeOffset como texto (ver DailySummaryService).
            // Carrega só as moedas selecionadas. Se o histórico crescer muito, filtrar por Id.
            var points = await _db.PricePoints
                .Where(p => ids.Contains(p.CoinId))
                .Select(p => new PricePointDto(p.CoinId, p.CollectedAt, p.Price))
                .ToListAsync(ct);

            return points
                .Where(p => p.At.ToUniversalTime() >= since)
                .OrderBy(p => p.At)
                .ToList();
        }

        public async Task<IReadOnlyList<NewsRow>> GetNewsAsync(string? coinId, int take = 30, CancellationToken ct = default)
        {
            var query = _db.NewsItems.AsQueryable();

            if (!string.IsNullOrWhiteSpace(coinId))
                query = query.Where(n => n.NewsCoins.Any(nc => nc.CoinId == coinId));

            var items = await query
                .OrderByDescending(n => n.Id)
                .Take(take)
                .Include(n => n.NewsCoins)
                .ToListAsync(ct);

            return items.Select(n => new NewsRow(
                n.Id,
                n.Title,
                n.Url,
                n.Source,
                n.PublishedAt,
                n.Sentiment,
                n.Score,
                n.NewsCoins.Select(nc => nc.CoinId).ToList())).ToList();
        }
    }
}
