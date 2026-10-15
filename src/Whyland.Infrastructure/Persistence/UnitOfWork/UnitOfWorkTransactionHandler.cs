using Whyland.Domain.Common.Results;

namespace Whyland.Infrastructure.Persistence.UnitOfWork;

/// <summary>
/// Implements <see cref="IUnitOfWorkTransactionHandler"/> by opening an EF Core transaction
/// around the executed work and committing on success or rolling back on failure or exception.
/// </summary>

public class UnitOfWorkTransactionHandler(IUnitOfWork unitOfWork) : IUnitOfWorkTransactionHandler
{
    #region Execute (for Result)

    public async Task<Result> ExecuteAsync(Func<IUnitOfWork, CancellationToken, Task<Result>> action, CancellationToken cancellationToken = default)
    {
        await using var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var result = await action(unitOfWork, cancellationToken);
            if (!result.IsSuccess)
            {
                await unitOfWork.RollbackTransactionAsync(transaction, cancellationToken);
                return result;
            }

            await unitOfWork.CommitTransactionAsync(transaction, cancellationToken);

            return Result.Success();
        }
        catch
        {
            await unitOfWork.RollbackTransactionAsync(transaction, cancellationToken);
            throw;
        }
    }

    #endregion

    #region Execute (for Result<T> with data)

    public async Task<Result<T>> ExecuteAsync<T>(Func<IUnitOfWork, CancellationToken, Task<Result<T>>> action, CancellationToken cancellationToken = default)
    {
        await using var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            var result = await action(unitOfWork, cancellationToken);
            if (!result.IsSuccess)
            {
                await unitOfWork.RollbackTransactionAsync(transaction, cancellationToken);
                return result;
            }

            await unitOfWork.CommitTransactionAsync(transaction, cancellationToken);
            return result;
        }
        catch
        {
            await unitOfWork.RollbackTransactionAsync(transaction, cancellationToken);
            throw;
        }
    }

    #endregion

    #region Dispose
    public async ValueTask DisposeAsync()
    {
        await unitOfWork.DisposeAsync();
    }
    #endregion
}
