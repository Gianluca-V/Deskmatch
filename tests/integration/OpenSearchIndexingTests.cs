using DeskMatch.SDK.OpenSearch;
using DeskMatch.SDK.OpenSearch.Documents;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using OpenSearch.Client;
using Xunit;

namespace DeskMatch.IntegrationTests;

public class OpenSearchIndexingTests
{
    private const string TestIndex = "offices_test";

    private static (IOpenSearchClient? client, IOpenSearchRepository<WorkspaceDocument>? repo, bool available) Connect()
    {
        var config = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .Build();

        var uri = config["OPENSEARCH_URL"] ?? "http://localhost:9200";
        var username = config["OPENSEARCH_USER"] ?? "admin";
        var password = config["OPENSEARCH_PASS"] ?? "admin";

        try
        {
            var settings = new ConnectionSettings(new Uri(uri))
                .BasicAuthentication(username, password)
                .ServerCertificateValidationCallback((_, _, _, _) => true);

            var client = new OpenSearchClient(settings);
            var ping = client.Ping();
            if (!ping.IsValid)
                return (null, null, false);

            return (client, new OpenSearchRepository<WorkspaceDocument>(client), true);
        }
        catch
        {
            return (null, null, false);
        }
    }

    [Fact]
    public async Task IndexAndSearch_ShouldStoreAndRetrieveDocument()
    {
        var (client, repo, available) = Connect();
        if (!available || client == null || repo == null) return;

        var doc = new WorkspaceDocument
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Test Office",
            Description = "A test workspace",
            City = "Buenos Aires",
            Country = "Argentina",
            Capacity = 10,
            PricePerHour = 25.0,
            Amenities = new List<string> { "wifi", "ac" },
            DynamicAttributes = new Dictionary<string, object>
            {
                { "pet-friendly", "true" },
                { "24-7-access", false }
            }
        };

        var indexResp = await repo.IndexAsync(doc, TestIndex);
        indexResp.IsValid.Should().BeTrue();

        await Task.Delay(1000);

        var getResp = await client.GetAsync<WorkspaceDocument>(doc.Id, g => g.Index(TestIndex));
        getResp.Found.Should().BeTrue();
        getResp.Source.Name.Should().Be("Test Office");

        var searchResp = await repo.SearchAsync(s => s
            .Index(TestIndex)
            .Query(q => q.MultiMatch(mm => mm
                .Fields(f => f.Field(d => d.Name, 3).Field(d => d.Description, 2))
                .Query("test office")
            ))
        );

        searchResp.Hits.Should().NotBeEmpty();
        searchResp.Hits.First().Source.Id.Should().Be(doc.Id);

        await repo.DeleteAsync(doc.Id, TestIndex);
    }

    [Fact]
    public async Task SearchWithFilters_ShouldReturnFilteredResults()
    {
        var (client, repo, available) = Connect();
        if (!available || client == null || repo == null) return;

        await repo.IndexAsync(new WorkspaceDocument
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Premium Office",
            City = "Buenos Aires",
            Country = "Argentina",
            Capacity = 50,
            PricePerHour = 100.0,
            Amenities = new List<string> { "wifi", "parking" },
            Rating = 4.8
        }, TestIndex);

        await repo.IndexAsync(new WorkspaceDocument
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Budget Space",
            City = "Buenos Aires",
            Country = "Argentina",
            Capacity = 5,
            PricePerHour = 5.0,
            Amenities = new List<string> { "wifi" }
        }, TestIndex);

        await Task.Delay(1000);

        var result = await repo.SearchAsync(s => s
            .Index(TestIndex)
            .Query(q => q.Bool(b => b
                .Filter(f => f.Term(t => t.Field(d => d.City).Value("Buenos Aires")))
                .Filter(f => f.Range(r => r.Field(d => d.PricePerHour).GreaterThanOrEquals(50.0)))
            ))
        );

        result.Hits.Should().HaveCount(1);
        result.Hits.First().Source.Name.Should().Be("Premium Office");
    }

    [Fact]
    public async Task GeoFilter_ShouldReturnNearbyOffices()
    {
        var (client, repo, available) = Connect();
        if (!available || client == null || repo == null) return;

        await repo.IndexAsync(new WorkspaceDocument
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Palermo Office",
            City = "Buenos Aires",
            Capacity = 15,
            PricePerHour = 20.0,
            Location = new GeoLocation(-34.5888, -58.4306)
        }, TestIndex);

        await Task.Delay(1000);

        var result = await repo.SearchAsync(s => s
            .Index(TestIndex)
            .Query(q => q.GeoDistance(g => g
                .Field(d => d.Location)
                .Distance("50km")
                .Location(-34.6037, -58.3816)
            ))
        );

        result.Hits.Should().NotBeEmpty();
    }

    [Fact]
    public async Task BulkIndex_ShouldStoreMultipleDocuments()
    {
        var (client, repo, available) = Connect();
        if (!available || client == null || repo == null) return;

        var docs = new[]
        {
            new WorkspaceDocument { Id = Guid.NewGuid().ToString(), Name = "Office A", Capacity = 10, PricePerHour = 20.0 },
            new WorkspaceDocument { Id = Guid.NewGuid().ToString(), Name = "Office B", Capacity = 20, PricePerHour = 30.0 },
            new WorkspaceDocument { Id = Guid.NewGuid().ToString(), Name = "Office C", Capacity = 30, PricePerHour = 40.0 }
        };

        var response = await repo.BulkIndexAsync(docs, TestIndex);
        response.IsValid.Should().BeTrue();
    }
}
