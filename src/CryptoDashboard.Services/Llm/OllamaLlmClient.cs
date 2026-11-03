using System.Net.Http.Json;
using System.Text.Json;
using CryptoDashboard.Infra;
using Microsoft.Extensions.Options;

namespace CryptoDashboard.Services.Llm
{
    // Chama o endpoint /api/chat do Ollama. O HttpClient já vem com a BaseAddress configurada.
    public class OllamaLlmClient : ILlmClient
    {
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

        private readonly HttpClient _http;
        private readonly SentimentSettings _settings;

        public OllamaLlmClient(HttpClient http, IOptions<SentimentSettings> settings)
        {
            _http = http;
            _settings = settings.Value;
        }

        public async Task<string> CompleteAsync(string system, string user, CancellationToken ct = default)
        {
            var body = new OllamaChatRequest(
                Model: _settings.Ollama.Model,
                Stream: false,
                // "json" força o modelo a responder JSON válido
                Format: "json",
                Options: new OllamaOptions(Temperature: 0),
                Messages:
                [
                    new OllamaMessage("system", system),
                    new OllamaMessage("user", user)
                ]);

            using var response = await _http.PostAsJsonAsync("api/chat", body, JsonOptions, ct);
            response.EnsureSuccessStatusCode();

            var payload = await response.Content.ReadFromJsonAsync<OllamaChatResponse>(JsonOptions, ct);
            return payload?.Message?.Content ?? string.Empty;
        }

        private record OllamaChatRequest(
            string Model,
            bool Stream,
            string Format,
            OllamaOptions Options,
            List<OllamaMessage> Messages);

        private record OllamaOptions(double Temperature);

        private record OllamaMessage(string Role, string Content);

        private record OllamaChatResponse(OllamaMessage? Message);
    }
}
