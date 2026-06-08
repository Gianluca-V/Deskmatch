using DeskMatch.SDK.Ollama;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace DeskMatch.UnitTests.SDK;

public class OllamaClientTests
{
    [Fact]
    public async Task GetEmbeddingAsync_WhenNotConfigured_ShouldReturnNull()
    {
        var configuration = new Mock<IConfiguration>();
        configuration.Setup(c => c["Ollama:BaseUrl"]).Returns(string.Empty);
        configuration.Setup(c => c["Ollama:Model"]).Returns("test-model");

        var client = new OllamaClient(configuration.Object);

        client.IsAvailable.Should().BeFalse();

        var result = await client.GetEmbeddingAsync("some text");

        result.Should().BeNull();
    }

    [Fact]
    public void Constructor_WhenBaseUrlIsConfigured_ShouldMarkAsAvailable()
    {
        var configuration = new Mock<IConfiguration>();
        configuration.Setup(c => c["Ollama:BaseUrl"]).Returns("http://192.168.1.47:11434");
        configuration.Setup(c => c["Ollama:Model"]).Returns("nomic-embed-text-v2-moe");

        var client = new OllamaClient(configuration.Object);

        client.IsAvailable.Should().BeTrue();
    }

    [Fact]
    public async Task GetEmbeddingAsync_WithEmptyText_ShouldNotThrow()
    {
        var configuration = new Mock<IConfiguration>();
        configuration.Setup(c => c["Ollama:BaseUrl"]).Returns(string.Empty);

        var client = new OllamaClient(configuration.Object);

        var result = await client.GetEmbeddingAsync(string.Empty);

        result.Should().BeNull();
    }

    [Fact]
    public void Constructor_WhenBaseUrlIsNull_ShouldNotBeAvailable()
    {
        var configuration = new Mock<IConfiguration>();
        configuration.Setup(c => c["Ollama:BaseUrl"]).Returns((string?)null);

        var client = new OllamaClient(configuration.Object);

        client.IsAvailable.Should().BeFalse();
    }
}
