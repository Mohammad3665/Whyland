namespace Whyland.Shared.Filters;

/// <summary>
/// Represents the response structure for DataTables server-side processing.
/// Contains paginated data along with metadata about total records and filtering.
/// </summary>
/// <typeparam name="TEntity">The type of entities being returned.</typeparam>
public class DataTableResult<TEntity>
{
    /// <summary>
    /// Draw counter - must match the draw value sent in the request.
    /// </summary>
    [JsonPropertyName("draw")]
    public string? Draw { get; set; }

    /// <summary>
    /// Total number of records in the dataset before filtering.
    /// </summary>
    [JsonPropertyName("recordsTotal")]
    public int RecordsTotal { get; set; }

    /// <summary>
    /// Total number of records after applying filters.
    /// </summary>
    [JsonPropertyName("recordsFiltered")]
    public int RecordsFiltered { get; set; }

    /// <summary>
    /// The actual data to display on the current page.
    /// </summary>
    [JsonPropertyName("data")]
    public IEnumerable<TEntity>? Data { get; set; }

    /// <summary>
    /// Optional error message if something went wrong during processing.
    /// </summary>
    [JsonPropertyName("error")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Error { get; set; }
}
