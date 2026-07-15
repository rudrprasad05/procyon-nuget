namespace Procyon.Email.Resend.Models;

internal sealed record ResendEmailAttachment(
    string FileName,
    string ContentType,
    ReadOnlyMemory<byte> Content,
    string? ContentId,
    bool IsInline);
