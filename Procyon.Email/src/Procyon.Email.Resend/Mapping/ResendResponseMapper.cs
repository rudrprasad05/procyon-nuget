using Procyon.Email.Abstractions;
using Procyon.Email.Providers;
using Procyon.Email.Resend.Models;

namespace Procyon.Email.Resend.Mapping;

internal sealed class ResendResponseMapper
{
    public EmailProviderResult MapAccepted(ResendEmailResponse response)
    {
        return EmailProviderResult.Accepted(response.MessageId);
    }

    public EmailProviderResult MapFailure(string errorCode, string errorMessage)
    {
        return new EmailProviderResult
        {
            Succeeded = false,
            Status = EmailSendStatus.Failed,
            ErrorCode = errorCode,
            ErrorMessage = errorMessage
        };
    }
}
