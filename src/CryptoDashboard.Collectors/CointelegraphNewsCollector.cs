using CryptoDashboard.Data.Models;
using Microsoft.Extensions.Logging;

namespace CryptoDashboard.Collectors
{
    public class CointelegraphNewsCollector : INewsCollector
    {
        private readonly HttpClient _http;
        private readonly ILogger<CointelegraphNewsCollector> _logger;

        public string SourceName => "Cointelegraph";

        public CointelegraphNewsCollector(HttpClient http, ILogger<CointelegraphNewsCollector> logger)
        {
            _http = http;
            _logger = logger;
        }

        public async Task<IReadOnlyList<NewsItem>> CollectAsync(IEnumerable<string> coinIds, CancellationToken ct = default)
        {
            // Feed geral + um feed por tag de moeda
            var paths = new List<string> { "rss" };
            paths.AddRange(coinIds.Select(id => $"rss/tag/{id}"));

            var seenUrls = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var result = new List<NewsItem>();

            foreach (var path in paths)
            {
                try
                {
                    var items = await RssFeedReader.ReadAsync(_http, path, SourceName, ct);

                    foreach (var item in items)
                    {
                        if (seenUrls.Add(item.Url))
                            result.Add(item);
                    }
                }
                catch (HttpRequestException ex)
                {
                    // Uma tag inexistente ou fora do ar não derruba as demais
                    _logger.LogWarning(ex, "Falha ao ler o feed {Path} do Cointelegraph", path);
                }
            }

            return result;
        }
    }
}
