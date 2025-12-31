using MediatR;
using Ardalis.Result;

namespace Apex.API.UseCases.Users.List;

/// <summary>
/// Query to list users in the current tenant
/// </summary>
public record ListUsersQuery : IRequest<Result<List<UserDto>>>;

/// <summary>
/// User DTO for list operations
/// </summary>
public record UserDto(
    Guid UserId,
    string Email,
    string FirstName,
    string LastName,
    string FullName,
    bool IsActive,
    IEnumerable<string> Roles);
