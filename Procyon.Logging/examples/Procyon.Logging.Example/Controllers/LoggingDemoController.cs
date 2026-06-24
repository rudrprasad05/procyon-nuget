using Microsoft.AspNetCore.Mvc;
using Procyon.Logging.Abstractions;

namespace Procyon.Logging.Example.Controllers;

[ApiController]
[Route("api/logging-demo")]
public sealed class LoggingDemoController : ControllerBase
{
    private readonly IProcyonLogger _logger;

    public LoggingDemoController(IProcyonLogger logger)
    {
        _logger = logger;
    }

    [HttpGet("ping")]
    public IActionResult Ping([FromQuery] string source = "browser")
    {
        _logger.Info("Ping endpoint called", new
        {
            source,
            remoteIp = HttpContext.Connection.RemoteIpAddress?.ToString()
        });

        return Ok(new
        {
            message = "pong",
            source,
            traceId = HttpContext.TraceIdentifier,
            logUi = "/procyon/logs"
        });
    }

    [HttpPost("orders")]
    public IActionResult CreateOrder(CreateOrderRequest request)
    {
        _logger.Info("Order created", new
        {
            request.CustomerId,
            request.Sku,
            request.Quantity,
            request.Expedite
        });

        if (request.Expedite)
        {
            _logger.Warning("Expedited order requested", new
            {
                request.CustomerId,
                request.Sku
            });
        }

        return Created(
            $"/api/logging-demo/orders/{Guid.NewGuid():N}",
            new
            {
                id = Guid.NewGuid(),
                status = request.Expedite ? "expedited" : "queued",
                request.CustomerId,
                request.Sku,
                request.Quantity
            });
    }

    [HttpPost("levels")]
    public IActionResult WriteEveryLevel()
    {
        _logger.Trace("Trace level example", new { area = "demo" });
        _logger.Debug("Debug level example", new { area = "demo" });
        _logger.Info("Information level example", new { area = "demo" });
        _logger.Warning("Warning level example", new { area = "demo" });
        _logger.Error("Error level example without exception", new { area = "demo" });
        _logger.Critical("Critical level example without exception", new { area = "demo" });

        return Ok(new
        {
            message = "Level logs were queued. With the sample config, Trace and Debug are filtered by MinimumLevel=Information."
        });
    }

    [HttpPost("exception")]
    public IActionResult LogException()
    {
        try
        {
            ThrowDemoException();
            return Ok();
        }
        catch (InvalidOperationException exception)
        {
            _logger.Error(exception, "Demo exception captured", new
            {
                operation = "LogException",
                traceId = HttpContext.TraceIdentifier
            });

            return Problem(
                title: "Demo exception logged",
                detail: "This endpoint intentionally logs an exception and returns a 500 response.",
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    [NoLog]
    [HttpGet("quiet")]
    public IActionResult Quiet()
    {
        _logger.Info("Quiet endpoint custom log still works", new
        {
            note = "The [NoLog] attribute skips automatic API request logging only."
        });

        return Ok(new
        {
            message = "Automatic request logging is skipped for this endpoint."
        });
    }

    private static void ThrowDemoException()
        => throw new InvalidOperationException("This is a Procyon.Logging example exception.");
}

public sealed record CreateOrderRequest(
    string CustomerId,
    string Sku,
    int Quantity,
    bool Expedite);
