namespace DeskMatch.SDK.Storage;

public interface IStorageService
{
    Task<string> UploadAsync(string container, string fileName, Stream content, string contentType);
    Task<Stream> DownloadAsync(string container, string fileName);
    Task DeleteAsync(string container, string fileName);
    Task<string> GetUrlAsync(string container, string fileName);
}