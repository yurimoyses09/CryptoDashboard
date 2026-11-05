using CryptoDashboard.Data;
using Microsoft.EntityFrameworkCore;

namespace CryptoDashboard.Services
{
    // Lê apenas o que já está no banco: não chama API externa, então não usa chave.
    public class DailySummaryService : IDailySummaryService
    {
        private readonly AppDbContext _db;

        public DailySummaryService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<IReadOnlyList<CoinDailySummary>> GetAsync(
            TimeSpan window, int top = 10, CancellationToken ct = default)
        {
            // SQLite compara DateTimeOffset como texto, e offsets diferentes quebram a comparação.
            // Por isso o filtro de data é feito em memória, em UTC. O volume de notícias é pequeno.
            var since = DateTimeOffset.UtcNow - window;

            var classified = await _db.NewsItems
                .Where(n => n.Sentiment != null)
                .Select(n => new { n.Id, n.PublishedAt, n.Sentiment, n.Score })
                .ToListAsync(ct);

            var recent = classified
                .Where(n => n.PublishedAt.ToUniversalTime() >= since)
                .ToDictionary(n => n.Id);

            if (recent.Count == 0)
                return [];

            var links = await _db.NewsCoins
                .Select(nc => new { nc.NewsItemId, nc.CoinId })
                .ToListAsync(ct);

            return links
                .Where(l => recent.ContainsKey(l.NewsItemId))
                .GroupBy(l => l.CoinId)
                .Select(g =>
                {
                    var items = g.Select(l => recent[l.NewsItemId]).ToList();
                    var scores = items.Where(i => i.Score.HasValue).Select(i => i.Score!.Value).ToList();

                    return new CoinDailySummary(
                        CoinId: g.Key,
                        NewsCount: items.Count,
                        AverageScore: scores.Count > 0 ? Math.Round(scores.Average(), 2) : null,
                        Positive: items.Count(i => i.Sentiment == "positivo"),
                        Neutral: items.Count(i => i.Sentiment == "neutro"),
                        Negative: items.Count(i => i.Sentiment == "negativo"));
                })
                .OrderByDescending(s => s.NewsCount)
                .ThenBy(s => s.CoinId)
                .Take(top)
                .ToList();
        }
    }
}
