namespace Whyland.Domain.Common.Stores;

/// <summary>
/// Central store of static runtime identifiers used across the application,
/// so configuration keys are never duplicated (or re-hard-coded) in each layer.
/// </summary>
public static class StaticDataStore
{
    #region Connection

    /// <summary>
    /// Name of the PostgreSQL connection string entry in <c>ConnectionStrings</c>.
    /// The actual connection string value must always come from configuration (s2.9);
    /// only the key name is static.
    /// </summary>
    public const string PostgresConnectionStringName = "Postgres";

    #endregion
}
