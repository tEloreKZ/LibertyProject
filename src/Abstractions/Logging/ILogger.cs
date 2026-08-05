namespace ProjectLiberty.Abstractions.Logging;

/// <summary>
/// Logger interface - provides logging functionality.
/// </summary>
public interface ILogger
{
    /// <summary>
    /// Gets the minimum log level.
    /// </summary>
    LogLevel MinimumLevel { get; set; }
    
    /// <summary>
    /// Logs a debug message.
    /// </summary>
    /// <param name="message">Message to log.</param>
    void LogDebug(string message);
    
    /// <summary>
    /// Logs an information message.
    /// </summary>
    /// <param name="message">Message to log.</param>
    void LogInformation(string message);
    
    /// <summary>
    /// Logs a warning message.
    /// </summary>
    /// <param name="message">Message to log.</param>
    void LogWarning(string message);
    
    /// <summary>
    /// Logs an error message.
    /// </summary>
    /// <param name="message">Message to log.</param>
    void LogError(string message);
    
    /// <summary>
    /// Logs an error message with exception.
    /// </summary>
    /// <param name="exception">Exception to log.</param>
    /// <param name="message">Message to log.</param>
    void LogError(Exception exception, string message);
    
    /// <summary>
    /// Logs a critical message.
    /// </summary>
    /// <param name="message">Message to log.</param>
    void LogCritical(string message);
    
    /// <summary>
    /// Logs a message at specified level.
    /// </summary>
    /// <param name="level">Log level.</param>
    /// <param name="message">Message to log.</param>
    void Log(LogLevel level, string message);
}

/// <summary>
/// Log level enum.
/// </summary>
public enum LogLevel
{
    Debug,
    Information,
    Warning,
    Error,
    Critical
}
