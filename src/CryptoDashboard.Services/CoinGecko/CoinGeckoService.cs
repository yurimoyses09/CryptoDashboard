using System.Net.Http.Json;
using System.Text.Json;

namespace CryptoDashboard.Services.CoinGecko
{
    public static class CoinGeckoService
    {
        public static async Task GetCoinMarketData()
        {
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
            };

            // Implementation to fetch market data for the specified coin from CoinGecko API
            var ids = string.Join(",", new List<string> { "bitcoin", "ethereum", "ripple" });
            var api_key = "CG-7XEyweqT7ytemHxBJbg92ctQ";
            var url = $"https://api.coingecko.com/api/v3/coins/markets?vs_currency=usd&ids={ids}&x_cg_demo_api_key={api_key}";


            // Make an HTTP request to the CoinGecko API and process the response
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("CryptoDashboardApp/1.0");

            var coins = await httpClient.GetFromJsonAsync<List<CoinMarket>>(url, options);

            foreach (var c in coins!)
                Console.WriteLine($"{c.Symbol} {c.Name}: {c.CurrentPrice} USD ({c.PriceChangePercentage24h}%)");
        }
    }
}
