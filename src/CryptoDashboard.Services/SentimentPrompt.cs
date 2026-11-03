using System.Text.Json;

namespace CryptoDashboard.Services
{
    // Prompt e validação ficam fora dos provedores: Bedrock e Ollama recebem o mesmo texto
    // e devem ser avaliados com as mesmas regras.
    public static class SentimentPrompt
    {
        public const string System = """
            Você classifica notícias sobre criptomoedas.
            Responda somente com um objeto JSON, sem texto antes ou depois, no formato:
            {"sentimento": "positivo" | "neutro" | "negativo", "score": número entre -1 e 1, "moedas_citadas": [ids], "temas": [até 5 temas curtos]}
            Regras:
            - "moedas_citadas" deve conter apenas IDs da lista de moedas válidas informada pelo usuário.
            - Símbolos ambíguos (como SOL ou ADA) só contam se o texto falar da criptomoeda, e não de uma palavra comum.
            - Se a notícia não citar nenhuma moeda da lista, use [].
            """;

        public static string Build(string title, string text, IEnumerable<string> coinIds) =>
            $"Moedas válidas: {string.Join(", ", coinIds)}\n\nTítulo: {title}\nResumo: {text}";

        public static SentimentResult? Parse(string raw, IReadOnlyCollection<string> allowedCoinIds)
        {
            // Alguns modelos locais colocam texto em volta do JSON: pega do primeiro { ao último }.
            var start = raw.IndexOf('{');
            var end = raw.LastIndexOf('}');
            if (start < 0 || end <= start)
                return null;

            var allowed = new HashSet<string>(allowedCoinIds, StringComparer.OrdinalIgnoreCase);

            try
            {
                using var doc = JsonDocument.Parse(raw[start..(end + 1)]);
                var root = doc.RootElement;

                var sentiment = ReadString(root, "sentimento")?.Trim().ToLowerInvariant();
                if (sentiment is not ("positivo" or "neutro" or "negativo"))
                    return null;

                var score = 0m;
                if (root.TryGetProperty("score", out var scoreEl) && scoreEl.TryGetDecimal(out var parsed))
                    score = Math.Clamp(parsed, -1m, 1m);

                var coins = ReadStrings(root, "moedas_citadas")
                    .Select(c => c.Trim().ToLowerInvariant())
                    .Where(allowed.Contains)
                    .Distinct()
                    .ToList();

                var themes = ReadStrings(root, "temas")
                    .Select(t => t.Trim())
                    .Where(t => t.Length > 0)
                    .Take(5)
                    .ToList();

                return new SentimentResult(sentiment, score, coins, themes);
            }
            catch (JsonException)
            {
                return null;
            }
        }

        private static string? ReadString(JsonElement root, string name) =>
            root.TryGetProperty(name, out var el) && el.ValueKind == JsonValueKind.String
                ? el.GetString()
                : null;

        private static IEnumerable<string> ReadStrings(JsonElement root, string name)
        {
            if (!root.TryGetProperty(name, out var el) || el.ValueKind != JsonValueKind.Array)
                return [];

            return el.EnumerateArray()
                .Where(e => e.ValueKind == JsonValueKind.String)
                .Select(e => e.GetString()!)
                .ToList();
        }
    }
}
