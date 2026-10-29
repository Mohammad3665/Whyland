namespace Whyland.Infrastructure.Persistence.Queries;

/// <summary>Comparison a filter can apply to a single column value.</summary>
public enum SqlComparisonType
{
    Equal,
    NotEqual,
    GreaterThan,
    GreaterThanOrEqual,
    LessThan,
    LessThanOrEqual,
}
