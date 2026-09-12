using Procyon.Media;
using Procyon.Media.Abstractions.Interfaces;
using Procyon.Media.Abstractions.Models;
using Procyon.Media.Azure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProcyonMedia(builder.Configuration);
builder.Services.AddAzureMediaProvider(builder.Configuration);

var app = builder.Build();

app.MapGet("/", () => Results.Ok(new
{
    endpoints = new[]
    {
        "POST /media",
        "GET /media?key=uploads/example.png",
        "DELETE /media?key=uploads/example.png",
        "GET /media/url?key=uploads/example.png&expiresInMinutes=15",
        "GET /media/public-url?key=uploads/example.png"
    }
}));

app.MapPost("/media", async (
    IFormFile file,
    IMediaService media,
    CancellationToken ct) =>
{
    if (file.Length == 0)
    {
        return Results.BadRequest(new { error = "A non-empty file is required." });
    }

    await using var stream = file.OpenReadStream();
    var result = await media.UploadAsync(
        stream,
        new MediaUploadOptions
        {
            FileName = file.FileName,
            ContentType = file.ContentType
        },
        ct);

    return Results.Created($"/media?key={Uri.EscapeDataString(result.Key)}", result);
}).DisableAntiforgery();

app.MapGet("/media", async (
    string key,
    IMediaService media,
    CancellationToken ct) =>
{
    var stream = await media.GetAsync(key, ct);
    return Results.Stream(stream, "application/octet-stream", fileDownloadName: Path.GetFileName(key));
});

app.MapDelete("/media", async (
    string key,
    IMediaService media,
    CancellationToken ct) =>
{
    await media.DeleteAsync(key, ct);
    return Results.NoContent();
});

app.MapGet("/media/url", async (
    string key,
    int? expiresInMinutes,
    IMediaService media,
    CancellationToken ct) =>
{
    var expiry = TimeSpan.FromMinutes(expiresInMinutes ?? 15);
    var url = await media.GetSignedUrlAsync(key, expiry, ct);
    return Results.Ok(new { key, url, expiresIn = expiry });
});

app.MapGet("/media/public-url", (
    string key,
    IMediaUrlResolver urlResolver) =>
    Results.Ok(new { key, url = urlResolver.Resolve(key) }));

app.Run();
