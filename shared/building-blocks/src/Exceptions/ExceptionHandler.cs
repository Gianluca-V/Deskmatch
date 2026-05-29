using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DeskMatch.BuildingBlocks.Exceptions;

public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }
    public NotFoundException(string name, object key) : base($"Entity \"{name}\" ({key}) was not found.") { }
}

public class ValidationException : Exception
{
    public IDictionary<string, string[]> Errors { get; }

    public ValidationException(IDictionary<string, string[]> errors)
        : base("One or more validation errors occurred.")
    {
        Errors = errors;
    }
}

public class UnauthorizedException : Exception
{
    public UnauthorizedException(string message = "Unauthorized access.") : base(message) { }
}

public class ForbiddenException : Exception
{
    public ForbiddenException(string message = "Forbidden access.") : base(message) { }
}

public class BadRequestException : Exception
{
    public BadRequestException(string message) : base(message) { }
}

public class ConflictException : Exception
{
    public ConflictException(string message) : base(message) { }
}

public static class ExceptionHandlerExtensions
{
    public static IApplicationBuilder UseBuildingBlockExceptionHandler(this IApplicationBuilder app)
    {
        return app.UseExceptionHandler(builder =>
        {
            builder.Run(async context =>
            {
                var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;
                var logger = context.RequestServices.GetRequiredService<ILogger<object>>();

                var (statusCode, problemDetails) = exception switch
                {
                    NotFoundException ex => (
                        (int)HttpStatusCode.NotFound,
                        new ProblemDetails
                        {
                            Title = "Not Found",
                            Detail = ex.Message,
                            Status = (int)HttpStatusCode.NotFound,
                            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4"
                        }),
                    ValidationException ex => (
                        (int)HttpStatusCode.BadRequest,
                        new ProblemDetails
                        {
                            Title = "Validation Error",
                            Detail = ex.Message,
                            Status = (int)HttpStatusCode.BadRequest,
                            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1"
                        }),
                    UnauthorizedException ex => (
                        (int)HttpStatusCode.Unauthorized,
                        new ProblemDetails
                        {
                            Title = "Unauthorized",
                            Detail = ex.Message,
                            Status = (int)HttpStatusCode.Unauthorized,
                            Type = "https://tools.ietf.org/html/rfc7235#section-3.1"
                        }),
                    ForbiddenException ex => (
                        (int)HttpStatusCode.Forbidden,
                        new ProblemDetails
                        {
                            Title = "Forbidden",
                            Detail = ex.Message,
                            Status = (int)HttpStatusCode.Forbidden,
                            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.3"
                        }),
                    BadRequestException ex => (
                        (int)HttpStatusCode.BadRequest,
                        new ProblemDetails
                        {
                            Title = "Bad Request",
                            Detail = ex.Message,
                            Status = (int)HttpStatusCode.BadRequest,
                            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1"
                        }),
                    ConflictException ex => (
                        (int)HttpStatusCode.Conflict,
                        new ProblemDetails
                        {
                            Title = "Conflict",
                            Detail = ex.Message,
                            Status = (int)HttpStatusCode.Conflict,
                            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.8"
                        }),
                    _ => (
                        (int)HttpStatusCode.InternalServerError,
                        new ProblemDetails
                        {
                            Title = "Internal Server Error",
                            Detail = IsDevelopment(context)
                                ? exception?.Message
                                : "An unexpected error occurred.",
                            Status = (int)HttpStatusCode.InternalServerError,
                            Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1"
                        })
                };

                if (exception is not null && exception is not ValidationException)
                {
                    logger.LogError(exception, "An unhandled exception occurred: {Message}", exception.Message);
                }

                if (exception is ValidationException validationException)
                {
                    problemDetails.Extensions["errors"] = validationException.Errors;
                }

                context.Response.StatusCode = statusCode;
                context.Response.ContentType = "application/problem+json";

                var jsonOptions = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                await context.Response.WriteAsync(JsonSerializer.Serialize(problemDetails, jsonOptions));
            });
        });
    }

    private static bool IsDevelopment(HttpContext context)
    {
        var env = context.RequestServices.GetRequiredService<IHostEnvironment>();
        return env.IsDevelopment();
    }
}