using GiaPha.BusinessObjects.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;

namespace GiaPha.APIs.Middleware;

/// <summary>
/// Gom lỗi nghiệp vụ về ProblemDetails thống nhất.
/// Controller không cần try/catch lặp lại.
/// </summary>
public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (statusCode, code, message) = exception switch
        {
            RequiresChangeRequestException ex => (
                StatusCodes.Status409Conflict,
                ex.Code,
                ex.Message),
            ConflictException ex => (
                StatusCodes.Status409Conflict,
                ApiErrorCodes.Conflict,
                ex.Message),
            NotFoundException ex => (
                StatusCodes.Status404NotFound,
                ApiErrorCodes.NotFound,
                ex.Message),
            ArgumentException ex => (
                StatusCodes.Status400BadRequest,
                ApiErrorCodes.Validation,
                ex.Message),
            DbUpdateException => (
                StatusCodes.Status409Conflict,
                ApiErrorCodes.DbUpdate,
                "Không thể lưu dữ liệu."),
            _ => (
                StatusCodes.Status500InternalServerError,
                ApiErrorCodes.Internal,
                "Lỗi hệ thống.")
        };

        if (statusCode == StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(exception, "Unhandled exception while processing {Path}", httpContext.Request.Path);
        }
        else
        {
            _logger.LogWarning(exception, "Business exception while processing {Path}", httpContext.Request.Path);
        }

        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = "application/json";

        await httpContext.Response.WriteAsJsonAsync(
            new { code, message },
            cancellationToken);

        return true;
    }
}
