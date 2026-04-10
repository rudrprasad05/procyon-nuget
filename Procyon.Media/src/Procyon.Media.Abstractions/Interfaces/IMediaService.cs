using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Procyon.Media.Abstractions.Models;

namespace Procyon.Media.Abstractions.Interfaces
{

    public interface IMediaService
    {
        Task<MediaUploadResult> UploadAsync(
            Stream stream,
            MediaUploadOptions options,
            CancellationToken ct = default);

        Task DeleteAsync(string key, CancellationToken ct = default);

        Task<Stream> GetAsync(string key, CancellationToken ct = default);

        Task<string> GetSignedUrlAsync(
            string key,
            TimeSpan expiry,
            CancellationToken ct = default);
    }
}