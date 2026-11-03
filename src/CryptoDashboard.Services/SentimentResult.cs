namespace CryptoDashboard.Services
{
    // Resultado já validado: sentimento em {positivo, neutro, negativo}, score em [-1, 1],
    // moedas apenas da lista permitida e no máximo 5 temas.
    public record SentimentResult(
        string Sentiment,
        decimal Score,
        IReadOnlyList<string> Coins,
        IReadOnlyList<string> Themes);
}
