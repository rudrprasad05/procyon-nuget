using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using Procyon.Email.Abstractions;
using Procyon.Email.DependencyInjection;
using Procyon.Email.Resend.DependencyInjection;

namespace Procyon.Email.Resend.Tests;

public class ResendEmailProviderTests
{
    [Fact]
    public async Task SendAsync_PostsEmailToResend()
    {
        var environmentVariable = $"PROCYON_EMAIL_RESEND_TEST_{Guid.NewGuid():N}";
        Environment.SetEnvironmentVariable(environmentVariable, "re_test_key");
        var handler = new CapturingHandler(_ => JsonResponse(HttpStatusCode.OK, """{"id":"email_123"}"""));

        try
        {
            var sender = CreateSender(environmentVariable, handler);
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
                IdempotencyKey = "email-provider-test"
            };

            var result = await sender.SendAsync(message);

            Assert.True(result.Succeeded);
            Assert.Equal(EmailSendStatus.Accepted, result.Status);
            Assert.Equal("Resend", result.ProviderName);
            Assert.Equal("email_123", result.ProviderMessageId);
            Assert.Equal(HttpMethod.Post, handler.Method);
            Assert.Equal("https://api.resend.test/emails", handler.RequestUri?.ToString());
            Assert.Equal("Bearer", handler.AuthorizationScheme);
            Assert.Equal("re_test_key", handler.AuthorizationParameter);
            Assert.Equal("email-provider-test", handler.IdempotencyKey);

            using var document = JsonDocument.Parse(handler.Content);
            var root = document.RootElement;
            Assert.Equal("Procyon Test <no-reply@example.com>", root.GetProperty("from").GetString());
            Assert.Equal("User Example <user@example.com>", root.GetProperty("to")[0].GetString());
            Assert.Equal("copy@example.com", root.GetProperty("cc")[0].GetString());
            Assert.Equal("hidden@example.com", root.GetProperty("bcc")[0].GetString());
            Assert.Equal("Reply Team <reply@example.com>", root.GetProperty("reply_to")[0].GetString());
            Assert.Equal("Subject", root.GetProperty("subject").GetString());
            Assert.Equal("<p>Hello</p>", root.GetProperty("html").GetString());
            Assert.Equal("Hello", root.GetProperty("text").GetString());
            Assert.Equal("true", root.GetProperty("headers").GetProperty("X-Test").GetString());
            Assert.Equal("source", root.GetProperty("tags")[0].GetProperty("name").GetString());
            Assert.Equal("tests", root.GetProperty("tags")[0].GetProperty("value").GetString());
            Assert.Equal("summary.txt", root.GetProperty("attachments")[0].GetProperty("filename").GetString());
            Assert.Equal(
                Convert.ToBase64String(Encoding.UTF8.GetBytes("attached content")),
                root.GetProperty("attachments")[0].GetProperty("content").GetString());
        }
        finally
        {
            Environment.SetEnvironmentVariable(environmentVariable, null);
        }
    }

    [Fact]
    public async Task SendAsync_MapsResendErrors()
    {
        var environmentVariable = $"PROCYON_EMAIL_RESEND_TEST_{Guid.NewGuid():N}";
        Environment.SetEnvironmentVariable(environmentVariable, "re_test_key");
        var handler = new CapturingHandler(_ => JsonResponse(
            HttpStatusCode.Forbidden,
            """{"name":"validation_error","message":"Domain is not verified."}"""));

        try
        {
            var sender = CreateSender(environmentVariable, handler);
            var result = await sender.SendAsync(new EmailMessage
            {
                To = [new EmailAddress("user@example.com")],
                Subject = "Subject",
                HtmlBody = "<p>Hello</p>",
                IdempotencyKey = "email-provider-test"
            });

            Assert.False(result.Succeeded);
            Assert.Equal(EmailSendStatus.Rejected, result.Status);
            Assert.Equal("validation_error", result.ErrorCode);
            Assert.Equal("Domain is not verified.", result.ErrorMessage);
        }
        finally
        {
            Environment.SetEnvironmentVariable(environmentVariable, null);
        }
    }

    private static IEmailSender CreateSender(string environmentVariable, HttpMessageHandler handler)
    {
        var services = new ServiceCollection();
        services.AddSingleton<IHttpMessageHandlerBuilderFilter>(new TestHandlerFilter(handler));
        services
            .AddProcyonEmail(CreateConfiguration(environmentVariable))
            .UseResend();

        return services.BuildServiceProvider().GetRequiredService<IEmailSender>();
    }

    private static IConfiguration CreateConfiguration(string environmentVariable)
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Procyon:Email:Provider"] = "Resend",
                ["Procyon:Email:DefaultSender:Address"] = "no-reply@example.com",
                ["Procyon:Email:DefaultSender:Name"] = "Procyon Test",
                ["Procyon:Email:Retry:Enabled"] = "false",
                ["Procyon:Email:Resend:ApiBaseUrl"] = "https://api.resend.test",
                ["Procyon:Email:Resend:ApiKeyEnvironmentVariable"] = environmentVariable
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

        public string? IdempotencyKey { get; private set; }

        public string Content { get; private set; } = string.Empty;

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            Method = request.Method;
            RequestUri = request.RequestUri;
            AuthorizationScheme = request.Headers.Authorization?.Scheme;
            AuthorizationParameter = request.Headers.Authorization?.Parameter;
            IdempotencyKey = request.Headers.TryGetValues("Idempotency-Key", out var values)
                ? values.Single()
                : null;
            Content = request.Content is null
                ? string.Empty
                : await request.Content.ReadAsStringAsync(cancellationToken);

            return responseFactory(request);
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
