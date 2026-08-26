namespace Procyon.Email.Azure.Models;

internal sealed record AzureEmailAttachment(
    string Name,
    string ContentType,
    ReadOnlyMemory<byte> Content,
    string? ContentId,
    bool IsInline);
