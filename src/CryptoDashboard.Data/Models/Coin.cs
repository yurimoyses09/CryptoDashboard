using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CryptoDashboard.Data.Models
{
    [Table("tb_coin")]
    public class Coin
    {
        [Key]
        [Column("id")]
        public string Id { get; private set; } = null!;

        [Required]
        [Column("symbol")]
        public string Symbol { get; private set; } = null!;

        [Required]
        [Column("name")]
        public string Name { get; private set; } = null!;

        private Coin() { }

        public Coin(string id, string symbol, string name)
        {
            Id = id;
            Symbol = symbol;
            Name = name;
        }
    }
}