namespace CryptoDashboard.Collectors
{
    public interface IPriceCollector
    {
        Task<IReadOnlyList<CoinQuote>> CollectAsync(IEnumerable<string> coinIds, CancellationToken ct = default);
    }
}
