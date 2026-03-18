using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System;

namespace CoreCodeCamp.Infrastructure;

/// <summary>
/// Development-only middleware that randomly injects latency or 5xx faults to help exercise resilience.
/// Enable with configuration: "Chaos:Enabled" = true and tune probabilities in the "Chaos" section.
/// </summary>
public class ChaosMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ChaosMiddleware> _logger;
    private readonly Random _rnd = new();

    public ChaosMiddleware(RequestDelegate next, ILogger<ChaosMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IConfiguration config)
    {
        var enabled = config.GetValue<bool>("Chaos:Enabled");
        if (!enabled)
        {
            await _next(context);
            return;
        }

        // Read probabilities and latencies
        var latencyMs = config.GetValue<int>("Chaos:LatencyMs", 0);
        var latencyProbability = config.GetValue<double>("Chaos:LatencyProbability", 0.0);
        var errorProbability = config.GetValue<double>("Chaos:ErrorProbability", 0.0);

        // Inject latency
        if (latencyMs > 0 && _rnd.NextDouble() < latencyProbability)
        {
            _logger.LogWarning("ChaosMiddleware injecting {Latency}ms delay", latencyMs);
            await Task.Delay(latencyMs);
        }

        // Inject 5xx error
        if (_rnd.NextDouble() < errorProbability)
        {
            _logger.LogWarning("ChaosMiddleware returning 503 Service Unavailable");
            context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
            context.Response.ContentType = "application/problem+json";
            var problem = new { title = "Chaos - injected failure", status = 503 };
            await context.Response.WriteAsJsonAsync(problem);
            return;
        }

        await _next(context);
    }
}
