using MediatR;
using Ardalis.Result;

namespace Apex.API.UseCases.Users.Login;

/// <summary>
/// Command to authenticate a user and return JWT token
/// </summary>
public record LoginCommand(
    string Email,
    string Password
) : IRequest<Result<LoginResponse>>;

/// <summary>
/// Response containing JWT token and user information
/// </summary>
public record LoginResponse(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt,
    UserInfo User
);

/// <summary>
/// User information returned after successful login
/// </summary>
public record UserInfo(
    Guid UserId,
    string Email,
    string FirstName,
    string LastName,
    string FullName,
    Guid TenantId,
    string[] Roles
);
