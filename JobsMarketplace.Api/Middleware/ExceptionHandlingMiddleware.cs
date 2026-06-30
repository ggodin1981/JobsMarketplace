using System.Text.Json;
using JobsMarketplace.Api.Exceptions;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace JobsMarketplace.Api.Middleware;

public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (ApiException ex)
        {
            await WriteErrorAsync(context, ex.StatusCode, ex.Message);
        }
        catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
        {
            logger.LogWarning(ex, "A database conflict occurred while processing {Method} {Path}.", context.Request.Method, context.Request.Path);
            await WriteErrorAsync(context, StatusCodes.Status409Conflict, "The request conflicts with existing data.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unhandled exception occurred while processing {Method} {Path}.", context.Request.Method, context.Request.Path);
            await WriteErrorAsync(context, StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
        }
    }

    private static bool IsUniqueConstraintViolation(DbUpdateException exception)
    {
        return exception.InnerException is PostgresException postgresException
            && postgresException.SqlState == "23505";
    }

    private static Task WriteErrorAsync(HttpContext context, int statusCode, string message)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        return context.Response.WriteAsync(JsonSerializer.Serialize(new
        {
            error = message,
            traceId = context.TraceIdentifier
        }));
    }
}
