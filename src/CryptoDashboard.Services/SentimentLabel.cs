namespace CryptoDashboard.Services
{
    // Converte a média do score em um rótulo para exibição. Acima de 0,2 é positivo; abaixo de -0,2, negativo.
    public static class SentimentLabel
    {
        public static string? FromScore(decimal? score) => score switch
        {
            null => null,
            >= 0.2m => "positivo",
            <= -0.2m => "negativo",
            _ => "neutro"
        };
    }
}
