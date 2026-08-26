using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Procyon.Email.Abstractions;
using Procyon.Email.Azure.Configuration;
using Procyon.Email.Azure.Internal;
using Procyon.Email.Azure.Mapping;
using Procyon.Email.Azure.Models;
using Procyon.Email.Providers;

namespace Procyon.Email.Azure.Providers;

internal sealed class AzureEmailProvider(
    HttpClient httpClient,
    AzureApiKeyProvider apiKeyProvider,
    AzureRequestMapper requestMapper,
    AzureResponseMapper responseMapper) : IEmailProvider
{
    private const string SendPathAndQuery = "/emails:send?api-version=2023-03-31";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public string Name => AzureEmailOptions.ProviderName;

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
        var connectionDetails = apiKeyProvider.GetConnectionDetails();
        var azureRequest = requestMapper.Map(message);
        var payload = AzureSendEmailPayload.FromRequest(azureRequest);
        var content = JsonSerializer.Serialize(payload, JsonOptions);

        using var request = new HttpRequestMessage(HttpMethod.Post, SendPathAndQuery)
        {
            Content = new StringContent(content, Encoding.UTF8, "application/json")
        };

        if (!string.IsNullOrWhiteSpace(azureRequest.IdempotencyKey))
        {
            request.Headers.TryAddWithoutValidation("Operation-Id", azureRequest.IdempotencyKey);
            request.Headers.TryAddWithoutValidation("x-ms-client-request-id", azureRequest.IdempotencyKey);
        }

        SignRequest(request, httpClient.BaseAddress, content, connectionDetails.AccessKey);

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
                return responseMapper.MapAccepted(DeserializeSuccess(responseContent, response));
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
                "azure_transport_error",
                $"Azure email request failed: {exception.Message}");
        }
    }

    private static void SignRequest(
        HttpRequestMessage request,
        Uri? baseAddress,
        string content,
        string accessKey)
    {
        var requestUri = request.RequestUri
            ?? throw new InvalidOperationException("Azure email request URI is required.");
        var absoluteUri = requestUri.IsAbsoluteUri
            ? requestUri
            : new Uri(baseAddress ?? throw new InvalidOperationException("Azure email API base address is required."), requestUri);
        var host = absoluteUri.Authority;

        if (string.IsNullOrWhiteSpace(host))
        {
            throw new InvalidOperationException("Azure email request host is required for request signing.");
        }

        var timestamp = DateTimeOffset.UtcNow.ToString("r", CultureInfo.InvariantCulture);
        var contentHash = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(content)));
        var pathAndQuery = absoluteUri.PathAndQuery;
        var stringToSign = $"{request.Method.Method}\n{pathAndQuery}\n{timestamp};{host};{contentHash}";
        var signature = ComputeSignature(accessKey, stringToSign);

        request.Headers.Host = host;
        request.Headers.TryAddWithoutValidation("x-ms-date", timestamp);
        request.Headers.TryAddWithoutValidation("x-ms-content-sha256", contentHash);
        request.Headers.Authorization = new AuthenticationHeaderValue(
            "HMAC-SHA256",
            $"SignedHeaders=x-ms-date;host;x-ms-content-sha256&Signature={signature}");
    }

    private static string ComputeSignature(string accessKey, string stringToSign)
    {
        var key = Convert.FromBase64String(accessKey);
        using var hmac = new HMACSHA256(key);
        return Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(stringToSign)));
    }

    private static AzureEmailResponse DeserializeSuccess(string content, HttpResponseMessage response)
    {
        var id = ReadOperationId(response.Headers.Location?.ToString())
            ?? ReadFirstHeader(response, "Operation-Location");

        if (string.IsNullOrWhiteSpace(content))
        {
            return new AzureEmailResponse(id);
        }

        try
        {
            using var document = JsonDocument.Parse(content);
            var root = document.RootElement;
            id = ReadString(root, "id")
                ?? ReadString(root, "messageId")
                ?? ReadString(root, "operationId")
                ?? id;

            return new AzureEmailResponse(id);
        }
        catch (JsonException)
        {
            return new AzureEmailResponse(id);
        }
    }

    private static AzureError DeserializeError(string content, HttpStatusCode statusCode)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return new AzureError(
                statusCode.ToString(),
                $"Azure Communication Services returned HTTP {(int)statusCode}.");
        }

        try
        {
            using var document = JsonDocument.Parse(content);
            var root = document.RootElement;
            var source = root.TryGetProperty("error", out var nestedError) && nestedError.ValueKind == JsonValueKind.Object
                ? nestedError
                : root;

            var code = ReadString(source, "code")
                ?? ReadString(source, "name")
                ?? statusCode.ToString();
            var message = ReadString(source, "message")
                ?? $"Azure Communication Services returned HTTP {(int)statusCode}.";

            return new AzureError(code, message);
        }
        catch (JsonException)
        {
            return new AzureError(statusCode.ToString(), content);
        }
    }

    private static string? ReadOperationId(string? operationLocation)
    {
        if (string.IsNullOrWhiteSpace(operationLocation))
        {
            return null;
        }

        var lastSlash = operationLocation.LastIndexOf('/');
        return lastSlash >= 0 && lastSlash < operationLocation.Length - 1
            ? operationLocation[(lastSlash + 1)..]
            : operationLocation;
    }

    private static string? ReadFirstHeader(HttpResponseMessage response, string headerName)
    {
        return response.Headers.TryGetValues(headerName, out var values)
            ? ReadOperationId(values.FirstOrDefault())
            : null;
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

    private sealed record AzureError(string Code, string Message);

    private sealed record AzureSendEmailPayload(
        string SenderAddress,
        AzureSendEmailContent Content,
        AzureSendEmailRecipients Recipients,
        IReadOnlyCollection<AzureSendEmailAttachment>? Attachments,
        IReadOnlyDictionary<string, string>? Headers,
        IReadOnlyCollection<AzureSendEmailAddress>? ReplyTo)
    {
        public static AzureSendEmailPayload FromRequest(AzureEmailRequest request)
        {
            return new AzureSendEmailPayload(
                request.SenderAddress,
                new AzureSendEmailContent(
                    request.Subject,
                    request.TextBody,
                    request.HtmlBody),
                new AzureSendEmailRecipients(
                    FormatAddresses(request.To),
                    EmptyToNull(FormatAddresses(request.Cc)),
                    EmptyToNull(FormatAddresses(request.Bcc))),
                EmptyToNull(request.Attachments.Select(MapAttachment).ToArray()),
                EmptyToNull(request.Headers),
                EmptyToNull(FormatAddresses(request.ReplyTo)));
        }

        private static IReadOnlyCollection<AzureSendEmailAddress> FormatAddresses(
            IReadOnlyCollection<AzureEmailRecipient> recipients)
        {
            return recipients
                .Select(recipient => new AzureSendEmailAddress(recipient.Address, recipient.DisplayName))
                .ToArray();
        }

        private static AzureSendEmailAttachment MapAttachment(AzureEmailAttachment attachment)
        {
            return new AzureSendEmailAttachment(
                attachment.Name,
                attachment.ContentType,
                Convert.ToBase64String(attachment.Content.Span),
                attachment.ContentId,
                attachment.IsInline);
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

    private sealed record AzureSendEmailContent(
        string Subject,
        [property: JsonPropertyName("plainText")]
        string? PlainText,
        string? Html);

    private sealed record AzureSendEmailRecipients(
        IReadOnlyCollection<AzureSendEmailAddress> To,
        IReadOnlyCollection<AzureSendEmailAddress>? Cc,
        IReadOnlyCollection<AzureSendEmailAddress>? Bcc);

    private sealed record AzureSendEmailAddress(
        string Address,
        string? DisplayName);

    private sealed record AzureSendEmailAttachment(
        string Name,
        string ContentType,
        string ContentInBase64,
        string? ContentId,
        bool? IsInline);
}
