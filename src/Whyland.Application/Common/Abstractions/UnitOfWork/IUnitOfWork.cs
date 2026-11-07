using Microsoft.EntityFrameworkCore.Storage;
using Whyland.Domain.Model.Authorization.Role.Repositories;
using Whyland.Domain.Model.Catalog.Category.Repositories;
using Whyland.Domain.Model.Catalog.Course.Repositories;
using Whyland.Domain.Model.Identity.ExternalLogin.Repositories;
using Whyland.Domain.Model.Identity.InstructorProfile.Repositories;
using Whyland.Domain.Model.Identity.OtpRequest.Repositories;
using Whyland.Domain.Model.Identity.User.Repositories;
using Whyland.Domain.Model.Content.Faq.Repositories;

namespace Whyland.Application.Common.Abstractions.UnitOfWork;

/// <summary>
/// Unit of Work pattern for coordinating multiple repositories and database transactions.
/// </summary>
/// <remarks>
/// Provides a unified interface for all repositories and ensures transactional consistency.
/// All changes are committed together or rolled back entirely.
/// </remarks>
public interface IUnitOfWork : IAsyncDisposable
{
    #region Repositories

    /// <summary>
    /// Repository for write (command) operations on FAQ entities.
    /// </summary>
    IFaqCommandRepository FaqCommandRepository { get; }

    /// <summary>
    /// Repository for read (query) operations on FAQ entities.
    /// </summary>
    IFaqQueryRepository FaqQueryRepository { get; }

    /// <summary>
    /// Repository for write (command) operations on Category entities.
    /// </summary>
    ICategoryCommandRepository CategoryCommandRepository { get; }

    /// <summary>
    /// Repository for read (query) operations on Category entities.
    /// </summary>
    ICategoryQueryRepository CategoryQueryRepository { get; }

    /// <summary>
    /// Repository for write (command) operations on Course entities.
    /// </summary>
    ICourseCommandRepository CourseCommandRepository { get; }

    /// <summary>
    /// Repository for read (query) operations on Course entities.
    /// </summary>
    ICourseQueryRepository CourseQueryRepository { get; }

    /// <summary>
    /// Repository for write (command) operations on Role entities.
    /// </summary>
    IRoleCommandRepository RoleCommandRepository { get; }

    /// <summary>
    /// Repository for read (query) operations on Role entities.
    /// </summary>
    IRoleQueryRepository RoleQueryRepository { get; }

    /// <summary>
    /// Repository for write (command) operations on OTP request entities.
    /// </summary>
    IOtpRequestCommandRepository OtpRequestCommandRepository { get; }

    /// <summary>
    /// Repository for write (command) operations on User entities.
    /// </summary>
    IUserCommandRepository UserCommandRepository { get; }

    /// <summary>
    /// Repository for read (query) operations on User entities.
    /// </summary>
    IUserQueryRepository UserQueryRepository { get; }

    /// <summary>
    /// Repository for write (command) operations on instructor profiles.
    /// </summary>
    IInstructorProfileCommandRepository InstructorProfileCommandRepository { get; }

    /// <summary>
    /// Repository for read (query) operations on instructor profiles.
    /// </summary>
    IInstructorProfileQueryRepository InstructorProfileQueryRepository { get; }

    /// <summary>
    /// Repository for write (command) operations on external provider links (s5.4).
    /// </summary>
    IExternalLoginCommandRepository ExternalLoginCommandRepository { get; }

    #endregion

    #region Transactions

    /// <summary>
    /// Saves all changes without a transaction.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Success result or failure with error details.</returns>
    Task<Result> SaveAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Begins a new database transaction.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The started transaction, to be finished with <see cref="CommitTransactionAsync"/> or <see cref="RollbackTransactionAsync"/>.</returns>
    Task<IDbContextTransaction> BeginTransactionAsync(
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Commits the transaction, persisting every change made inside it.
    /// </summary>
    /// <param name="transaction">The transaction to commit.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous commit operation.</returns>
    Task CommitTransactionAsync(
        IDbContextTransaction transaction,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Rolls the transaction back, discarding every change made inside it.
    /// </summary>
    /// <param name="transaction">The transaction to roll back.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous rollback operation.</returns>
    Task RollbackTransactionAsync(
        IDbContextTransaction transaction,
        CancellationToken cancellationToken = default
    );
    #endregion
}
