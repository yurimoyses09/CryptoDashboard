namespace CryptoDashboard.Services
{
    public interface ISentimentService
    {
        // Devolve null quando o modelo responde algo que não passa na validação.
        Task<SentimentResult?> AnalyzeAsync(
            string title,
            string text,
            IReadOnlyList<string> allowedCoinIds,
            CancellationToken ct = default);
    }
}
