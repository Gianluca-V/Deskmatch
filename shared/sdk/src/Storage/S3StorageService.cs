using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;

namespace DeskMatch.SDK.Storage;

public class S3StorageOptions
{
    public string Endpoint { get; set; } = "localhost:9000";
    public string AccessKey { get; set; } = "minioadmin";
    public string SecretKey { get; set; } = "minioadmin";
    public string BucketName { get; set; } = "deskmatch";
    public bool UseSsl { get; set; } = false;
    public string PublicUrl { get; set; } = "http://localhost:9000";
}

public class S3StorageService : IStorageService
{
    private readonly IMinioClient _minio;
    private readonly S3StorageOptions _options;

    public S3StorageService(IOptions<S3StorageOptions> options)
    {
        _options = options.Value;

        _minio = new MinioClient()
            .WithEndpoint(_options.Endpoint)
            .WithCredentials(_options.AccessKey, _options.SecretKey);

        if (_options.UseSsl)
            _minio = _minio.WithSSL();

        _minio = _minio.Build();
    }

    public async Task<string> UploadAsync(string container, string fileName, Stream content, string contentType)
    {
        var objectName = $"{container}/{fileName}";

        var args = new PutObjectArgs()
            .WithBucket(_options.BucketName)
            .WithObject(objectName)
            .WithStreamData(content)
            .WithObjectSize(content.Length)
            .WithContentType(contentType);

        await _minio.PutObjectAsync(args);

        return $"{_options.PublicUrl}/{_options.BucketName}/{objectName}";
    }

    public async Task<Stream> DownloadAsync(string container, string fileName)
    {
        var objectName = $"{container}/{fileName}";
        var memoryStream = new MemoryStream();

        var args = new GetObjectArgs()
            .WithBucket(_options.BucketName)
            .WithObject(objectName)
            .WithCallbackStream(stream => stream.CopyTo(memoryStream));

        await _minio.GetObjectAsync(args);
        memoryStream.Position = 0;

        return memoryStream;
    }

    public async Task DeleteAsync(string container, string fileName)
    {
        var objectName = $"{container}/{fileName}";

        var args = new RemoveObjectArgs()
            .WithBucket(_options.BucketName)
            .WithObject(objectName);

        await _minio.RemoveObjectAsync(args);
    }

    public Task<string> GetUrlAsync(string container, string fileName)
    {
        var objectName = $"{container}/{fileName}";
        var url = $"{_options.PublicUrl}/{_options.BucketName}/{objectName}";
        return Task.FromResult(url);
    }
}
