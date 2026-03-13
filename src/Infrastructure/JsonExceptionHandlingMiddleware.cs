using System.Text.Json;

namespace CoreCodeCamp.Infrastructure;

/// <summary>
/// Middleware that catches and formats JSON deserialization errors clearly.
/// Returns a structured response that distinguishes deserialization errors from validation errors.
/// </summary>
public class JsonExceptionHandlingMiddleware(RequestDelegate next, ILogger<JsonExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (BadHttpRequestException ex) when (ex.InnerException is JsonException jsonEx)
        {
            logger.LogWarning(ex, "JSON deserialization error");
            
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            context.Response.ContentType = "application/problem+json";

            var propertyPath = ExtractPropertyName(jsonEx.Path);
            var response = new
            {
                type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                title = "Invalid JSON format",
                status = 400,
                detail = $"Failed to parse JSON. {jsonEx.Message}",
                errors = new Dictionary<string, string[]>
                {
                    { propertyPath, new[] { "Invalid format or value" } }
                }
            };

            await context.Response.WriteAsJsonAsync(response);
        }
    }

    private static string ExtractPropertyName(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return "_";

        // Path format: "$.eventDate" → extract "eventDate"
        var parts = path.Split('.');
        return parts.Length > 1 ? parts[^1] : path;
    }
}
