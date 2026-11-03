namespace Whyland.Domain.Abstractions;

/// <summary>
/// Reads an identifier from the outside. The Dapper data-table engine and the filter-field
/// pickers both describe columns by their CLR type and bind values by their CLR type, and neither
/// of them can know about <c>UserId</c> or <c>CategoryId</c> by name — a wrapped key would look
/// like an unknown struct, silently drop out of the whitelist, and stop being filterable.
/// <para>
/// Unwrapping is the one translation each side needs: the column type is the type the database
/// actually stores, and the value sent to the driver has to be that same raw type.
/// </para>
/// </summary>
public static class StronglyTypedIds
{
    #region Types

    /// <summary>
    /// The type a column is stored as: the wrapped type itself when it is not an identifier, and
    /// the raw key type when it is (a nullable wrapper is unwrapped first).
    /// </summary>
    /// <param name="type">The CLR type of a property or a field.</param>
    public static Type Unwrap(Type type)
    {
        var bare = Nullable.GetUnderlyingType(type) ?? type;

        return RawTypeOf(bare) ?? bare;
    }

    #endregion

    #region Values

    /// <summary>
    /// The value a database driver can send for an identifier-typed column: the inner key of a
    /// wrapped value, anything else unchanged.
    /// </summary>
    /// <param name="value">A bound value, possibly a boxed identifier.</param>
    public static object? ValueOf(object? value)
    {
        if (value is not IStronglyTypedId || RawTypeOf(value.GetType()) is null)
            return value;

        // The marker interface carries no key, so the getter of the struct's own Value property
        // is what returns the raw number or guid.
        return value.GetType().GetProperty("Value")!.GetValue(value);
    }

    #endregion

    #region Internals

    /// <summary>
    /// The key type an identifier wraps, or null when the type is not one. Read off the
    /// non-generic marker and then the generic interface, so the caller never has to name a
    /// specific identifier.
    /// </summary>
    /// <param name="type">A type with nullability already removed.</param>
    private static Type? RawTypeOf(Type type)
        => type is { IsValueType: true } && typeof(IStronglyTypedId).IsAssignableFrom(type)
            ? type.GetInterfaces()
                  .FirstOrDefault(x => x.IsGenericType &&
                                       x.GetGenericTypeDefinition() == typeof(IStronglyTypedId<>))
                  ?.GenericTypeArguments[0]
            : null;

    #endregion
}
