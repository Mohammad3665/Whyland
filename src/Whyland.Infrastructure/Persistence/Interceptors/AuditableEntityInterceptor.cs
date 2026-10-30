using Microsoft.EntityFrameworkCore.Diagnostics;
using Whyland.Domain.Common.Primitives;

namespace Whyland.Infrastructure.Persistence.Interceptors;

/// <summary>
/// An EF Core <see cref="SaveChangesInterceptor"/> that applies auditing rules
/// to <see cref="IAuditable"/> entities and converts hard deletes into soft deletes
/// for <see cref="ISoftDeletable"/> entities, just before changes are persisted.
/// </summary>
public sealed class AuditableEntityInterceptor(TimeProvider timeProvider, ICurrentUserService currentUser) : SaveChangesInterceptor
{
    /// <summary>
    /// Applies audit rules synchronously before saving changes.
    /// </summary>
    /// <param name="eventData">Contextual information about the current save operation.</param>
    /// <param name="result">The interception result produced by the previous interceptor.</param>
    /// <returns>The interception result passed through to the next stage.</returns>
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        ApplyAuditRules(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    /// <summary>
    /// Applies audit rules asynchronously before saving changes.
    /// </summary>
    /// <param name="eventData">Contextual information about the current save operation.</param>
    /// <param name="result">The interception result produced by the previous interceptor.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A <see cref="ValueTask{TResult}"/> that completes with the interception result passed through to the next stage.</returns>
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        ApplyAuditRules(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    /// <summary>
    /// Stamps <see cref="IAuditable"/> entities with creation/update metadata and
    /// rewrites hard deletes of <see cref="ISoftDeletable"/> entities as soft deletes.
    /// </summary>
    /// <param name="context">The <see cref="DbContext"/> whose change tracker is inspected. May be <c>null</c>.</param>
    private void ApplyAuditRules(DbContext? context)
    {
        if (context is null) return;

        var now = timeProvider.GetUtcNow().UtcDateTime;
        var userId = currentUser.UserId;

        // --- Audit: Created / Updated ---
        foreach (var entry in context.ChangeTracker.Entries<IAuditable>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = now;
                    entry.Entity.CreatedBy = userId;
                    entry.Entity.UpdatedAt = now;
                    entry.Entity.UpdatedBy = userId;
                    break;

                case EntityState.Modified:
                    entry.Entity.UpdatedAt = now;
                    entry.Entity.UpdatedBy = userId;

                    entry.Property(e => e.CreatedAt).IsModified = false;
                    entry.Property(e => e.CreatedBy).IsModified = false;
                    break;
            }
        }

        // --- Soft delete ---
        foreach (var entry in context.ChangeTracker.Entries<ISoftDeletable>())
        {
            if (entry.State != EntityState.Deleted) continue;

            entry.State = EntityState.Modified;
            entry.Entity.IsDeleted = true;
            entry.Entity.DeletedAt = now;
            entry.Entity.DeletedBy = userId;
        }
    }
}