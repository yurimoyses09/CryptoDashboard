namespace CryptoDashboard.Infra
{
    public class SentimentSettings
    {
        // "Bedrock" ou "Ollama". Escolhe qual modelo classifica as notícias.
        public string Provider { get; set; } = "Ollama";

        // Quantas notícias pendentes são classificadas por ciclo.
        public int BatchSize { get; set; } = 10;

        public BedrockSentimentSettings Bedrock { get; set; } = new();
        public OllamaSentimentSettings Ollama { get; set; } = new();
    }

    public class BedrockSentimentSettings
    {
        // Para alguns modelos, o Bedrock exige o ID de inference profile (ex.: "us.<modelo>").
        public string ModelId { get; set; } = string.Empty;
        public string Region { get; set; } = "us-east-1";
    }

    public class OllamaSentimentSettings
    {
        public string BaseUrl { get; set; } = "http://localhost:11434";
        public string Model { get; set; } = "llama3.2";
    }
}
