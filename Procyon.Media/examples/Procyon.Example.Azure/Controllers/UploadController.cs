using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Procyon.Example.Azure.Data;
using Procyon.Media.Abstractions.Interfaces;
using Procyon.Media.Abstractions.Models;

namespace Procyon.Example.Azure.Controllers;

[ApiController]
[Route("api/upload")]
public class UploadController : ControllerBase
{
    private readonly IMediaService _mediaService;
    private readonly AppDbContext _db;

    public UploadController(IMediaService mediaService, AppDbContext db)
    {
        _mediaService = mediaService;
        _db = db;
    }

    [HttpPost]
    public async Task<IActionResult> Upload(IFormFile file, CancellationToken ct)
    {
        if (file == null || file.Length == 0)
            return BadRequest("File is required");

        var result = await _mediaService.UploadAsync(
            file.OpenReadStream(),
            new MediaUploadOptions
            {
                FileName = file.FileName,
                ContentType = file.ContentType
            },
            ct);

        var entity = new MediaFile
        {
            Id = Guid.NewGuid(),
            Key = result.Key,
            Url = result.Url,
            FileName = file.FileName
        };

        _db.MediaFiles.Add(entity);
        await _db.SaveChangesAsync(ct);

        return Ok(entity);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var items = await _db.MediaFiles.ToListAsync(ct);
        return Ok(items);
    }

    [HttpGet("signed-url")]
    public async Task<IActionResult> GetSignedUrl(
        [FromQuery] string key,
        [FromQuery] int expiresInSeconds)
    {
        var url = await _mediaService.GetSignedUrlAsync(
            key,
            TimeSpan.FromSeconds(expiresInSeconds));

        return Ok(new { url });
    }

    [HttpDelete]
    public async Task<IActionResult> Delete([FromQuery] string key, CancellationToken ct)
    {
        await _mediaService.DeleteAsync(key, ct);
        return NoContent();
    }

    [HttpGet("file")]
    public async Task<IActionResult> Download([FromQuery] string key, CancellationToken ct)
    {
        var stream = await _mediaService.GetAsync(key, ct);
        return File(stream, "application/octet-stream", Path.GetFileName(key));
    }

    [HttpGet("public-url")]
    public IActionResult GetPublicUrl([FromQuery] string key, [FromServices] IMediaUrlResolver resolver)
    {
        return Ok(new { url = resolver.Resolve(key) });
    }
}
