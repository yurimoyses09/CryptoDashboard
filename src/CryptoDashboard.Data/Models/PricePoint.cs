using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CryptoDashboard.Data.Models
{
    [Table("tb_price_point")]
    public class PricePoint
    {
        [Key]
        [Column("id")]
        public long Id { get; private set; }

        [Required]
        [Column("coin_id")]
        public string CoinId { get; private set; } = null!;

        public Coin Coin { get; private set; } = null!;

        [Column("price")]
        public decimal Price { get; private set; }

        [Column("market_cap")]
        public decimal? MarketCap { get; private set; }

        [Column("variation_24h")]
        public decimal? Variation24h { get; private set; }

        [Column("collected_at")]
        public DateTimeOffset CollectedAt { get; private set; }

        private PricePoint() { }

        public PricePoint(string coinId, decimal price, decimal? marketCap, decimal? variation24h, DateTimeOffset collectedAt)
        {
            CoinId = coinId;
            Price = price;
            MarketCap = marketCap;
            Variation24h = variation24h;
            CollectedAt = collectedAt;
        }
    }
}