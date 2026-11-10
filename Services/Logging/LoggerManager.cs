using Microsoft.Extensions.Logging;

namespace GiaPha.Services.Logging;

public sealed class LoggerManager : ILoggerManager
{
    private readonly ILogger<LoggerManager> _logger;

    public LoggerManager(ILogger<LoggerManager> logger)
    {
        _logger = logger;
    }

    public void LogInfo(string message) => _logger.LogInformation("{Message}", message);

    public void LogWarn(string message) => _logger.LogWarning("{Message}", message);

    public void LogDebug(string message) => _logger.LogDebug("{Message}", message);

    public void LogError(string message) => _logger.LogError("{Message}", message);
}
