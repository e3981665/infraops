using System.Net;
using System.Text.Json;
using InfraOps.Api.Contracts.Responses;
using FluentValidation;
using InfraOps.Application.Common.Exceptions;
using InfraOps.Domain.Common.Exceptions;

namespace InfraOps.Api.Middleware;

public sealed class ExceptionHandlingMiddleware
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new(JsonSerializerDefaults.Web);
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ValidationException exception)
        {
            var validationErrors = exception.Errors
                .GroupBy(x => x.PropertyName, StringComparer.Ordinal)
                .ToDictionary(
                    group => group.Key,
                    group => group.Select(x => x.ErrorMessage).Distinct().ToArray(),
                    StringComparer.Ordinal);

            await WriteErrorResponseAsync(
                context,
                HttpStatusCode.BadRequest,
                CreateErrorResponse(
                    context,
                    "validation_error",
                    "One or more validation errors occurred.",
                    validationErrors));
        }
        catch (ApplicationUnauthorizedException exception)
        {
            await WriteErrorResponseAsync(
                context,
                HttpStatusCode.Unauthorized,
                CreateErrorResponse(context, "unauthorized", exception.Message));
        }
        catch (ApplicationNotFoundException exception)
        {
            await WriteErrorResponseAsync(
                context,
                HttpStatusCode.NotFound,
                CreateErrorResponse(context, "not_found", exception.Message));
        }
        catch (DomainRuleException exception)
        {
            await WriteErrorResponseAsync(
                context,
                HttpStatusCode.BadRequest,
                CreateErrorResponse(context, "domain_rule_violation", exception.Message));
        }
        catch (Exception exception)
        {
            var correlationId = CorrelationIdMiddleware.GetCorrelationId(context);

            _logger.LogError(
                exception,
                "Unhandled exception while processing {Path} with correlation {CorrelationId}.",
                context.Request.Path.Value,
                correlationId);

            await WriteErrorResponseAsync(
                context,
                HttpStatusCode.InternalServerError,
                CreateErrorResponse(context, "server_error", "An unexpected error occurred."));
        }
    }

    private static ErrorResponse CreateErrorResponse(
        HttpContext context,
        string code,
        string message,
        IReadOnlyDictionary<string, string[]>? errors = null)
    {
        return new ErrorResponse(
            code,
            message,
            errors,
            CorrelationIdMiddleware.GetCorrelationId(context));
    }

    private static async Task WriteErrorResponseAsync(
        HttpContext context,
        HttpStatusCode statusCode,
        ErrorResponse response)
    {
        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json";

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, JsonSerializerOptions));
    }
}
