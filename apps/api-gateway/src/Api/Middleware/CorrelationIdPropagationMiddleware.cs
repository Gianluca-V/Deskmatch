using Microsoft.Extensions.Primitives;

namespace DeskMatch.ApiGateway.Api.Middleware;

public class CorrelationIdPropagationMiddleware
{
    private readonly RequestDelegate _next;
    private const string CorrelationIdHeader = "X-Correlation-Id";

    public CorrelationIdPropagationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue(CorrelationIdHeader, out var correlationId))
        {
            context.Request.Headers[CorrelationIdHeader] = correlationId;
        }
        else
        {
            var newCorrelationId = context.TraceIdentifier;
            context.Request.Headers[CorrelationIdHeader] = new StringValues(newCorrelationId);
        }

        context.Response.OnStarting(() =>
        {
            if (context.Request.Headers.TryGetValue(CorrelationIdHeader, out var responseCorrelationId))
            {
                context.Response.Headers[CorrelationIdHeader] = responseCorrelationId;
            }
            return Task.CompletedTask;
        });

        await _next(context);
    }
}