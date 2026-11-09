using Whyland.Domain.Common.Errors;

using RoleEntity = Whyland.Domain.Model.Authorization.Role.Entities.Role;

namespace Whyland.Domain.Model.Authorization.Role.Errors;

/// <summary>
/// Every failure the <see cref="RoleEntity"/> aggregate can report, including the ones raised by the
/// handlers that save or read it.
/// <para>
/// Each factory carries only the code, the category and the arguments the message needs. The wording
/// lives in the localization catalog, so a failure never embeds user input in a hand-written sentence.
/// </para>
/// </summary>
public static class RoleErrors
{
    /// <summary>The specified field is required but was empty or whitespace.</summary>
    /// <param name="field">The name of the offending field.</param>
    public static DomainError FieldRequired(string field) =>
        DomainError.For<RoleEntity>(nameof(FieldRequired), ErrorType.Validation, field: field);

    /// <summary>The specified field exceeds its maximum allowed length.</summary>
    /// <param name="field">The name of the offending field.</param>
    /// <param name="max">The maximum allowed length.</param>
    public static DomainError FieldTooLong(string field, int max) =>
        DomainError.For<RoleEntity>(nameof(FieldTooLong), ErrorType.Validation, field: field, ("Max", max));

    /// <summary>No role exists with the given identifier.</summary>
    /// <param name="id">The identifier that was looked up.</param>
    public static DomainError NotFound(RoleId id) =>
        DomainError.For<RoleEntity>(nameof(NotFound), ErrorType.NotFound, field: null, ("Id", id));

    /// <summary>
    /// The name is already used by another role. Unlike a category, a role name has no sibling group
    /// to be unique inside: it is the string the code and the seeders refer to, so it must be unique
    /// across the whole table.
    /// </summary>
    /// <param name="name">The name that clashed.</param>
    public static DomainError NameTaken(string name) =>
        DomainError.For<RoleEntity>(nameof(NameTaken), ErrorType.Validation, nameof(RoleEntity.Name), ("Name", name));

    /// <summary>
    /// A role the application itself depends on (the seeder's SuperAdmin/Admin/Instructor/User) was
    /// asked to be renamed, retitled or deleted. Such a role is managed by its seed row and its
    /// permissions, not by the panel, because the code looks it up by name.
    /// </summary>
    public static DomainError SystemRoleProtected() =>
        DomainError.For<RoleEntity>(nameof(SystemRoleProtected), ErrorType.Validation);

    /// <summary>
    /// One of the permission identifiers of a grant does not exist, so the role would be given a key
    /// nothing can ever honour.
    /// </summary>
    /// <param name="permissionId">The identifier that could not be resolved.</param>
    public static DomainError PermissionNotFound(PermissionId permissionId) =>
        DomainError.For<RoleEntity>(nameof(PermissionNotFound), ErrorType.NotFound, field: null, ("PermissionId", permissionId));

    /// <summary>The role could not be created (persistence failure); masked behind a generic message.</summary>
    public static DomainError CreateFailed() =>
        DomainError.For<RoleEntity>(nameof(CreateFailed), ErrorType.Unexpected);

    /// <summary>The role could not be updated (persistence failure); masked behind a generic message.</summary>
    public static DomainError UpdateFailed() =>
        DomainError.For<RoleEntity>(nameof(UpdateFailed), ErrorType.Unexpected);

    /// <summary>The role could not be deleted (persistence failure); masked behind a generic message.</summary>
    public static DomainError SoftDeleteFailed() =>
        DomainError.For<RoleEntity>(nameof(SoftDeleteFailed), ErrorType.Unexpected);

    /// <summary>The role list could not be read; masked behind a generic message.</summary>
    public static DomainError ListFailed() =>
        DomainError.For<RoleEntity>(nameof(ListFailed), ErrorType.Unexpected);
}
