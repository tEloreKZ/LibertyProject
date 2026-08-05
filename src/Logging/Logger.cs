using ProjectLiberty.Abstractions.Logging;

namespace ProjectLiberty.Logging;

/// <summary>
/// Logger implementation.
/// </summary>
public class Logger : ILogger, IDisposable
{
    private readonly string _categoryName;
    private readonly string _logDirectory;
    private readonly bool _logToFile;
    private readonly bool _logToConsole;
    private readonly int _maxLogFiles;
    private readonly int _maxLogFileSizeMB;
    private StreamWriter? _fileWriter;
    private string? _currentLogFile;
    private LogLevel _minimumLevel = LogLevel.Information;
    private bool _disposed;

    public Logger(string categoryName, string logDirectory, bool logToFile = true, bool logToConsole = true, int maxLogFiles = 10, int maxLogFileSizeMB = 10)
    {
        _categoryName = categoryName;
        _logDirectory = logDirectory;
        _logToFile = logToFile;
        _logToConsole = logToConsole;
        _maxLogFiles = maxLogFiles;
        _maxLogFileSizeMB = maxLogFileSizeMB;

        if (_logToFile)
        {
            InitializeFileLogging();
        }
    }

    public LogLevel MinimumLevel
    {
        get => _minimumLevel;
        set => _minimumLevel = value;
    }

    public void LogDebug(string message)
    {
        Log(LogLevel.Debug, message);
    }

    public void LogInformation(string message)
    {
        Log(LogLevel.Information, message);
    }

    public void LogWarning(string message)
    {
        Log(LogLevel.Warning, message);
    }

    public void LogError(string message)
    {
        Log(LogLevel.Error, message);
    }

    public void LogError(Exception exception, string message)
    {
        Log(LogLevel.Error, $"{message}: {exception.Message}");
    }

    public void LogCritical(string message)
    {
        Log(LogLevel.Critical, message);
    }

    public void Log(LogLevel level, string message)
    {
        if (level < _minimumLevel)
        {
            return;
        }

        var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff");
        var logMessage = $"[{timestamp}] [{level}] [{_categoryName}] {message}";

        if (_logToConsole)
        {
            Console.WriteLine(logMessage);
        }

        if (_logToFile && _fileWriter != null)
        {
            try
            {
                _fileWriter.WriteLine(logMessage);
                _fileWriter.Flush();

                // Check file size
                if (_fileWriter.BaseStream.Length > _maxLogFileSizeMB * 1024 * 1024)
                {
                    RotateLogFile();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to write to log file: {ex.Message}");
            }
        }
    }

    private void InitializeFileLogging()
    {
        if (!Directory.Exists(_logDirectory))
        {
            Directory.CreateDirectory(_logDirectory);
        }

        CleanOldLogFiles();

        var fileName = $"log_{DateTime.UtcNow:yyyyMMdd_HHmmss}.log";
        _currentLogFile = Path.Combine(_logDirectory, fileName);
        _fileWriter = new StreamWriter(new FileStream(_currentLogFile, FileMode.Append, FileAccess.Write, FileShare.ReadWrite));
    }

    private void CleanOldLogFiles()
    {
        var logFiles = Directory.GetFiles(_logDirectory, "log_*.log")
            .OrderByDescending(f => File.GetCreationTime(f))
            .Skip(_maxLogFiles);

        foreach (var logFile in logFiles)
        {
            try
            {
                File.Delete(logFile);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to delete old log file: {ex.Message}");
            }
        }
    }

    private void RotateLogFile()
    {
        _fileWriter?.Dispose();
        InitializeFileLogging();
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _fileWriter?.Dispose();
        _disposed = true;
    }
}
