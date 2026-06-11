namespace DeskMatch.SDK.Ollama;

public interface IOllamaClient
{
    Task<float[]?> GetEmbeddingAsync(string text);
    Task<string?> ChatCompletionAsync(string systemPrompt, string userMessage, string? model = null);
    bool IsAvailable { get; }
}
