namespace GiaPha.Services.Logging;

/// <summary>
/// Interface logging theo mẫu PRN232 (Chapter 05).
/// Được đăng ký Singleton trong DI.
/// </summary>
public interface ILoggerManager
{
    void LogInfo(string message);

    void LogWarn(string message);

    void LogDebug(string message);

    void LogError(string message);
}
