using System.Security.Claims;

namespace Apex.API.UseCases.Common.Interfaces;

/// <summary>
/// Service for generating and validating JWT tokens
/// </summary>
public interface IJwtTokenService
{
    string GenerateAccessToken(Guid userId, string email, string fullName, string firstName, string lastName, Guid tenantId, IList<string> roles);
    string GenerateRefreshToken();
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
    Task<bool> ValidateRefreshToken(Guid userId, string refreshToken);
    Task SaveRefreshToken(Guid userId, string refreshToken, DateTime expiresAt);
    Task RevokeRefreshToken(Guid userId);
}
