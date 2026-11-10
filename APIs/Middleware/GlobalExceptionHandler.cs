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
        var (statusCode, title) = exception switch
        {
            ConflictException => (StatusCodes.Status409Conflict, "Xung đột dữ liệu"),
            NotFoundException => (StatusCodes.Status404NotFound, "Không tìm thấy"),
            ArgumentException => (StatusCodes.Status400BadRequest, "Dữ liệu không hợp lệ"),
            DbUpdateException => (StatusCodes.Status409Conflict, "Không thể lưu dữ liệu"),
            _ => (StatusCodes.Status500InternalServerError, "Lỗi hệ thống")
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

        await Results.Problem(
                title: title,
                detail: exception.Message,
                statusCode: statusCode,
                instance: httpContext.Request.Path)
            .ExecuteAsync(httpContext);

        return true;
    }
}
