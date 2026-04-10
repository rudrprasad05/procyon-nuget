using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Procyon.Media.Abstractions.Interfaces;

public interface IMediaProvider
{
    Task<string> UploadAsync(
        Stream stream,
        string path,
        string contentType,
        CancellationToken ct);

    Task DeleteAsync(string path, CancellationToken ct);

    Task<Stream> GetAsync(string path, CancellationToken ct);

    Task<string> GetSignedUrlAsync(
        string path,
        TimeSpan expiry,
        CancellationToken ct);
}