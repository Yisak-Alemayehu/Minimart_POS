namespace AbayPOS.Domain.Exceptions;

/// <summary>Thrown when a use-case input fails a business rule.</summary>
public sealed class DomainValidationException : Exception
{
    public DomainValidationException(string message, string? field = null)
        : base(message)
    {
        Field = field;
    }

    public string? Field { get; }
}
