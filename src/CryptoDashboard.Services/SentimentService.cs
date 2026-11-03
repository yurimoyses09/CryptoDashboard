using CryptoDashboard.Services.Llm;
using Microsoft.Extensions.Logging;

namespace CryptoDashboard.Services
{
    // Une o prompt, o provedor escolhido e a validação. Não sabe qual modelo está por trás.
    public class SentimentService : ISentimentService
    {
        private readonly ILlmClient _llm;
        private readonly ILogger<SentimentService> _logger;

        public SentimentService(ILlmClient llm, ILogger<SentimentService> logger)
        {
            _llm = llm;
            _logger = logger;
        }

        public async Task<SentimentResult?> AnalyzeAsync(
            string title,
            string text,
            IReadOnlyList<string> allowedCoinIds,
            CancellationToken ct = default)
        {
            var raw = await _llm.CompleteAsync(
                SentimentPrompt.System,
                SentimentPrompt.Build(title, text, allowedCoinIds),
                ct);

            var result = SentimentPrompt.Parse(raw, allowedCoinIds);

            if (result is null)
                _logger.LogWarning("Resposta do modelo fora do formato esperado: {Trecho}", Truncate(raw, 200));

            return result;
        }

        private static string Truncate(string value, int max) =>
            value.Length <= max ? value : value[..max];
    }
}
