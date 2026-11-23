namespace AbayPOS.Domain.Logging;

/// <summary>Minimal logger so UI and services never write files directly with ad-hoc paths.</summary>
public interface IAppLogger
{
    void Info(string message);
    void Warn(string message);
    void Error(string message, Exception? exception = null);
}
