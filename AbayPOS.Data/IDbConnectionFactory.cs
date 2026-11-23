using System.Data.Common;

namespace AbayPOS.Data;

/// <summary>Creates database connections. Swap <c>SqliteConnectionFactory</c> for a MySQL factory later.</summary>
public interface IDbConnectionFactory
{
    /// <summary>Returns a closed connection. Callers open it and dispose it.</summary>
    DbConnection Create();
}
