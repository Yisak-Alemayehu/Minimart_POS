using AbayPOS.Domain.Logging;

namespace AbayPOS.Business.Logging;

/// <summary>Appends timestamped lines to <c>logs/app.log</c>. Never throws into the UI.</summary>
public sealed class FileLogger : IAppLogger
{
    private readonly string _path;
    private readonly object _gate = new();

    public FileLogger(string? path = null)
    {
        _path = path ?? Path.Combine(AppContext.BaseDirectory, "logs", "app.log");
        var directory = Path.GetDirectoryName(_path);
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);
    }

    public void Info(string message) => Write("INFO", message, null);
    public void Warn(string message) => Write("WARN", message, null);
    public void Error(string message, Exception? exception = null) => Write("ERROR", message, exception);

    private void Write(string level, string message, Exception? exception)
    {
        try
        {
            var line = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} [{level}] {message}";
            if (exception is not null)
                line += Environment.NewLine + exception;
            lock (_gate)
                File.AppendAllText(_path, line + Environment.NewLine);
        }
        catch (Exception)
        {
            // Logging must never take down the register.
        }
    }
}
