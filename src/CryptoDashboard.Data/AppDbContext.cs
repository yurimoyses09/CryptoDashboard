using CryptoDashboard.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace CryptoDashboard.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Coin> Coins => Set<Coin>();
        public DbSet<NewsItem> NewsItems => Set<NewsItem>();
        public DbSet<PricePoint> PricePoints => Set<PricePoint>();
        public DbSet<NewsCoin> NewsCoins => Set<NewsCoin>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<NewsItem>()
                .HasIndex(n => n.Url)
                .IsUnique();

            modelBuilder.Entity<NewsCoin>()
                .HasKey(nc => new { nc.NewsItemId, nc.CoinId });

            modelBuilder.Entity<NewsCoin>()
                .HasOne(nc => nc.NewsItem)
                .WithMany(n => n.NewsCoins)
                .HasForeignKey(nc => nc.NewsItemId);

            modelBuilder.Entity<NewsCoin>()
                .HasOne(nc => nc.Coin)
                .WithMany()
                .HasForeignKey(nc => nc.CoinId);

            modelBuilder.Entity<PricePoint>()
                .HasOne(p => p.Coin)
                .WithMany()
                .HasForeignKey(p => p.CoinId);

            modelBuilder.Entity<PricePoint>()
                .HasIndex(p => new { p.CoinId, p.CollectedAt });
        }
    }
}