namespace CryptoDashboard.Infra
{
    public class CoinSettings
    {
        public List<string> Coins { get; set; } = new();
        public CoinGeckoApiSettings CoinGeckoApi { get; set; } = new();
    }

    public class CoinGeckoApiSettings
    {
        public string BaseUrl { get; set; } = string.Empty;
    }
}
