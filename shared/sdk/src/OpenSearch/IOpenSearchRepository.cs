using OpenSearch.Client;

namespace DeskMatch.SDK.OpenSearch;

public interface IOpenSearchRepository<T> where T : class
{
    Task<IndexResponse> IndexAsync(T document, string? index = null);
    Task<ISearchResponse<T>> SearchAsync(Func<SearchDescriptor<T>, ISearchRequest> selector, string? index = null);
    Task<DeleteResponse> DeleteAsync(string id, string? index = null);
    Task<UpdateResponse<T>> UpdateAsync(string id, object partialDocument, string? index = null);
    Task<BulkResponse> BulkIndexAsync(IEnumerable<T> documents, string? index = null);
}