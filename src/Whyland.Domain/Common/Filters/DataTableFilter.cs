namespace Whyland.Domain.Common.Filters;

/// <summary>
/// Represents filter parameters for DataTables server-side processing.
/// Contains pagination, sorting, and search information sent from the client.
/// </summary>
public class DataTableFilter
{
    /// <summary>
    /// Draw counter - used by DataTables to ensure response matches request.
    /// </summary>
    public string? Draw { get; set; }

    /// <summary>
    /// Number of records to display per page (from DataTables 'length' parameter).
    /// </summary>
    public string? Length { get; set; }

    /// <summary>
    /// Starting record index for pagination (from DataTables 'start' parameter).
    /// </summary>
    public string? Start { get; set; }

    /// <summary>
    /// Column name to sort by.
    /// </summary>
    public string? SortColumn { get; set; }

    /// <summary>
    /// Sort direction: "asc" or "desc".
    /// </summary>
    public string? SortColumnDirection { get; set; }

    /// <summary>
    /// Column index being sorted (0-based).
    /// </summary>
    public string? SortColumnIndex { get; set; }

    /// <summary>
    /// Field name for advanced search filtering.
    /// </summary>
    public string? SearchField { get; set; }

    /// <summary>
    /// Global search value entered by user.
    /// </summary>
    public string? SearchValue { get; set; }

    /// <summary>
    /// Calculated page size (converted from Length string to int).
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Calculated skip count (converted from Start string to int).
    /// </summary>
    public int Skip { get; set; }
}
