using DeskMatch.SDK.OpenSearch;
using DeskMatch.SDK.Ollama;
using DeskMatch.SDK.OpenSearch.Documents;
using DeskMatch.SearchService.Application.Search;
using FluentAssertions;
using Moq;
using OpenSearch.Client;
using Xunit;

namespace DeskMatch.UnitTests.Application;

public class SearchOfficesQueryHandlerTests
{
    private readonly Mock<IOpenSearchRepository<WorkspaceDocument>> _searchRepoMock;
    private readonly Mock<IOllamaClient> _ollamaMock;
    private readonly SearchOfficesQueryHandler _handler;

    public SearchOfficesQueryHandlerTests()
    {
        _searchRepoMock = new Mock<IOpenSearchRepository<WorkspaceDocument>>();
        _ollamaMock = new Mock<IOllamaClient>();
        _handler = new SearchOfficesQueryHandler(_searchRepoMock.Object, _ollamaMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WithTextQuery_ShouldCallSearchWithBM25()
    {
        var query = new SearchOfficesQuery { Text = "modern workspace", Page = 1, PageSize = 10 };
        SetupSearchResponseWithHits(new[]
        {
            new WorkspaceDocument { Id = "1", Name = "Modern Office", City = "Buenos Aires" }
        });

        _ollamaMock.Setup(o => o.GetEmbeddingAsync("modern workspace")).ReturnsAsync((float[]?)null);

        var result = await _handler.HandleAsync(query);

        result.Items.Should().HaveCount(1);
        result.Items[0].Id.Should().Be("1");
        result.Items[0].Name.Should().Be("Modern Office");
        _searchRepoMock.Verify(r => r.SearchAsync(
            It.IsAny<Func<SearchDescriptor<WorkspaceDocument>, ISearchRequest>>(),
            null), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithEmbedding_ShouldUseHybridSearch()
    {
        var embedding = new float[] { 0.1f, 0.2f, 0.3f };
        var query = new SearchOfficesQuery { Text = "creative space", Page = 1, PageSize = 10 };

        SetupSearchResponseWithHits(new[]
        {
            new WorkspaceDocument { Id = "2", Name = "Creative Hub" }
        });

        _ollamaMock.Setup(o => o.GetEmbeddingAsync("creative space")).ReturnsAsync(embedding);

        var result = await _handler.HandleAsync(query);

        result.Items.Should().HaveCount(1);
        _searchRepoMock.Verify(r => r.SearchAsync(
            It.IsAny<Func<SearchDescriptor<WorkspaceDocument>, ISearchRequest>>(),
            null), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithoutOllama_ShouldStillReturnResults()
    {
        var query = new SearchOfficesQuery { Text = "office", Page = 1, PageSize = 10 };

        SetupSearchResponseWithHits(new[]
        {
            new WorkspaceDocument { Id = "3", Name = "Office Space" }
        });

        _ollamaMock.Setup(o => o.GetEmbeddingAsync("office")).ReturnsAsync((float[]?)null);

        var result = await _handler.HandleAsync(query);

        result.Items.Should().HaveCount(1);
        result.Items[0].Id.Should().Be("3");
    }

    [Fact]
    public async Task HandleAsync_WithEmptyText_ShouldNotThrow()
    {
        var query = new SearchOfficesQuery { Page = 1, PageSize = 10 };
        SetupSearchResponseWithHits(Array.Empty<WorkspaceDocument>());

        _ollamaMock.Setup(o => o.GetEmbeddingAsync("")).ReturnsAsync((float[]?)null);

        var result = await _handler.HandleAsync(query);

        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task HandleAsync_WithGeoLocation_ShouldReturnResults()
    {
        var query = new SearchOfficesQuery
        {
            Lat = -34.6037,
            Lon = -58.3816,
            RadiusKm = 50,
            Page = 1,
            PageSize = 10
        };
        SetupSearchResponseWithHits(new[]
        {
            new WorkspaceDocument { Id = "4", Name = "Nearby Office", City = "Buenos Aires" }
        });

        _ollamaMock.Setup(o => o.GetEmbeddingAsync("")).ReturnsAsync((float[]?)null);

        var result = await _handler.HandleAsync(query);

        result.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task HandleAsync_WithAllFilters_ShouldCallSearchOnce()
    {
        var query = new SearchOfficesQuery
        {
            Text = "modern",
            City = "Buenos Aires",
            Country = "Argentina",
            MinPrice = 10,
            MaxPrice = 50,
            MinCapacity = 5,
            Amenities = new List<string> { "wifi", "ac" },
            Lat = -34.6037,
            Lon = -58.3816,
            RadiusKm = 10,
            Page = 1,
            PageSize = 20
        };

        var embedding = new float[] { 1.0f, 2.0f, 3.0f };
        SetupSearchResponseWithHits(Array.Empty<WorkspaceDocument>());

        _ollamaMock.Setup(o => o.GetEmbeddingAsync("modern")).ReturnsAsync(embedding);

        var result = await _handler.HandleAsync(query);

        result.Items.Should().BeEmpty();
        _searchRepoMock.Verify(r => r.SearchAsync(
            It.IsAny<Func<SearchDescriptor<WorkspaceDocument>, ISearchRequest>>(),
            null), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithMultipleResults_ShouldReturnAllMapped()
    {
        var query = new SearchOfficesQuery { Text = "workspace", Page = 1, PageSize = 20 };

        SetupSearchResponseWithHits(new[]
        {
            new WorkspaceDocument
            {
                Id = "5",
                Name = "Space A",
                Description = "Desc A",
                City = "Buenos Aires",
                Country = "Argentina",
                Address = "123 Main St",
                Capacity = 10,
                PricePerHour = 25.0,
                Amenities = new List<string> { "wifi" },
                DynamicAttributes = new Dictionary<string, object> { { "pet-friendly", "true" } },
                Location = new GeoLocation(-34.6037, -58.3816),
                Rating = 4.5,
                ReviewCount = 100
            },
            new WorkspaceDocument
            {
                Id = "6",
                Name = "Space B",
                City = "Rosario"
            }
        });

        _ollamaMock.Setup(o => o.GetEmbeddingAsync("workspace")).ReturnsAsync((float[]?)null);

        var result = await _handler.HandleAsync(query);

        result.Items.Should().HaveCount(2);
        result.Items[0].Id.Should().Be("5");
        result.Items[0].Lat.Should().Be(-34.6037);
        result.Items[0].DynamicAttributes.Should().ContainKey("pet-friendly");
        result.Items[1].Id.Should().Be("6");
        result.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnTotalPagesCorrectly()
    {
        var query = new SearchOfficesQuery { Page = 1, PageSize = 2 };

        SetupSearchResponseWithTotalHits(5, new[]
        {
            new WorkspaceDocument { Id = "a" },
            new WorkspaceDocument { Id = "b" }
        });

        _ollamaMock.Setup(o => o.GetEmbeddingAsync("")).ReturnsAsync((float[]?)null);

        var result = await _handler.HandleAsync(query);

        result.TotalPages.Should().Be(3);
    }

    private void SetupSearchResponseWithHits(IReadOnlyList<WorkspaceDocument> hits)
    {
        SetupSearchResponseWithTotalHits(hits.Count, hits);
    }

    private void SetupSearchResponseWithTotalHits(long total, IReadOnlyList<WorkspaceDocument> hits)
    {
        var searchResponseMock = new Mock<ISearchResponse<WorkspaceDocument>>();
        var hitsCollection = hits.Select((doc, i) =>
        {
            var hitMock = new Mock<IHit<WorkspaceDocument>>();
            hitMock.SetupGet(h => h.Source).Returns(doc);
            return hitMock.Object;
        }).ToList();

        searchResponseMock.SetupGet(r => r.Hits).Returns(hitsCollection);
        searchResponseMock.SetupGet(r => r.Total).Returns(total);

        _searchRepoMock
            .Setup(r => r.SearchAsync(
                It.IsAny<Func<SearchDescriptor<WorkspaceDocument>, ISearchRequest>>(),
                null))
            .ReturnsAsync(searchResponseMock.Object);
    }
}
