using System.ServiceModel.Syndication;
using System.Text.RegularExpressions;
using System.Xml;
using CryptoDashboard.Data.Models;

namespace CryptoDashboard.Collectors
{
    internal static partial class RssFeedReader
    {
        private const int MaxSummaryLength = 500;

        public static async Task<List<NewsItem>> ReadAsync(
            HttpClient http,
            string url,
            string source,
            CancellationToken ct)
        {
            await using var stream = await http.GetStreamAsync(url, ct);
            using var reader = XmlReader.Create(stream);
            var feed = SyndicationFeed.Load(reader);

            var items = new List<NewsItem>();

            foreach (var item in feed.Items)
            {
                var title = item.Title?.Text?.Trim();
                var link = CleanUrl(item.Links.FirstOrDefault()?.Uri);

                if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(link))
                    continue;

                var publishedAt = item.PublishDate != DateTimeOffset.MinValue
                    ? item.PublishDate
                    : item.LastUpdatedTime;

                var summary = CleanSummary(item.Summary?.Text);

                items.Add(new NewsItem(title, summary, link, source, publishedAt));
            }

            return items;
        }

        // Remove utm_* e outros parâmetros: o mesmo artigo vem com parâmetros
        // diferentes em cada tag, e isso impedia a deduplicação pela URL.
        private static string? CleanUrl(Uri? uri)
        {
            if (uri is null)
                return null;

            var builder = new UriBuilder(uri)
            {
                Query = string.Empty,
                Fragment = string.Empty
            };

            return builder.Uri.ToString();
        }

        private static string CleanSummary(string? html)
        {
            if (string.IsNullOrWhiteSpace(html))
                return string.Empty;

            var text = HtmlTag().Replace(html, string.Empty).Trim();

            return text.Length <= MaxSummaryLength
                ? text
                : text[..MaxSummaryLength];
        }

        [GeneratedRegex("<.*?>")]
        private static partial Regex HtmlTag();
    }
}