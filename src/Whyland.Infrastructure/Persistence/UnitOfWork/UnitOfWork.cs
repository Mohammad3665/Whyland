using Microsoft.EntityFrameworkCore.Storage;
using Whyland.Domain.Common.Results;

namespace Whyland.Infrastructure.Persistence.UnitOfWork;

/// <summary>
/// The EF Core implementation of <see cref="IUnitOfWork"/>.
/// Coordinates change persistence and database transactions around a single <see cref="WhylandDbContext"/>.
/// </summary>
/// <remarks>
/// The context is owned by the dependency-injection container (scoped per request); the
/// explicit <see cref="DisposeAsync"/> only exists to satisfy the <see cref="IUnitOfWork"/> contract
/// for handlers that manage the unit-of-work lifetime themselves.
/// </remarks>
public class UnitOfWork(WhylandDbContext context) : IUnitOfWork
{
    #region Save

    public async Task<Result> SaveAsync(CancellationToken cancellationToken = default)
    {
        await context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    #endregion

    #region Transaction

    Task<Result> IUnitOfWork.SaveAsync(CancellationToken cancellationToken)
    {
        return SaveAsync(cancellationToken);
    }
    Task<IDbContextTransaction> IUnitOfWork.BeginTransactionAsync(CancellationToken cancellationToken)
    {
        return BeginTransactionAsync(cancellationToken);
    }

    Task IUnitOfWork.CommitTransactionAsync(IDbContextTransaction transaction, CancellationToken cancellationToken)
    {
        return CommitTransactionAsync(transaction, cancellationToken);
    }

    Task IUnitOfWork.RollbackTransactionAsync(IDbContextTransaction transaction, CancellationToken cancellationToken)
    {
        return RollbackTransactionAsync(transaction, cancellationToken);
    }

    public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        return await context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(IDbContextTransaction? transaction, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(transaction);
        await transaction.CommitAsync(cancellationToken);
    }

    public async Task RollbackTransactionAsync(IDbContextTransaction? transaction, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(transaction);
        await transaction.RollbackAsync(cancellationToken);

    }

    #endregion

    #region Dispose

    public async ValueTask DisposeAsync()
    {
        await context.DisposeAsync();
    }

    #endregion
}