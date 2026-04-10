using System.Security.Cryptography;
using Procyon.Media.Abstractions.Interfaces;
namespace Procyon.Media.Services;

public static class FileHashing
{
    public static async Task<string> ComputeSha256Async(Stream stream)
    {
        using var sha = SHA256.Create();
        var hashBytes = await sha.ComputeHashAsync(stream);
        return Convert.ToHexString(hashBytes);
    }
}