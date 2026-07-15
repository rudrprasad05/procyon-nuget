namespace Procyon.Email.Abstractions;

/// <summary>
/// Represents a provider-neutral email attachment.
/// </summary>
public sealed record EmailAttachment
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EmailAttachment" /> class.
    /// </summary>
    /// <param name="fileName">The attachment file name.</param>
    /// <param name="contentType">The attachment content type.</param>
    /// <param name="content">The attachment content bytes.</param>
    /// <param name="contentId">The optional inline content identifier.</param>
    /// <param name="isInline">Whether the attachment should be treated as inline content.</param>
    /// <exception cref="EmailValidationException">Thrown when required attachment metadata is missing.</exception>
    public EmailAttachment(
        string fileName,
        string contentType,
        ReadOnlyMemory<byte> content,
        string? contentId = null,
        bool isInline = false)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new EmailValidationException("Attachment file name is required.");
        }

        if (string.IsNullOrWhiteSpace(contentType))
        {
            throw new EmailValidationException("Attachment content type is required.");
        }

        FileName = fileName;
        ContentType = contentType;
        Content = content;
        ContentId = string.IsNullOrWhiteSpace(contentId) ? null : contentId;
        IsInline = isInline;
    }

    /// <summary>
    /// Gets the attachment file name.
    /// </summary>
    public string FileName { get; }

    /// <summary>
    /// Gets the attachment content type.
    /// </summary>
    public string ContentType { get; }

    /// <summary>
    /// Gets the attachment content bytes.
    /// </summary>
    public ReadOnlyMemory<byte> Content { get; }

    /// <summary>
    /// Gets the optional inline content identifier.
    /// </summary>
    public string? ContentId { get; }

    /// <summary>
    /// Gets a value indicating whether the attachment should be treated as inline content.
    /// </summary>
    public bool IsInline { get; }
}
