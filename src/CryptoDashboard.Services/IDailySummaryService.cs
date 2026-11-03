namespace CryptoDashboard.Services
{
    public interface IDailySummaryService
    {
        // Resumo das moedas citadas nas notícias classificadas dentro da janela informada
        Task<IReadOnlyList<CoinDailySummary>> GetAsync(TimeSpan window, int top = 10, CancellationToken ct = default);
    }

    public record CoinDailySummary(
        string CoinId,
        int NewsCount,
        decimal? AverageScore,
        int Positive,
        int Neutral,
        int Negative);
}
