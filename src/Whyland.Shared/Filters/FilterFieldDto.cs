namespace Whyland.Shared.Filters;

/// <summary>
/// Describes one field a list page can be filtered or sorted on, so the picker is built
/// from the model instead of being hand-written per page. The property name is the same
/// one <see cref="FilterCondition"/> uses, which the generic filter resolver matches
/// case-insensitively against the entity.
/// </summary>
/// <param name="Property">Entity property name, e.g. "PhoneNumber".</param>
/// <param name="Label">Persian label shown in the picker.</param>
/// <param name="Kind">Input kind: string, bool, number, date, guid, enum, or one of the lists the
/// picker fills from the server (user: an account, role: a role).</param>
/// <param name="Operators">Operator names accepted for this field, matching <see cref="Whyland.Shared.Enums.FilterOperator"/>.</param>
/// <param name="Options">Fixed values for closed domains (bool/enum); empty when the value is free-form.</param>
/// <param name="Min">Smallest value the field can ever hold; the picker refuses anything below it. Null = unbounded.</param>
/// <param name="Max">Largest value the field can ever hold; the picker refuses anything above it. Null = unbounded.</param>
public sealed record FilterFieldDto(
    string Property,
    string Label,
    string Kind,
    IReadOnlyList<string> Operators,
    IReadOnlyList<string>? Options = null,
    int? Min = null,
    int? Max = null);
