namespace AbayPOS.Domain.Exceptions;

/// <summary>Thrown when sign-in fails. Message is safe to show in the UI.</summary>
public sealed class LoginFailedException : Exception
{
    public LoginFailedException(string message, bool isLocked = false, string? field = "Password")
        : base(message)
    {
        IsLocked = isLocked;
        Field = field;
    }

    public bool IsLocked { get; }
    public string? Field { get; }
}
