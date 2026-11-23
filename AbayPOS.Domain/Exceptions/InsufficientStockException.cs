namespace AbayPOS.Domain.Exceptions;

/// <summary>Thrown when a checkout line would take stock below zero.</summary>
public sealed class InsufficientStockException : Exception
{
    public InsufficientStockException(string productName, int available)
        : base($"Not enough stock: only {available} {productName} left.")
    {
        ProductName = productName;
        Available = available;
    }

    public string ProductName { get; }
    public int Available { get; }
}
