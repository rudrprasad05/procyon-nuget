using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using Procyon.Email.Abstractions;
using Procyon.Email.Azure.DependencyInjection;
using Procyon.Email.DependencyInjection;

namespace Procyon.Email.Azure.Tests;

public class AzureEmailProviderTests
{
    [Fact]
    public async Task SendAsync_PostsEmailToAzureCommunicationServices()
    {
        var handler = new CapturingHandler(_ => JsonResponse(
            HttpStatusCode.Accepted,
            """{"id":"operation_123"}"""));

        var sender = CreateSender(handler);
        var message = new EmailMessage
        {
            To = [new EmailAddress("user@example.com", "User Example")],
            Cc = [new EmailAddress("copy@example.com")],
            Bcc = [new EmailAddress("hidden@example.com")],
            ReplyTo = [new EmailAddress("reply@example.com", "Reply Team")],
            Subject = "Subject",
            HtmlBody = "<p>Hello</p>",
            TextBody = "Hello",
            Attachments =
            [
                new EmailAttachment(
                    "summary.txt",
                    "text/plain",
                    Encoding.UTF8.GetBytes("attached content"))
            ],
            Headers = new Dictionary<string, string> { ["X-Test"] = "true" },
            Tags = new Dictionary<string, string> { ["source"] = "tests" },
            IdempotencyKey = "00000000-0000-0000-0000-000000000001"
        };

        var result = await sender.SendAsync(message);

        Assert.True(result.Succeeded);
        Assert.Equal(EmailSendStatus.Accepted, result.Status);
        Assert.Equal("Azure", result.ProviderName);
        Assert.Equal("operation_123", result.ProviderMessageId);
        Assert.Equal(HttpMethod.Post, handler.Method);
        Assert.Equal("https://contoso.communication.azure.com/emails:send?api-version=2023-03-31", handler.RequestUri?.ToString());
        Assert.Equal("HMAC-SHA256", handler.AuthorizationScheme);
        Assert.Contains("Signature=", handler.AuthorizationParameter, StringComparison.Ordinal);
        Assert.Equal("00000000-0000-0000-0000-000000000001", handler.OperationId);
        Assert.Equal("00000000-0000-0000-0000-000000000001", handler.ClientRequestId);
        Assert.False(string.IsNullOrWhiteSpace(handler.MsDate));
        Assert.False(string.IsNullOrWhiteSpace(handler.ContentHash));

        using var document = JsonDocument.Parse(handler.Content);
        var root = document.RootElement;
        Assert.Equal("no-reply@example.com", root.GetProperty("senderAddress").GetString());
        Assert.Equal("Subject", root.GetProperty("content").GetProperty("subject").GetString());
        Assert.Equal("Hello", root.GetProperty("content").GetProperty("plainText").GetString());
        Assert.Equal("<p>Hello</p>", root.GetProperty("content").GetProperty("html").GetString());
        Assert.Equal("user@example.com", root.GetProperty("recipients").GetProperty("to")[0].GetProperty("address").GetString());
        Assert.Equal("User Example", root.GetProperty("recipients").GetProperty("to")[0].GetProperty("displayName").GetString());
        Assert.Equal("copy@example.com", root.GetProperty("recipients").GetProperty("cc")[0].GetProperty("address").GetString());
        Assert.Equal("hidden@example.com", root.GetProperty("recipients").GetProperty("bcc")[0].GetProperty("address").GetString());
        Assert.Equal("reply@example.com", root.GetProperty("replyTo")[0].GetProperty("address").GetString());
        Assert.Equal("true", root.GetProperty("headers").GetProperty("X-Test").GetString());
        Assert.Equal("tests", root.GetProperty("headers").GetProperty("X-Procyon-Tag-source").GetString());
        Assert.Equal("summary.txt", root.GetProperty("attachments")[0].GetProperty("name").GetString());
        Assert.Equal("text/plain", root.GetProperty("attachments")[0].GetProperty("contentType").GetString());
        Assert.Equal(
            Convert.ToBase64String(Encoding.UTF8.GetBytes("attached content")),
            root.GetProperty("attachments")[0].GetProperty("contentInBase64").GetString());
    }

    [Fact]
    public async Task SendAsync_MapsAzureErrors()
    {
        var handler = new CapturingHandler(_ => JsonResponse(
            HttpStatusCode.Forbidden,
            """{"error":{"code":"Denied","message":"Domain is not verified."}}"""));

        var sender = CreateSender(handler);
        var result = await sender.SendAsync(new EmailMessage
        {
            To = [new EmailAddress("user@example.com")],
            Subject = "Subject",
            HtmlBody = "<p>Hello</p>"
        });

        Assert.False(result.Succeeded);
        Assert.Equal(EmailSendStatus.Rejected, result.Status);
        Assert.Equal("Denied", result.ErrorCode);
        Assert.Equal("Domain is not verified.", result.ErrorMessage);
    }

    private static IEmailSender CreateSender(HttpMessageHandler handler)
    {
        var services = new ServiceCollection();
        services.AddSingleton<IHttpMessageHandlerBuilderFilter>(new TestHandlerFilter(handler));
        services
            .AddProcyonEmail(CreateConfiguration())
            .UseAzure();

        return services.BuildServiceProvider().GetRequiredService<IEmailSender>();
    }

    private static IConfiguration CreateConfiguration()
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Procyon:Email:Provider"] = "Azure",
                ["Procyon:Email:DefaultSender:Address"] = "no-reply@example.com",
                ["Procyon:Email:DefaultSender:Name"] = "Procyon Test",
                ["Procyon:Email:Retry:Enabled"] = "false",
                ["Procyon:Email:Azure:ConnectionString"] = "Endpoint=https://contoso.communication.azure.com/;AccessKey=dGVzdC1rZXk=",
                ["Procyon:Email:Azure:SenderEmail"] = "no-reply@example.com"
            })
            .Build();
    }

    private static HttpResponseMessage JsonResponse(HttpStatusCode statusCode, string content)
    {
        return new HttpResponseMessage(statusCode)
        {
            Content = new StringContent(content, Encoding.UTF8, "application/json")
        };
    }

    private sealed class CapturingHandler(Func<HttpRequestMessage, HttpResponseMessage> responseFactory) : HttpMessageHandler
    {
        public HttpMethod? Method { get; private set; }

        public Uri? RequestUri { get; private set; }

        public string? AuthorizationScheme { get; private set; }

        public string? AuthorizationParameter { get; private set; }

        public string? OperationId { get; private set; }

        public string? ClientRequestId { get; private set; }

        public string? MsDate { get; private set; }

        public string? ContentHash { get; private set; }

        public string Content { get; private set; } = string.Empty;

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            Method = request.Method;
            RequestUri = request.RequestUri;
            AuthorizationScheme = request.Headers.Authorization?.Scheme;
            AuthorizationParameter = request.Headers.Authorization?.Parameter;
            OperationId = ReadHeader(request, "Operation-Id");
            ClientRequestId = ReadHeader(request, "x-ms-client-request-id");
            MsDate = ReadHeader(request, "x-ms-date");
            ContentHash = ReadHeader(request, "x-ms-content-sha256");
            Content = request.Content is null
                ? string.Empty
                : await request.Content.ReadAsStringAsync(cancellationToken);

            return responseFactory(request);
        }

        private static string? ReadHeader(HttpRequestMessage request, string name)
        {
            return request.Headers.TryGetValues(name, out var values)
                ? values.Single()
                : null;
        }
    }

    private sealed class TestHandlerFilter(HttpMessageHandler handler) : IHttpMessageHandlerBuilderFilter
    {
        public Action<HttpMessageHandlerBuilder> Configure(Action<HttpMessageHandlerBuilder> next)
        {
            return builder =>
            {
                next(builder);
                builder.PrimaryHandler = handler;
            };
        }
    }
}
