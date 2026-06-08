namespace DeskMatch.SDK.Ollama;

public interface IOllamaClient
{
    Task<float[]?> GetEmbeddingAsync(string text);
    bool IsAvailable { get; }
}
