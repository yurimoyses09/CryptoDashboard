using CryptoDashboard.Collectors.CoinGenko;
using CryptoDashboard.Infra;
using System.Net.Http.Json;
using System.Text.Json;

namespace CryptoDashboard.Services
{
    public class DailySummaryService : IDailySummaryService
    {
        private readonly CoinSettings _settings;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly JsonSerializerOptions _options;

        public DailySummaryService(CoinSettings settings, IHttpClientFactory httpClientFactory)
        {
            _settings = settings;
            _httpClientFactory = httpClientFactory;
            _options = new()
            {
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
            };
        }

        public async Task<IEnumerable<CoinMarket>> GetCoinMarketDataAsync()
        {
            // Implementation to fetch market data for the specified coin from CoinGecko API
            var ids = string.Join(",", _settings.Coins.Select(c => c.ToLowerInvariant()));

            var api_key = "CG-7XEyweqT7ytemHxBJbg92ctQ";

            var relativeUrl = $"/markets?vs_currency=usd&ids={ids}&x_cg_demo_api_key={api_key}";

            var httpClient = _httpClientFactory.CreateClient("CoinGeckoClient");
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("CryptoDashboardApp/1.0");

            // Make an HTTP request to the CoinGecko API and process the response
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("CryptoDashboardApp/1.0");

            return await httpClient.GetFromJsonAsync<IEnumerable<CoinMarket>>(relativeUrl, _options);
        }
    }
}
