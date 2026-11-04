using System.Data;

namespace Whyland.Infrastructure.Persistence.Conventions;

/// <summary>
/// The Dapper half of the strongly-typed-id convention that
/// <see cref="StronglyTypedIdConverter{TId,TValue}"/> is the EF Core half of: every identifier
/// struct discovered in the scanned assemblies gets a type handler that wraps the raw key coming
/// out of the reader and unwraps the value going into a parameter.
/// </summary>
public static class DapperStronglyTypedIdHandlers
{
    #region Registration

    /// <summary>
    /// Registers one handler per identifier type. Calling it again for the same type only replaces
    /// the handler, so it is safe to run once at composition time.
    /// </summary>
    /// <param name="assemblies">Assemblies to scan; normally the Domain assembly.</param>
    /// <remarks>
    /// Without this, a read that materializes an <em>entity</em> dies on its key column. The fluent
    /// builder describes an id column by the type the database stores — it unwraps the identifier so
    /// the column stays filterable and sortable — so the reader hands Dapper a <c>Guid</c> for a
    /// member typed <c>UserId</c>. Dapper then falls back to <c>Convert.ChangeType</c>, which throws
    /// "Object must implement IConvertible" because <c>System.Guid</c> does not implement it.
    /// </remarks>
    public static void Register(params Assembly[] assemblies)
    {
        foreach (var (idType, keyType) in Discover(assemblies))
            SqlMapper.AddTypeHandler(idType, new StronglyTypedIdTypeHandler(idType, keyType));
    }

    /// <summary>
    /// The identifier types and the raw key each one wraps, read off <see cref="IStronglyTypedId{T}"/>.
    /// </summary>
    /// <param name="assemblies">Assemblies to scan.</param>
    private static IEnumerable<(Type IdType, Type KeyType)> Discover(IEnumerable<Assembly> assemblies)
        => from assembly in assemblies
           from idType in assembly.GetTypes()
           where idType.IsValueType
           let idInterface = idType.GetInterfaces().FirstOrDefault(x =>
               x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IStronglyTypedId<>))
           where idInterface is not null
           select (idType, idInterface.GetGenericArguments()[0]);

    #endregion

    #region Handler

    /// <summary>
    /// Translates a reader value into an identifier struct and a parameter value back into the raw
    /// key, for one specific identifier type.
    /// </summary>
    /// <param name="idType">The identifier struct, e.g. <c>UserId</c>.</param>
    /// <param name="keyType">The primitive it wraps, e.g. <c>Guid</c>.</param>
    private sealed class StronglyTypedIdTypeHandler(Type idType, Type keyType) : SqlMapper.ITypeHandler
    {
        /// <summary>
        /// The single-key constructor, resolved once per type instead of per row. The same
        /// requirement the EF converter has: an identifier must be constructible from its raw key.
        /// </summary>
        private readonly ConstructorInfo _constructor =
            idType.GetConstructor([keyType])
            ?? throw new InvalidOperationException(
                $"Type '{idType.Name}' must have a constructor accepting '{keyType.Name}'.");

        /// <summary>Wraps the raw key the data reader returned.</summary>
        /// <param name="type">The member type Dapper is filling; always <c>idType</c> here.</param>
        /// <param name="value">The column value, never <see cref="DBNull"/>.</param>
        public object Parse(Type type, object value) => _constructor.Invoke([value]);

        /// <summary>Binds the inner key, so the driver only ever sees a type it knows.</summary>
        /// <param name="parameter">The parameter being filled.</param>
        /// <param name="value">The boxed identifier Dapper was handed.</param>
        public void SetValue(IDbDataParameter parameter, object value)
            => parameter.Value = StronglyTypedIds.ValueOf(value);
    }

    #endregion
}
