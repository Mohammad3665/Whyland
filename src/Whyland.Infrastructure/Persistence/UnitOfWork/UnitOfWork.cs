using Microsoft.EntityFrameworkCore.Storage;
using Whyland.Domain.IRepositories.Identity.OtpRequest;
using Whyland.Domain.IRepositories.Identity.User;
using Whyland.Infrastructure.Persistence.Repositories.Content.Faq;
using Whyland.Infrastructure.Persistence.Repositories.Identity.OtpRequest;
using Whyland.Infrastructure.Persistence.Repositories.Identity.User;

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
public class UnitOfWork(WhylandDbContext context, NpgsqlDataSource dataSource) : IUnitOfWork
{
    #region Repositories

    private IFaqCommandRepository? _faqCommandRepository;
    /// <summary>
    /// Lazily creates the singleton-per-unit-of-work <see cref="IFaqCommandRepository"/>.
    /// </summary>
    public IFaqCommandRepository FaqCommandRepository => _faqCommandRepository ??= new FaqCommandRepository(context);

    private IFaqQueryRepository? _faqQueryRepository;
    /// <summary>
    /// Lazily creates the singleton-per-unit-of-work <see cref="IFaqQueryRepository"/>,
    /// built on the pooled Npgsql data source for Dapper queries.
    /// </summary>
    public IFaqQueryRepository FaqQueryRepository => _faqQueryRepository ??= new FaqQueryRepository(dataSource);

    private IOtpRequestCommandRepository? _otpRequestCommandRepository;

    /// <summary>
    /// Lazily-initialized repository for write (command) operations on OTP request entities.
    /// The instance is created on first access and cached for the lifetime of the unit of work.
    /// </summary>
    public IOtpRequestCommandRepository OtpRequestCommandRepository =>
        _otpRequestCommandRepository ??= new OtpRequestCommandRepository(context);

    private IUserCommandRepository? _userCommandRepository;

    /// <summary>
    /// Lazily-initialized repository for write (command) operations on User entities.
    /// The instance is created on first access and cached for the lifetime of the unit of work.
    /// </summary>
    public IUserCommandRepository UserCommandRepository =>
        _userCommandRepository ??= new UserCommandRepository(context);

    private IUserQueryRepository? _userQueryRepository;

    /// <summary>
    /// Lazily-initialized repository for read (query) operations on User entities.
    /// Unlike the command repositories, this one is backed by a Dapper data source
    /// (<c>dataSource</c>) rather than the EF Core <c>context</c>.
    /// </summary>
    public IUserQueryRepository UserQueryRepository =>
        _userQueryRepository ??= new UserQueryRepository(dataSource);
    #endregion


    #region Save

    /// <summary>
    /// Persists all tracked changes to the database.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the save operation.</param>
    /// <returns><see cref="Result.Success"/> when the save completed, or the failure result.</returns>
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

    /// <summary>
    /// Begins a database transaction for grouping multiple persistence operations.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The started database transaction.</returns>
    public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        return await context.Database.BeginTransactionAsync(cancellationToken);
    }

    /// <summary>
    /// Commits the given transaction, making its changes permanent.
    /// </summary>
    /// <param name="transaction">The transaction to commit; must not be <c>null</c>.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    public async Task CommitTransactionAsync(IDbContextTransaction? transaction, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(transaction);
        await transaction.CommitAsync(cancellationToken);
    }

    /// <summary>
    /// Rolls back the given transaction, discarding all of its changes.
    /// </summary>
    /// <param name="transaction">The transaction to roll back; must not be <c>null</c>.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    public async Task RollbackTransactionAsync(IDbContextTransaction? transaction, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(transaction);
        await transaction.RollbackAsync(cancellationToken);

    }

    #endregion

    #region Dispose

    /// <summary>
    /// Disposes the underlying scoped <see cref="WhylandDbContext"/>.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        await context.DisposeAsync();
    }

    #endregion
}