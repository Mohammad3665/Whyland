namespace Whyland.Domain.Abstractions;

/// <summary>
/// Marker interface for strongly-typed IDs.
/// Allows quick detection of a type as a strongly-typed ID without knowing its underlying value type.
/// </summary>
public interface IStronglyTypedId;

/// <summary>
/// Generic interface exposing the underlying value of a strongly-typed ID.
/// </summary>
/// <typeparam name="T">The underlying primitive type (e.g. int, Guid, long).</typeparam>
/// <remarks>
/// The <c>out</c> modifier (covariance) is applied to allow casting
/// <c>IStronglyTypedId&lt;Derived&gt;</c> to <c>IStronglyTypedId&lt;Base&gt;</c>.
/// Inheritance from the non-generic <see cref="IStronglyTypedId"/> enables
/// treating every strongly-typed ID uniformly through a shared type.
/// </remarks>
public interface IStronglyTypedId<out T> : IStronglyTypedId
{
    /// <summary>
    /// The raw underlying value of this ID.
    /// </summary>
    T Value { get; }
}