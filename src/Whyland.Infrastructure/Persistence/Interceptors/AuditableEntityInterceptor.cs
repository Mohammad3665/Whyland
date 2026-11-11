using Microsoft.EntityFrameworkCore.ChangeTracking;
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
    /// Whether the only modified property of the row is its display order. Renumbering is an
    /// internal ordering pass, not a business edit, so it must not stamp the update audit.
    /// </summary>
    /// <param name="entry">The tracked auditable entity.</param>
    /// <returns><c>true</c> when no field other than <c>DisplayOrder</c> changed.</returns>
    private static bool IsOrderingOnlyChange(EntityEntry<IAuditable> entry)
    {
        var changed = entry.Properties.Where(property => property.IsModified).ToList();

        return changed.Count > 0 &&
               changed.All(property => property.Metadata.Name == "DisplayOrder");
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
        // The contract of the accessor is a raw guid, while the audit columns store the account
        // identifier, so the wrapper happens once here.
        var userId = currentUser.UserId;

        // --- Audit: Created / Updated ---
        foreach (var entry in context.ChangeTracker.Entries<IAuditable>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Property(nameof(IAuditable.CreatedAt)).CurrentValue = now;
                    entry.Property(nameof(IAuditable.CreatedBy)).CurrentValue = userId;
                    // A new row has not been edited yet, so its update audit stays empty. Only
                    // CreatedAt/CreatedBy describe it until the first real modification.
                    entry.Property(nameof(IAuditable.UpdatedAt)).CurrentValue = null;
                    entry.Property(nameof(IAuditable.UpdatedBy)).CurrentValue = null;
                    break;

                case EntityState.Modified:
                    // Automatic 1..n renumbering only rewrites DisplayOrder. That maintenance
                    // pass is not a user edit of the affected rows, so it must not populate their
                    // update audit (or a fresh row would look edited immediately after creation).
                    if (!IsOrderingOnlyChange(entry))
                    {
                        entry.Property(nameof(IAuditable.UpdatedAt)).CurrentValue = now;
                        entry.Property(nameof(IAuditable.UpdatedBy)).CurrentValue = userId;
                    }

                    entry.Property(nameof(IAuditable.CreatedAt)).IsModified = false;
                    entry.Property(nameof(IAuditable.CreatedBy)).IsModified = false;
                    break;
            }
        }

        // --- Soft delete ---
        foreach (var entry in context.ChangeTracker.Entries<ISoftDeletable>())
        {
            if (entry.State != EntityState.Deleted) continue;

            entry.State = EntityState.Modified;
            entry.Property(nameof(ISoftDeletable.IsDeleted)).CurrentValue = true;
            entry.Property(nameof(ISoftDeletable.DeletedAt)).CurrentValue = now;
            entry.Property(nameof(ISoftDeletable.DeletedBy)).CurrentValue = userId;
        }
    }
}