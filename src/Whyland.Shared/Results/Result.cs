using System.Diagnostics.CodeAnalysis;

namespace Whyland.Shared.Results;

/// <summary>
/// Represents the outcome of an operation that either succeeds or fails, without a return value.
/// Provides an implicit conversion from <see cref="Errors.Error"/> to allow direct error propagation.
/// </summary>
public class Result : BaseResult
{
    private Result(bool isSuccess, Errors.Error error)
        : base(isSuccess, error) { }

    /// <summary>
    /// Creates a successful result indicating that the operation completed without errors.
    /// </summary>
    public static Result Success() => new(true, Errors.Error.None);

    /// <summary>
    /// Creates a failure result with the specified error.
    /// </summary>
    /// <param name="error">The error describing the failure. Must not be <see cref="Errors.Error.None"/>.</param>
    public static Result Failure(Errors.Error error) => new(false, error);

    /// <summary>
    /// Enables implicit conversion of an <see cref="Errors.Error"/> to a failure result,
    /// allowing a method to return an error directly where a <see cref="Result"/> is expected.
    /// </summary>
    public static implicit operator Result(Errors.Error error) => Failure(error);
}

/// <summary>
/// Represents the outcome of an operation that either succeeds and returns data,
/// or fails with an error. Implicit conversions from both the success data and an error are provided.
/// </summary>
/// <typeparam name="TData">The type of data returned on success.</typeparam>
public class Result<TData> : BaseResult
{
    private Result(TData? data, bool isSuccess, Errors.Error error) : base(isSuccess, error)
    {
        Data = data;
    }

    /// <summary>
    /// Gets the data produced by a successful operation.
    /// When <see cref="IsSuccess"/> is <c>true</c>, this value is guaranteed non-null (enforced by
    /// <see cref="System.Diagnostics.CodeAnalysis.MemberNotNullWhenAttribute"/>).
    /// </summary>
    public TData? Data { get; }

    /// <summary>
    /// Indicates whether the operation succeeded. Delegates to the base property so the
    /// constructor's state (not a shadowing backing field) drives the value, while the
    /// <see cref="MemberNotNullWhenAttribute"/> makes <see cref="Data"/> non-null after a <c>true</c> check.
    /// </summary>
    [MemberNotNullWhen(true, nameof(Data))]
    public override bool IsSuccess => base.IsSuccess;

    /// <summary>
    /// Creates a successful result containing the provided data.
    /// </summary>
    /// <param name="data">The data to return with the success result.</param>
    public static Result<TData> Success(TData data) => new(data, true, Errors.Error.None);

    /// <summary>
    /// Creates a failure result with the specified error.
    /// </summary>
    /// <param name="error">The error describing the failure. Must not be <see cref="Errors.Error.None"/>.</param>
    public static Result<TData> Failure(Errors.Error error) => new(default, false, error);

    /// <summary>
    /// Enables implicit conversion of a data value to a successful result,
    /// allowing a method to return a value directly where a <see cref="Result{TData}"/> is expected.
    /// </summary>
    public static implicit operator Result<TData>(TData data) => Success(data);

    /// <summary>
    /// Enables implicit conversion of an <see cref="Errors.Error"/> to a failure result,
    /// allowing a method to return an error directly where a <see cref="Result{TData}"/> is expected.
    /// </summary>
    public static implicit operator Result<TData>(Errors.Error error) => Failure(error);
}
