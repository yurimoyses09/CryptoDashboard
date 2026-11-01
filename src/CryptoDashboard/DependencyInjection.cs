using CryptoDashboard.Collectors;
using CryptoDashboard.Data;
using CryptoDashboard.Data.Repositories;
using CryptoDashboard.Infra;
using Microsoft.EntityFrameworkCore;
using Polly;

namespace CryptoDashboard
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfra(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<CoinSettings>(configuration);

            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlite(configuration.GetConnectionString("Default")));

            services.AddScoped<NewsRepository>();

            services.AddHttpClient<IPriceCollector, CoinGeckoPriceCollector>(client =>
            {
                client.BaseAddress = new Uri("https://api.coingecko.com/api/v3/");
                client.DefaultRequestHeaders.UserAgent.ParseAdd("CryptoDashboard/1.0");
            })
            .AddStandardResilienceHandler(options =>
            {
                options.Retry.MaxRetryAttempts = 3;
                options.Retry.Delay = TimeSpan.FromSeconds(2);
                options.Retry.BackoffType = DelayBackoffType.Exponential;
                options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(30);
            });

            services.AddHttpClient<INewsCollector, CointelegraphNewsCollector>(client =>
            {
                client.BaseAddress = new Uri("https://cointelegraph.com/");
                client.DefaultRequestHeaders.UserAgent.ParseAdd("CryptoDashboard/1.0");
            })
            .AddStandardResilienceHandler(options =>
            {
                options.Retry.MaxRetryAttempts = 2;
                options.Retry.Delay = TimeSpan.FromSeconds(2);
                options.Retry.BackoffType = DelayBackoffType.Exponential;
                options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(20);
            });

            services.AddHostedService<CollectorWorker>();

            return services;
        }

        public static void InitializeDatabase(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            db.Database.Migrate();
            db.Database.ExecuteSqlRaw("PRAGMA journal_mode=WAL;");
        }
    }
}