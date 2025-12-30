using MediatR;
using Ardalis.Result;
using Apex.API.Core.ValueObjects;

namespace Apex.API.UseCases.Users.Register;

/// <summary>
/// Command to register a new user within a tenant
/// </summary>
public record RegisterUserCommand(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string? PhoneNumber = null,
    string? TimeZone = null
) : IRequest<Result<UserId>>;

/// <summary>
/// Strongly-typed identifier for User
/// </summary>
public readonly record struct UserId(Guid Value)
{
    public static UserId From(Guid value) => new(value);
    public static UserId CreateUnique() => new(Guid.NewGuid());
}
