namespace Apex.API.UseCases.Common.Interfaces;

/// <summary>
/// Service for user authentication operations
/// Abstracts Identity framework from UseCases layer
/// </summary>
public interface IAuthenticationService
{
    Task<AuthenticationResult> AuthenticateAsync(string email, string password);
}

/// <summary>
/// Result of authentication attempt
/// </summary>
public class AuthenticationResult
{
    public bool Succeeded { get; set; }
    public bool IsLockedOut { get; set; }
    public Guid? UserId { get; set; }
    public string? Email { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? FullName { get; set; }
    public Guid? TenantId { get; set; }
    public bool IsActive { get; set; }
    public IList<string> Roles { get; set; } = new List<string>();
    public string? ErrorMessage { get; set; }
}