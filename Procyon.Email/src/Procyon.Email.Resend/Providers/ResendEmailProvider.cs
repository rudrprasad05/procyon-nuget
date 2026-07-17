using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Procyon.Email.Abstractions;
using Procyon.Email.Providers;
using Procyon.Email.Resend.Configuration;
using Procyon.Email.Resend.Internal;
using Procyon.Email.Resend.Mapping;
using Procyon.Email.Resend.Models;

namespace Procyon.Email.Resend.Providers;

internal sealed class ResendEmailProvider(
    HttpClient httpClient,
    ResendApiKeyProvider apiKeyProvider,
    ResendRequestMapper requestMapper,
    ResendResponseMapper responseMapper) : IEmailProvider
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public string Name => ResendEmailOptions.ProviderName;

    public EmailProviderCapabilities Capabilities =>
        EmailProviderCapabilities.HtmlBody |
        EmailProviderCapabilities.TextBody |
        EmailProviderCapabilities.Attachments |
        EmailProviderCapabilities.Cc |
        EmailProviderCapabilities.Bcc |
        EmailProviderCapabilities.ReplyTo |
        EmailProviderCapabilities.CustomHeaders |
        EmailProviderCapabilities.Tags |
        EmailProviderCapabilities.Idempotency;

    public async Task<EmailProviderResult> SendAsync(
        EmailMessage message,
        CancellationToken cancellationToken = default)
    {
        var apiKey = apiKeyProvider.GetApiKey();
        var resendRequest = requestMapper.Map(message);
        var payload = ResendSendEmailPayload.FromRequest(resendRequest);
        var content = JsonSerializer.Serialize(payload, JsonOptions);

        using var request = new HttpRequestMessage(HttpMethod.Post, "emails")
        {
            Content = new StringContent(content, Encoding.UTF8, "application/json")
        };

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        if (!string.IsNullOrWhiteSpace(resendRequest.IdempotencyKey))
        {
            request.Headers.TryAddWithoutValidation("Idempotency-Key", resendRequest.IdempotencyKey);
        }

        try
        {
            using var response = await httpClient
                .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
                .ConfigureAwait(false);

            var responseContent = await response.Content
                .ReadAsStringAsync(cancellationToken)
                .ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                return responseMapper.MapAccepted(DeserializeSuccess(responseContent));
            }

            var error = DeserializeError(responseContent, response.StatusCode);
            return responseMapper.MapFailure(
                MapStatus(response.StatusCode),
                error.Code,
                error.Message);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException)
        {
            return responseMapper.MapFailure(
                EmailSendStatus.Failed,
                "resend_transport_error",
                $"Resend email request failed: {exception.Message}");
        }
    }

    private static ResendEmailResponse DeserializeSuccess(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return new ResendEmailResponse(null);
        }

        try
        {
            return JsonSerializer.Deserialize<ResendEmailResponse>(content, JsonOptions)
                ?? new ResendEmailResponse(null);
        }
        catch (JsonException)
        {
            return new ResendEmailResponse(null);
        }
    }

    private static ResendError DeserializeError(string content, HttpStatusCode statusCode)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return new ResendError(
                statusCode.ToString(),
                $"Resend returned HTTP {(int)statusCode}.");
        }

        try
        {
            using var document = JsonDocument.Parse(content);
            var root = document.RootElement;
            var source = root.TryGetProperty("error", out var nestedError) && nestedError.ValueKind == JsonValueKind.Object
                ? nestedError
                : root;

            var code = ReadString(source, "name")
                ?? ReadString(source, "type")
                ?? ReadString(source, "code")
                ?? statusCode.ToString();
            var message = ReadString(source, "message")
                ?? $"Resend returned HTTP {(int)statusCode}.";

            return new ResendError(code, message);
        }
        catch (JsonException)
        {
            return new ResendError(statusCode.ToString(), content);
        }
    }

    private static string? ReadString(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String
            ? property.GetString()
            : null;
    }

    private static EmailSendStatus MapStatus(HttpStatusCode statusCode)
    {
        var code = (int)statusCode;
        return code == 408 || code == 429 || code >= 500
            ? EmailSendStatus.Failed
            : EmailSendStatus.Rejected;
    }

    private sealed record ResendError(string Code, string Message);

    private sealed record ResendSendEmailPayload(
        string From,
        IReadOnlyCollection<string> To,
        IReadOnlyCollection<string>? Cc,
        IReadOnlyCollection<string>? Bcc,
        [property: JsonPropertyName("reply_to")]
        IReadOnlyCollection<string>? ReplyTo,
        string Subject,
        string? Html,
        string? Text,
        IReadOnlyCollection<ResendSendEmailAttachment>? Attachments,
        IReadOnlyDictionary<string, string>? Headers,
        IReadOnlyCollection<ResendSendEmailTag>? Tags)
    {
        public static ResendSendEmailPayload FromRequest(ResendEmailRequest request)
        {
            return new ResendSendEmailPayload(
                FormatAddress(request.From),
                FormatAddresses(request.To),
                EmptyToNull(FormatAddresses(request.Cc)),
                EmptyToNull(FormatAddresses(request.Bcc)),
                EmptyToNull(FormatAddresses(request.ReplyTo)),
                request.Subject,
                request.HtmlBody,
                request.TextBody,
                EmptyToNull(request.Attachments.Select(MapAttachment).ToArray()),
                EmptyToNull(request.Headers),
                EmptyToNull(request.Tags.Select(tag => new ResendSendEmailTag(tag.Key, tag.Value)).ToArray()));
        }

        private static string FormatAddress(ResendEmailRecipient recipient)
        {
            return string.IsNullOrWhiteSpace(recipient.Name)
                ? recipient.Address
                : $"{recipient.Name} <{recipient.Address}>";
        }

        private static IReadOnlyCollection<string> FormatAddresses(
            IReadOnlyCollection<ResendEmailRecipient> recipients)
        {
            return recipients.Select(FormatAddress).ToArray();
        }

        private static ResendSendEmailAttachment MapAttachment(ResendEmailAttachment attachment)
        {
            return new ResendSendEmailAttachment(
                attachment.FileName,
                Convert.ToBase64String(attachment.Content.Span));
        }

        private static IReadOnlyCollection<T>? EmptyToNull<T>(IReadOnlyCollection<T> values)
        {
            return values.Count == 0 ? null : values;
        }

        private static IReadOnlyDictionary<string, string>? EmptyToNull(
            IReadOnlyDictionary<string, string> values)
        {
            return values.Count == 0 ? null : values;
        }
    }

    private sealed record ResendSendEmailAttachment(
        [property: JsonPropertyName("filename")]
        string FileName,
        [property: JsonPropertyName("content")]
        string Content);

    private sealed record ResendSendEmailTag(string Name, string Value);
}
