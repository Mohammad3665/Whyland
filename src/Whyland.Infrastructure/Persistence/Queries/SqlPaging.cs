using System.Globalization;

namespace Whyland.Infrastructure.Persistence.Queries;

/// <summary>
/// Reads a page out of either request shape and turns it into a LIMIT/OFFSET pair, so the two
/// entry points of the query engine page the same way and one cap governs both.
/// Values arrive as text from a table widget, so anything unparsable falls back to the first
/// page of the default size instead of failing the request.
/// </summary>
public static class SqlPaging
{
    /// <summary>Hard cap, so a hand-made "length=999999" cannot pull a whole table.</summary>
    public const int MaxPageSize = 100;

    private const int DefaultPageSize = 10;

    /// <summary>The page taken from a data-table request: offset and length, with the cap applied.</summary>
    public static (int Take, int Skip) From(DataTableFilter filter)
    {
        ArgumentNullException.ThrowIfNull(filter);

        var take = int.TryParse(filter.Length, NumberStyles.Integer, CultureInfo.InvariantCulture, out var length)
                   && length > 0
            ? length
            : filter.PageSize > 0 ? filter.PageSize : DefaultPageSize;

        if (take > MaxPageSize) take = MaxPageSize;

        var skip = int.TryParse(filter.Start, NumberStyles.Integer, CultureInfo.InvariantCulture, out var start)
                   && start >= 0
            ? start
            : filter.Skip >= 0 ? filter.Skip : 0;

        return (take, skip);
    }

    /// <summary>The page taken from a <see cref="QueryRequest"/>: one-based page and size, with the cap applied.</summary>
    public static (int Take, int Skip) From(QueryRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var page = request.Page < 1 ? 1 : request.Page;
        var take = Math.Clamp(request.Take, 1, MaxPageSize);
        var offset = (page - 1) * (long)take;

        return (take, offset > int.MaxValue ? int.MaxValue : (int)offset);
    }
}
