using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CryptoDashboard.Data.Models
{
    [Table("tb_news")]
    public class NewsItem
    {
        [Key]
        [Column("id")]
        public long Id { get; private set; }

        [Required]
        [Column("title")]
        public string Title { get; private set; } = null!;

        [Required]
        [Column("text")]
        public string Text { get; private set; } = null!;

        [Required]
        [Column("url")]
        public string Url { get; private set; } = null!;

        [Required]
        [Column("source")]
        public string Source { get; private set; } = null!;

        [Column("published_at")]
        public DateTimeOffset PublishedAt { get; private set; }

        // "positivo", "neutro" ou "negativo". Fica nulo até a IA classificar.
        [Column("sentiment")]
        public string? Sentiment { get; private set; }

        [Column("score")]
        public decimal? Score { get; private set; }

        public ICollection<NewsCoin> NewsCoins { get; } = new List<NewsCoin>();

        private NewsItem() { }

        public NewsItem(string title, string text, string url, string source, DateTimeOffset publishedAt)
        {
            Title = title;
            Text = text;
            Url = url;
            Source = source;
            PublishedAt = publishedAt;
        }

        public void SetSentiment(string sentiment, decimal score)
        {
            Sentiment = sentiment;
            Score = score;
        }
    }
}