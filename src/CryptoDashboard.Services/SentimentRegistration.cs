using Amazon;
using Amazon.BedrockRuntime;
using CryptoDashboard.Infra;
using CryptoDashboard.Services.Llm;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace CryptoDashboard.Services
{
    public static class SentimentRegistration
    {
        public static IServiceCollection AddSentiment(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<SentimentSettings>(configuration.GetSection("Sentiment"));

            var settings = configuration.GetSection("Sentiment").Get<SentimentSettings>() ?? new SentimentSettings();

            switch (settings.Provider.Trim().ToLowerInvariant())
            {
                case "bedrock":
                    if (string.IsNullOrWhiteSpace(settings.Bedrock.ModelId))
                        throw new InvalidOperationException(
                            "Sentiment:Bedrock:ModelId não foi definido. Informe o modelo habilitado no console do Bedrock.");

                    services.AddSingleton<IAmazonBedrockRuntime>(_ =>
                        new AmazonBedrockRuntimeClient(RegionEndpoint.GetBySystemName(settings.Bedrock.Region)));
                    services.AddScoped<ILlmClient, BedrockLlmClient>();
                    break;

                case "ollama":
                    services.AddHttpClient<OllamaLlmClient>(client =>
                    {
                        // BaseAddress precisa terminar com "/" para o caminho "api/chat" ser montado corretamente
                        client.BaseAddress = new Uri(settings.Ollama.BaseUrl.TrimEnd('/') + "/");
                        // Modelo local em CPU pode levar dezenas de segundos por notícia
                        client.Timeout = TimeSpan.FromMinutes(2);
                    });
                    services.AddScoped<ILlmClient>(sp => sp.GetRequiredService<OllamaLlmClient>());
                    break;

                default:
                    throw new InvalidOperationException(
                        $"Sentiment:Provider inválido: '{settings.Provider}'. Use 'Bedrock' ou 'Ollama'.");
            }

            services.AddScoped<ISentimentService, SentimentService>();
            services.AddHostedService<SentimentWorker>();

            return services;
        }
    }
}
