using CryptoDashboard.Collectors.CoinGenko;
using CryptoDashboard.Data.Models;
using System.Net.Http.Json;
using System.Text.Json;

namespace CryptoDashboard.Collectors
{
    public class CoinGeckoPriceCollector : IPriceCollector
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
        };

        private readonly HttpClient _http;

        public CoinGeckoPriceCollector(HttpClient http)
        {
            _http = http;
        }

        public async Task<IReadOnlyList<PricePoint>> CollectAsync(IEnumerable<string> coinIds, CancellationToken ct = default)
        {
            var ids = coinIds.ToList();
            if (ids.Count == 0)
                return [];

            // Uma única chamada para todas as moedas
            var url = $"coins/markets?vs_currency=usd&ids={string.Join(",", ids)}";

            var markets = await _http.GetFromJsonAsync<List<CoinMarket>>(url, JsonOptions, ct)
                ?? [];

            // Mesmo instante para todas as moedas desta coleta
            var collectedAt = DateTimeOffset.UtcNow;

            return markets
                .Select(m => new PricePoint(
                    m.Id,
                    m.CurrentPrice,
                    m.MarketCap,
                    m.PriceChangePercentage24h,
                    collectedAt))
                .ToList();
        }
    }
}
