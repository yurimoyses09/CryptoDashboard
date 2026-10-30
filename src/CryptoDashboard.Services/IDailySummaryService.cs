using CryptoDashboard.Collectors.CoinGenko;

namespace CryptoDashboard.Services
{
    public interface IDailySummaryService
    {
        Task<IEnumerable<CoinMarket>> GetCoinMarketDataAsync();
    }
}
