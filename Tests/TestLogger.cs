using ProjectLiberty.Abstractions.Logging;

namespace ProjectLiberty.Tests;

internal sealed class TestLogger : ILogger
{
    public LogLevel MinimumLevel { get; set; }
    public List<string> Messages { get; } = new List<string>();
    public void LogDebug(string message) => Messages.Add(message);
    public void LogInformation(string message) => Messages.Add(message);
    public void LogWarning(string message) => Messages.Add(message);
    public void LogError(string message) => Messages.Add(message);
    public void LogError(Exception exception, string message) => Messages.Add(message + ": " + exception.Message);
    public void LogCritical(string message) => Messages.Add(message);
    public void Log(LogLevel level, string message) => Messages.Add(message);
}
