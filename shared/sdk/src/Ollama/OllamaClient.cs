using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;

namespace DeskMatch.SDK.Ollama;

public sealed class OllamaClient : IOllamaClient
{
    private readonly HttpClient _http;
    private readonly string _model;
    private readonly string? _chatModel;
    private readonly bool _configured;

    public bool IsAvailable => _configured;

    public OllamaClient(IConfiguration configuration)
    {
        var baseUrl = configuration["Ollama:BaseUrl"];
        _configured = !string.IsNullOrEmpty(baseUrl);
        _model = configuration["Ollama:Model"] ?? "nomic-embed-text-v2-moe";
        _chatModel = configuration["Ollama:ChatModel"];
        _http = new HttpClient
        {
            BaseAddress = new Uri(_configured ? baseUrl! : "http://localhost:11434"),
            Timeout = TimeSpan.FromSeconds(30)
        };
    }

    public async Task<float[]?> GetEmbeddingAsync(string text)
    {
        if (!_configured) return null;

        try
        {
            var response = await _http.PostAsJsonAsync("/api/embeddings", new
            {
                model = _model,
                prompt = text
            });

            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<OllamaEmbeddingResponse>();
            return result?.Embedding;
        }
        catch
        {
            return null;
        }
    }

    public async Task<string?> ChatCompletionAsync(string systemPrompt, string userMessage, string? model = null)
    {
        if (!_configured) return null;

        try
        {
            var chatModel = model ?? _chatModel ?? "qwen2.5:3b";
            var response = await _http.PostAsJsonAsync("/api/chat", new
            {
                model = chatModel,
                messages = new[]
                {
                    new { role = "system", content = systemPrompt },
                    new { role = "user", content = userMessage }
                },
                stream = false,
                options = new { temperature = 0.1 }
            });

            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<OllamaChatResponse>();
            return result?.Message?.Content?.Trim();
        }
        catch
        {
            return null;
        }
    }

    private sealed class OllamaEmbeddingResponse
    {
        public float[]? Embedding { get; set; }
    }

    private sealed class OllamaChatResponse
    {
        public OllamaChatMessage? Message { get; set; }
    }

    private sealed class OllamaChatMessage
    {
        public string? Content { get; set; }
    }
}
