using CryptoDashboard.Data.Models;

namespace CryptoDashboard.Collectors
{
    public interface INewsCollector
    {
        string SourceName { get; }

        Task<IReadOnlyList<NewsItem>> CollectAsync(IEnumerable<string> coinIds, CancellationToken ct = default);
    }
}
