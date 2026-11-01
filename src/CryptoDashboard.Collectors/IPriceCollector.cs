using CryptoDashboard.Data.Models;

namespace CryptoDashboard.Collectors
{
    public interface IPriceCollector
    {
        Task<IReadOnlyList<PricePoint>> CollectAsync(IEnumerable<string> coinIds, CancellationToken ct = default);
    }
}
