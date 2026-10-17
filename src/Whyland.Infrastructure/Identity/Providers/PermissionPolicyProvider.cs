using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace Whyland.Infrastructure.Identity.Providers;

/// <summary>
/// Provides dynamic authorization policies for permissions.
/// </summary>
/// <remarks>
/// Creates policies on-demand for permission-based authorization.
/// </remarks>
public class PermissionPolicyProvider(IOptions<AuthorizationOptions> options)
    : DefaultAuthorizationPolicyProvider(options)
{
    /// <summary>
    /// Returns the registered policy when one exists; otherwise builds a permission-based
    /// policy on demand so any permission key can be used directly as a policy name.
    /// </summary>
    /// <param name="policyName">The policy name to resolve, interpreted as a permission key on fallback.</param>
    /// <returns>The matching authorization policy, or <c>null</c> when it cannot be built.</returns>
    public override async Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        var policy = await base.GetPolicyAsync(policyName);
        if (policy is not null)
            return policy;

        return new AuthorizationPolicyBuilder()
            .AddRequirements(new PermissionRequirement(policyName))
            .Build();
    }
}

/// <summary>
/// An authorization requirement asserting that the user holds a specific permission claim.
/// </summary>
/// <param name="Permission">The permission key the user must carry (e.g. "course.create").</param>
public record PermissionRequirement(string Permission) : IAuthorizationRequirement;
