namespace CryptoDashboard.Infra
{
    public class CoinSettings
    {
        public List<string> Coins { get; set; } = new();
        public CoinGeckoApiSettings CoinGeckoApi { get; set; } = new();

        // Mapeia o ID da moeda (CoinGecko) para a tag do Cointelegraph, quando forem diferentes.
        // Moedas sem entrada aqui usam o próprio ID como tag.
        public Dictionary<string, string> NewsTags { get; set; } = new();
    }

    public class CoinGeckoApiSettings
    {
        public string BaseUrl { get; set; } = string.Empty;
    }
}
