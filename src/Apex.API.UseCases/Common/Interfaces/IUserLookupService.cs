using Apex.API.UseCases.Users.DTOs;

namespace Apex.API.UseCases.Users.Interfaces;

/// <summary>
/// Service for looking up user information
/// </summary>
public interface IUserLookupService
{
    /// <summary>
    /// Gets user by ID
    /// </summary>
    Task<UserDto?> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets user summary (lighter weight) by ID
    /// </summary>
    Task<UserSummaryDto?> GetUserSummaryByIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets multiple users by IDs (efficient batch lookup)
    /// </summary>
    Task<Dictionary<Guid, UserDto>> GetUsersByIdsAsync(
        IEnumerable<Guid> userIds, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets multiple user summaries by IDs (efficient batch lookup)
    /// </summary>
    Task<Dictionary<Guid, UserSummaryDto>> GetUserSummariesByIdsAsync(
        IEnumerable<Guid> userIds, 
        CancellationToken cancellationToken = default);
}
