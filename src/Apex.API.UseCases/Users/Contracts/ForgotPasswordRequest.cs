namespace Apex.API.UseCases.Users.Contracts;

/// <summary>
/// Request to initiate password reset
/// </summary>
public sealed record ForgotPasswordRequest
{
    /// <summary>
    /// Email address of the user requesting password reset
    /// </summary>
    public required string Email { get; init; }
}
