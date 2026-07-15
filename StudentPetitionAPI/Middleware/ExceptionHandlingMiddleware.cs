using System.Text.Json;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentPetitionAPI.Application.Exceptions;

namespace StudentPetitionAPI.Middleware;

public class ExceptionHandlingMiddleware
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            await HandleExceptionAsync(context, exception);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var problemDetails = CreateProblemDetails(context, exception);

        if (problemDetails.Status >= StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(exception, "Unhandled exception for {Method} {Path}",
                context.Request.Method, context.Request.Path);
        }
        else
        {
            _logger.LogWarning(exception, "Handled exception ({StatusCode}) for {Method} {Path}: {Message}",
                problemDetails.Status, context.Request.Method, context.Request.Path, exception.Message);
        }

        if (context.Response.HasStarted)
        {
            _logger.LogWarning("Response already started; cannot write problem details.");
            return;
        }

        context.Response.Clear();
        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = problemDetails.Status ?? StatusCodes.Status500InternalServerError;

        await context.Response.WriteAsJsonAsync(
            problemDetails,
            JsonOptions,
            contentType: "application/problem+json",
            cancellationToken: context.RequestAborted);
    }

    private ProblemDetails CreateProblemDetails(HttpContext context, Exception exception)
    {
        exception = Unwrap(exception);

        var (statusCode, title, detail) = exception switch
        {
            NotFoundException => (StatusCodes.Status404NotFound, "Resource not found", exception.Message),
            ValidationException => (StatusCodes.Status400BadRequest, "Validation failed", exception.Message),
            ConflictException => (StatusCodes.Status409Conflict, "Conflict", exception.Message),
            ForbiddenException => (StatusCodes.Status403Forbidden, "Forbidden", exception.Message),
            UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "Unauthorized", exception.Message),
            InvalidStatusTransitionException => (StatusCodes.Status400BadRequest, "Invalid status transition", exception.Message),
            BusinessRuleException => (StatusCodes.Status400BadRequest, "Business rule violation", exception.Message),
            DbUpdateException dbUpdate when IsUniqueConstraintViolation(dbUpdate) =>
                (StatusCodes.Status409Conflict, "Conflict", "A unique constraint was violated."),
            _ => (
                StatusCodes.Status500InternalServerError,
                "An unexpected error occurred",
                _environment.IsDevelopment()
                    ? exception.Message
                    : "An unexpected error occurred. Please try again later.")
        };

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = context.Request.Path
        };

        problemDetails.Extensions["traceId"] = context.TraceIdentifier;

        if (exception is ValidationException validationException)
        {
            problemDetails.Extensions["errors"] = validationException.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage).ToArray());
        }

        if (exception is InvalidStatusTransitionException transitionException)
        {
            problemDetails.Extensions["currentStatus"] = transitionException.CurrentStatus.ToString();
            problemDetails.Extensions["action"] = transitionException.Action;

            if (transitionException.AttemptedStatus.HasValue)
            {
                problemDetails.Extensions["attemptedStatus"] =
                    transitionException.AttemptedStatus.Value.ToString();
            }
        }

        return problemDetails;
    }

    private static Exception Unwrap(Exception exception) =>
        exception is AggregateException { InnerException: { } inner }
            ? Unwrap(inner)
            : exception;

    private static bool IsUniqueConstraintViolation(DbUpdateException exception)
    {
        var message = exception.InnerException?.Message ?? exception.Message;
        return message.Contains("UNIQUE", StringComparison.OrdinalIgnoreCase)
               || message.Contains("unique constraint", StringComparison.OrdinalIgnoreCase);
    }
}
