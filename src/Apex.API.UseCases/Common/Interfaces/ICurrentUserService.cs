namespace Apex.API.UseCases.Common.Interfaces;

/// <summary>
/// Service to get information about the current authenticated user
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// Gets the current user's ID
    /// </summary>
    Guid UserId { get; }

    /// <summary>
    /// Gets the current user's email
    /// </summary>
    string Email { get; }

    /// <summary>
    /// Gets the current user's tenant ID
    /// </summary>
    Guid TenantId { get; }

    /// <summary>
    /// Gets the current user's roles
    /// </summary>
    IEnumerable<string> Roles { get; }

    /// <summary>
    /// Checks if user is authenticated
    /// </summary>
    bool IsAuthenticated { get; }

    /// <summary>
    /// Checks if user has a specific role
    /// </summary>
    bool IsInRole(string role);
}
