using Amazon.BedrockRuntime;
using Amazon.BedrockRuntime.Model;
using CryptoDashboard.Infra;
using Microsoft.Extensions.Options;

namespace CryptoDashboard.Services.Llm
{
    // Usa a API Converse, que tem o mesmo formato para qualquer modelo do Bedrock.
    // As credenciais vêm da cadeia padrão da AWS (IAM Role no EC2, perfil local no Docker).
    public class BedrockLlmClient : ILlmClient
    {
        private readonly IAmazonBedrockRuntime _client;
        private readonly SentimentSettings _settings;

        public BedrockLlmClient(IAmazonBedrockRuntime client, IOptions<SentimentSettings> settings)
        {
            _client = client;
            _settings = settings.Value;
        }

        public async Task<string> CompleteAsync(string system, string user, CancellationToken ct = default)
        {
            var request = new ConverseRequest
            {
                ModelId = _settings.Bedrock.ModelId,
                System = [new SystemContentBlock { Text = system }],
                Messages =
                [
                    new Message
                    {
                        Role = ConversationRole.User,
                        Content = [new ContentBlock { Text = user }]
                    }
                ],
                InferenceConfig = new InferenceConfiguration
                {
                    MaxTokens = 300,
                    Temperature = 0
                }
            };

            var response = await _client.ConverseAsync(request, ct);

            return response.Output.Message.Content
                .Where(c => c.Text is not null)
                .Select(c => c.Text)
                .FirstOrDefault() ?? string.Empty;
        }
    }
}
