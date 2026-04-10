using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Procyon.Example.Data;
using Procyon.Media.Abstractions.Interfaces;
using Procyon.Media.Abstractions.Models;

namespace Procyon.Example.Controllers;

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
}