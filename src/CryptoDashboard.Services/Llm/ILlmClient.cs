namespace CryptoDashboard.Services.Llm
{
    // Contrato mínimo que cada provedor implementa: recebe instruções e pergunta, devolve texto.
    public interface ILlmClient
    {
        Task<string> CompleteAsync(string system, string user, CancellationToken ct = default);
    }
}
