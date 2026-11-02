using Whyland.Domain.Common.Errors;

namespace Whyland.Domain.Entities.Catalog.Errors;

/// <summary>
/// Every failure the <see cref="Category"/> aggregate can report, including the ones raised by the
/// handlers that save or read it.
/// <para>
/// Each factory carries only the code, the category and the arguments the message needs; the wording
/// lives in the localization catalog.
/// </para>
/// </summary>
public static class CategoryErrors
{
    /// <summary>The specified field is required but was empty or whitespace.</summary>
    /// <param name="field">The name of the offending field.</param>
    public static DomainError FieldRequired(string field) =>
        DomainError.For<Category>(nameof(FieldRequired), ErrorType.Validation, field: field);

    /// <summary>The specified field exceeds its maximum allowed length.</summary>
    /// <param name="field">The name of the offending field.</param>
    /// <param name="max">The maximum allowed length.</param>
    public static DomainError FieldTooLong(string field, int max) =>
        DomainError.For<Category>(nameof(FieldTooLong), ErrorType.Validation, field: field, ("Max", max));

    /// <summary>The specified field carries a negative value where only zero or above is allowed.</summary>
    /// <param name="field">The name of the offending field.</param>
    public static DomainError GreaterThanOrEqualToZero(string field) =>
        DomainError.For<Category>(nameof(GreaterThanOrEqualToZero), ErrorType.Validation, field: field);

    /// <summary>No category exists with the given identifier.</summary>
    /// <param name="id">The identifier that was looked up.</param>
    public static DomainError NotFound(long id) =>
        DomainError.For<Category>(nameof(NotFound), ErrorType.NotFound, field: null, ("Id", id));

    /// <summary>
    /// The chosen parent does not exist: it was deleted, or it belongs to another aggregate.
    /// A separate code from <see cref="NotFound"/> because here the row being saved does exist and
    /// it is the reference that is broken.
    /// </summary>
    /// <param name="parentId">The parent identifier that could not be resolved.</param>
    public static DomainError ParentNotFound(long parentId) =>
        DomainError.For<Category>(nameof(ParentNotFound), ErrorType.NotFound, field: null, ("ParentId", parentId));

    /// <summary>
    /// The name is already used by another category of the same sibling group, so the two would be
    /// indistinguishable where the catalogue is shown.
    /// </summary>
    /// <param name="name">The name that clashed.</param>
    public static DomainError NameTaken(string name) =>
        DomainError.For<Category>(nameof(NameTaken), ErrorType.Validation, nameof(Category.Name), ("Name", name));

    /// <summary>
    /// The new parent is the category itself or one of its own descendants, which would make the
    /// tree un-walkable.
    /// </summary>
    public static DomainError CircularParent() =>
        DomainError.For<Category>(nameof(CircularParent), ErrorType.Validation, nameof(Category.ParentId));

    /// <summary>
    /// The move would push the branch past the depth the catalogue allows.
    /// </summary>
    /// <param name="maxDepth">The ceiling from the entity, so the message never hardcodes a number.</param>
    public static DomainError MaxDepthExceeded(int maxDepth) =>
        DomainError.For<Category>(nameof(MaxDepthExceeded), ErrorType.Validation, nameof(Category.ParentId), ("MaxDepth", maxDepth));

    /// <summary>
    /// The chosen parent already holds the largest number of children the catalogue allows, so there
    /// is no slot left under it — the branch is wide enough.
    /// </summary>
    /// <param name="max">The ceiling from the entity, so the message never hardcodes a number.</param>
    public static DomainError MaxChildrenExceeded(int max) =>
        DomainError.For<Category>(nameof(MaxChildrenExceeded), ErrorType.Validation, nameof(Category.ParentId), ("Max", max));

    /// <summary>The category could not be created (persistence failure); masked behind a generic message.</summary>
    public static DomainError CreateFailed() =>
        DomainError.For<Category>(nameof(CreateFailed), ErrorType.Unexpected);

    /// <summary>The category could not be saved (persistence failure); masked behind a generic message.</summary>
    public static DomainError UpdateFailed() =>
        DomainError.For<Category>(nameof(UpdateFailed), ErrorType.Unexpected);

    /// <summary>The category could not be soft-deleted (persistence failure); masked behind a generic message.</summary>
    public static DomainError SoftDeleteFailed() =>
        DomainError.For<Category>(nameof(SoftDeleteFailed), ErrorType.Unexpected);

    /// <summary>The categories could not be read (persistence failure); masked behind a generic message.</summary>
    public static DomainError ListFailed() =>
        DomainError.For<Category>(nameof(ListFailed), ErrorType.Unexpected);

    /// <summary>
    /// The category still has live children, so deleting it would leave a branch hanging on a parent
    /// that no longer exists.
    /// </summary>
    /// <param name="childCount">How many children are waiting above the category being deleted.</param>
    public static DomainError HasChildren(int childCount) =>
        DomainError.For<Category>(nameof(HasChildren), ErrorType.Validation, field: null, ("ChildCount", childCount));

    /// <summary>The new display order could not be persisted (persistence failure); masked behind a generic message.</summary>
    public static DomainError ReorderFailed() =>
        DomainError.For<Category>(nameof(ReorderFailed), ErrorType.Unexpected);
}
