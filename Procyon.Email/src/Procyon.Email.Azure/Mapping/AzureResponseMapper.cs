using Procyon.Email.Abstractions;
using Procyon.Email.Azure.Models;
using Procyon.Email.Providers;

namespace Procyon.Email.Azure.Mapping;

internal sealed class AzureResponseMapper
{
    public EmailProviderResult MapAccepted(AzureEmailResponse response)
    {
        return EmailProviderResult.Accepted(response.Id);
    }

    public EmailProviderResult MapFailure(
        EmailSendStatus status,
        string errorCode,
        string errorMessage)
    {
        return new EmailProviderResult
        {
            Succeeded = false,
            Status = status,
            ErrorCode = errorCode,
            ErrorMessage = errorMessage
        };
    }
}
