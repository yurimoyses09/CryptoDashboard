namespace CryptoDashboard.Collectors
{
    // Cotação de uma moeda, no formato que o coletor devolve.
    // Quem decide como gravar no banco é o worker, não o coletor.
    public record CoinQuote(
        string Id,
        string Symbol,
        string Name,
        decimal Price,
        decimal? MarketCap,
        decimal? Variation24h);
}
