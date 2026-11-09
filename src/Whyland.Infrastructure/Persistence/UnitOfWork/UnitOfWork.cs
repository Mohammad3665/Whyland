using Microsoft.EntityFrameworkCore.Storage;
using Whyland.Domain.Model.Catalog.Course.Repositories;
using Whyland.Domain.Model.Catalog.Episode.Repositories;
using Whyland.Domain.Model.Catalog.EpisodeAttachment.Repositories;
using Whyland.Infrastructure.Persistence.Repositories.Catalog.Category;
using Whyland.Infrastructure.Persistence.Repositories.Catalog.Course;
using Whyland.Infrastructure.Persistence.Repositories.Catalog.CourseFaq;
using Whyland.Infrastructure.Persistence.Repositories.Catalog.CourseSection;
using Whyland.Infrastructure.Persistence.Repositories.Catalog.Episode;
using Whyland.Infrastructure.Persistence.Repositories.Catalog.EpisodeAttachment;
using Whyland.Infrastructure.Persistence.Repositories.Content.Faq;
using Whyland.Infrastructure.Persistence.Repositories.Identity.ExternalLogin;
using Whyland.Infrastructure.Persistence.Repositories.Identity.InstructorProfile;
using Whyland.Infrastructure.Persistence.Repositories.Identity.OtpRequest;
using Whyland.Infrastructure.Persistence.Repositories.Identity.Roles;
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

    private ICategoryCommandRepository? _categoryCommandRepository;
    /// <summary>
    /// Lazily creates the singleton-per-unit-of-work <see cref="ICategoryCommandRepository"/>.
    /// </summary>
    public ICategoryCommandRepository CategoryCommandRepository =>
        _categoryCommandRepository ??= new CategoryCommandRepository(context);

    private ICategoryQueryRepository? _categoryQueryRepository;
    /// <summary>
    /// Lazily creates the singleton-per-unit-of-work <see cref="ICategoryQueryRepository"/>,
    /// built on the pooled Npgsql data source for Dapper queries.
    /// </summary>
    public ICategoryQueryRepository CategoryQueryRepository =>
        _categoryQueryRepository ??= new CategoryQueryRepository(dataSource);

    private ICourseCommandRepository? _courseCommandRepository;

    /// <summary>
    /// Lazily creates the singleton-per-unit-of-work <see cref="ICourseCommandRepository"/>.
    /// </summary>
    public ICourseCommandRepository CourseCommandRepository =>
        _courseCommandRepository ??= new CourseCommandRepository(context);

    private ICourseQueryRepository? _courseQueryRepository;

    /// <summary>
    /// Lazily creates the singleton-per-unit-of-work <see cref="ICourseQueryRepository"/>,
    /// built on the pooled Npgsql data source for Dapper queries.
    /// </summary>
    public ICourseQueryRepository CourseQueryRepository =>
        _courseQueryRepository ??= new CourseQueryRepository(dataSource);

    private ICourseSectionCommandRepository? _courseSectionCommandRepository;

    /// <summary>Lazily creates the command repository for a course's sections.</summary>
    public ICourseSectionCommandRepository CourseSectionCommandRepository =>
        _courseSectionCommandRepository ??= new CourseSectionCommandRepository(context);

    private ICourseSectionQueryRepository? _courseSectionQueryRepository;

    /// <summary>Lazily creates the Dapper read repository for a course's sections.</summary>
    public ICourseSectionQueryRepository CourseSectionQueryRepository =>
        _courseSectionQueryRepository ??= new CourseSectionQueryRepository(dataSource);

    private ICourseFaqCommandRepository? _courseFaqCommandRepository;

    /// <summary>Lazily creates the command repository for a course's FAQs.</summary>
    public ICourseFaqCommandRepository CourseFaqCommandRepository =>
        _courseFaqCommandRepository ??= new CourseFaqCommandRepository(context);

    private ICourseFaqQueryRepository? _courseFaqQueryRepository;

    /// <summary>Lazily creates the Dapper read repository for a course's FAQs.</summary>
    public ICourseFaqQueryRepository CourseFaqQueryRepository =>
        _courseFaqQueryRepository ??= new CourseFaqQueryRepository(dataSource);

    private IEpisodeCommandRepository? _episodeCommandRepository;

    /// <summary>Lazily creates the command repository for a section's episodes.</summary>
    public IEpisodeCommandRepository EpisodeCommandRepository =>
        _episodeCommandRepository ??= new EpisodeCommandRepository(context);

    private IEpisodeQueryRepository? _episodeQueryRepository;

    /// <summary>Lazily creates the Dapper read repository for a section's episodes.</summary>
    public IEpisodeQueryRepository EpisodeQueryRepository =>
        _episodeQueryRepository ??= new EpisodeQueryRepository(dataSource);

    private IEpisodeAttachmentCommandRepository? _episodeAttachmentCommandRepository;

    /// <summary>Lazily creates the command repository for an episode's attachments.</summary>
    public IEpisodeAttachmentCommandRepository EpisodeAttachmentCommandRepository =>
        _episodeAttachmentCommandRepository ??= new EpisodeAttachmentCommandRepository(context);

    private IEpisodeAttachmentQueryRepository? _episodeAttachmentQueryRepository;

    /// <summary>Lazily creates the Dapper read repository for an episode's attachments.</summary>
    public IEpisodeAttachmentQueryRepository EpisodeAttachmentQueryRepository =>
        _episodeAttachmentQueryRepository ??= new EpisodeAttachmentQueryRepository(dataSource);

    private IRoleCommandRepository? _roleCommandRepository;

    /// <summary>
    /// Lazily creates the singleton-per-unit-of-work <see cref="IRoleCommandRepository"/>.
    /// </summary>
    public IRoleCommandRepository RoleCommandRepository =>
        _roleCommandRepository ??= new RoleCommandRepository(context);

    private IRoleQueryRepository? _roleQueryRepository;

    /// <summary>
    /// Lazily creates the singleton-per-unit-of-work <see cref="IRoleQueryRepository"/>,
    /// built on the pooled Npgsql data source for Dapper queries.
    /// </summary>
    public IRoleQueryRepository RoleQueryRepository =>
        _roleQueryRepository ??= new RoleQueryRepository(dataSource);

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
    /// Every read — the single-row auth ones and the admin catalog — runs on the Dapper
    /// <c>dataSource</c>; query repositories never take the EF Core <c>context</c>.
    /// </summary>
    public IUserQueryRepository UserQueryRepository =>
        _userQueryRepository ??= new UserQueryRepository(dataSource);

    private IInstructorProfileCommandRepository? _instructorProfileCommandRepository;

    /// <summary>
    /// Lazily-initialized repository for write (command) operations on instructor profiles.
    /// </summary>
    public IInstructorProfileCommandRepository InstructorProfileCommandRepository =>
        _instructorProfileCommandRepository ??= new InstructorProfileCommandRepository(context);

    private IInstructorProfileQueryRepository? _instructorProfileQueryRepository;

    /// <summary>
    /// Lazily-initialized repository for read (query) operations on instructor profiles.
    /// </summary>
    public IInstructorProfileQueryRepository InstructorProfileQueryRepository =>
        _instructorProfileQueryRepository ??= new InstructorProfileQueryRepository(dataSource);

    private IExternalLoginCommandRepository? _externalLoginCommandRepository;

    /// <summary>
    /// Lazily-initialized repository for external provider links (s5.4).
    /// </summary>
    public IExternalLoginCommandRepository ExternalLoginCommandRepository =>
        _externalLoginCommandRepository ??= new ExternalLoginCommandRepository(context);
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