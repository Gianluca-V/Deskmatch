using OpenSearch.Client;

namespace DeskMatch.SDK.OpenSearch;

public class OpenSearchRepository<T> : IOpenSearchRepository<T> where T : class
{
    private readonly IOpenSearchClient _client;

    public OpenSearchRepository(IOpenSearchClient client)
    {
        _client = client;
    }

    public async Task<IndexResponse> IndexAsync(T document, string? index = null)
    {
        if (!string.IsNullOrEmpty(index))
            return await _client.IndexAsync(document, i => i.Index(index));

        return await _client.IndexDocumentAsync(document);
    }

    public async Task<ISearchResponse<T>> SearchAsync(Func<SearchDescriptor<T>, ISearchRequest> selector, string? index = null)
    {
        var request = selector(new SearchDescriptor<T>().Index(index));
        return await _client.SearchAsync<T>(request);
    }

    public async Task<DeleteResponse> DeleteAsync(string id, string? index = null)
    {
        return await _client.DeleteAsync<T>(id, d => d.Index(index));
    }

    public async Task<UpdateResponse<T>> UpdateAsync(string id, object partialDocument, string? index = null)
    {
        return await _client.UpdateAsync<T, object>(id, u => u.Index(index).Doc(partialDocument));
    }

    public async Task<BulkResponse> BulkIndexAsync(IEnumerable<T> documents, string? index = null)
    {
        var descriptor = new BulkDescriptor();

        foreach (var document in documents)
        {
            descriptor.Index<T>(i => i.Document(document).Index(index));
        }

        return await _client.BulkAsync(descriptor);
    }
}