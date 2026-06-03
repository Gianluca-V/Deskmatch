using DeskMatch.SDK.Storage;
using Microsoft.AspNetCore.Mvc;

namespace DeskMatch.CoreService.Api.Controllers;

[ApiController]
[Route("api/storage")]
public class StorageController : ControllerBase
{
    private readonly IStorageService _storage;

    public StorageController(IStorageService storage)
    {
        _storage = storage;
    }

    [HttpPost("upload")]
    [RequestSizeLimit(10_485_760)]
    public async Task<IActionResult> Upload(
        [FromForm] IFormFile file,
        [FromForm] string container = "uploads")
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { error = "No file provided" });

        var fileName = $"{Guid.NewGuid():N}_{file.FileName}";

        await using var stream = file.OpenReadStream();
        var url = await _storage.UploadAsync(container, fileName, stream, file.ContentType);

        return Ok(new { url, fileName, container });
    }

    [HttpGet("{container}/{fileName}")]
    public async Task<IActionResult> Download(string container, string fileName)
    {
        try
        {
            var stream = await _storage.DownloadAsync(container, fileName);
            return File(stream, "application/octet-stream", fileName);
        }
        catch (FileNotFoundException)
        {
            return NotFound(new { error = "File not found" });
        }
    }

    [HttpGet("url/{container}/{fileName}")]
    public async Task<IActionResult> GetUrl(string container, string fileName)
    {
        var url = await _storage.GetUrlAsync(container, fileName);
        return Ok(new { url });
    }

    [HttpDelete("{container}/{fileName}")]
    public async Task<IActionResult> Delete(string container, string fileName)
    {
        await _storage.DeleteAsync(container, fileName);
        return NoContent();
    }
}
