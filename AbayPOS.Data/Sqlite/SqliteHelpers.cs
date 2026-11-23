using System.Globalization;
using AbayPOS.Domain;
using Microsoft.Data.Sqlite;

namespace AbayPOS.Data.Sqlite;

internal static class SqliteHelpers
{
    public static DateTime ReadDate(SqliteDataReader reader, string column)
    {
        var ordinal = reader.GetOrdinal(column);
        var raw = reader.GetString(ordinal);
        return DateTime.Parse(raw, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
    }

    public static string WriteDate(DateTime value) =>
        value.ToString("o", CultureInfo.InvariantCulture);

    public static decimal ReadMoney(SqliteDataReader reader, string column) =>
        Money.FromCents(reader.GetInt64(reader.GetOrdinal(column)));

    public static bool ReadBool(SqliteDataReader reader, string column) =>
        reader.GetInt32(reader.GetOrdinal(column)) != 0;

    public static string? ReadNullableString(SqliteDataReader reader, string column)
    {
        var ordinal = reader.GetOrdinal(column);
        return reader.IsDBNull(ordinal) ? null : reader.GetString(ordinal);
    }

    public static int? ReadNullableInt(SqliteDataReader reader, string column)
    {
        var ordinal = reader.GetOrdinal(column);
        return reader.IsDBNull(ordinal) ? null : reader.GetInt32(ordinal);
    }
}
