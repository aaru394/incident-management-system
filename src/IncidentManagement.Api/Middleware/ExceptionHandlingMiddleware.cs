using System.Net;
using IncidentManagement.Application.Common;
using IncidentManagement.Domain.Common;

namespace IncidentManagement.Api.Middleware;

public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            var (statusCode, title) = ex switch
            {
                NotFoundException => (HttpStatusCode.NotFound, "Resource not found"),
                ConflictException => (HttpStatusCode.Conflict, "Conflict"),
                UnauthorizedAppException => (HttpStatusCode.Unauthorized, "Unauthorized"),
                DomainException => (HttpStatusCode.BadRequest, "Business rule violation"),
                FluentValidation.ValidationException => (HttpStatusCode.BadRequest, "Validation failed"),
                _ => (HttpStatusCode.InternalServerError, "An unexpected error occurred")
            };

            if (statusCode == HttpStatusCode.InternalServerError)
            {
                logger.LogError(ex, "Unhandled exception processing {Method} {Path}", context.Request.Method, context.Request.Path);
            }
            else
            {
                logger.LogWarning("{ExceptionType}: {Message}", ex.GetType().Name, ex.Message);
            }

            context.Response.ContentType = "application/problem+json";
            context.Response.StatusCode = (int)statusCode;

            var problem = new
            {
                type = $"https://httpstatuses.io/{(int)statusCode}",
                title,
                status = (int)statusCode,
                detail = ex.Message,
                traceId = context.TraceIdentifier
            };

            await context.Response.WriteAsJsonAsync(problem);
        }
    }
}
