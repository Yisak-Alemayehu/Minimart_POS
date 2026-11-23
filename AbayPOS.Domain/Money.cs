using System.Globalization;

namespace AbayPOS.Domain;

/// <summary>ETB money helpers. Domain uses <see cref="decimal"/>; persistence may store integer cents.</summary>
public static class Money
{
    private static readonly CultureInfo Et = CultureInfo.GetCultureInfo("en-ET");

    /// <summary>Converts ETB to integer cents using away-from-zero rounding.</summary>
    public static long ToCents(decimal amount) =>
        (long)decimal.Round(amount * 100m, 0, MidpointRounding.AwayFromZero);

    /// <summary>Converts integer cents to ETB.</summary>
    public static decimal FromCents(long cents) => cents / 100m;

    /// <summary>Formats an amount as <c>ETB 1,000.00</c> with tabular-friendly grouping.</summary>
    public static string Format(decimal amount) =>
        "ETB " + amount.ToString("N2", Et);

    /// <summary>Formats a negative discount as <c>- ETB 15.00</c>.</summary>
    public static string FormatDiscount(decimal amount) =>
        amount <= 0m ? Format(0m) : "- " + Format(amount);
}
