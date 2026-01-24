using MediatR;
using Ardalis.Result;

namespace Apex.API.UseCases.Users.PMs;

/// <summary>
/// Query to list project managers in the current tenant
/// </summary>
public record ListPMsQuery : IRequest<Result<List<PMDto>>>;

/// <summary>
/// Project Manager DTO for list operations
/// </summary>
public record PMDto(
    Guid UserId,
    string Email,
    string FirstName,
    string LastName,
    string FullName,
    IEnumerable<string> Roles);