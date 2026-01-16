namespace Apex.API.UseCases.Users.DTOs;

/// <summary>
/// User information DTO for API responses
/// </summary>
public sealed record UserDto
{
    public Guid Id { get; init; }
    public string Email { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}".Trim();
    public bool IsActive { get; init; }
}

/// <summary>
/// Simplified user info for embedded responses (less data)
/// </summary>
public sealed record UserSummaryDto
{
    public Guid Id { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
}
