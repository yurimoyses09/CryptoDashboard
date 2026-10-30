using CryptoDashboard.Infra;
using Microsoft.Extensions.Options;

namespace CryptoDashboard
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfra(this IServiceCollection services, IConfiguration configuration)
        {
            // Vincula a raiz do JSON diretamente à classe CoinSettings 
            // (pois Coins e CoinGeckoApi estão no primeiro nível do appsettings.json)
            services.Configure<CoinSettings>(configuration);

            // Registra o HttpClient nomeado utilizando a URL do appsettings
            services.AddHttpClient("CoinGeckoClient", (serviceProvider, client) =>
            {
                var settings = serviceProvider.GetRequiredService<IOptions<CoinSettings>>().Value;

                if (string.IsNullOrEmpty(settings.CoinGeckoApi?.BaseUrl))
                {
                    throw new InvalidOperationException("A BaseUrl do CoinGecko não foi configurada corretamente.");
                }

                client.BaseAddress = new Uri(settings.CoinGeckoApi.BaseUrl);
            });

            return services;
        }
    }
}