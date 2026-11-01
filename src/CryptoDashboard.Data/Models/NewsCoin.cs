using System.ComponentModel.DataAnnotations.Schema;

namespace CryptoDashboard.Data.Models
{
    [Table("tb_news_coin")]
    public class NewsCoin
    {
        [Column("news_item_id")]
        public long NewsItemId { get; private set; }

        public NewsItem NewsItem { get; private set; } = null!;

        [Column("coin_id")]
        public string CoinId { get; private set; } = null!;

        public Coin Coin { get; private set; } = null!;

        private NewsCoin() { }

        public NewsCoin(long newsItemId, string coinId)
        {
            NewsItemId = newsItemId;
            CoinId = coinId;
        }
    }
}