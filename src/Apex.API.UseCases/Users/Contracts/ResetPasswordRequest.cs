namespace Apex.API.UseCases.Users.Contracts;

/// <summary>
/// Request to reset password with token
/// </summary>
public sealed record ResetPasswordRequest
{
    /// <summary>
    /// Email address of the user
    /// </summary>
    public required string Email { get; init; }

    /// <summary>
    /// Password reset token sent via email
    /// </summary>
    public required string Token { get; init; }

    /// <summary>
    /// New password
    /// </summary>
    public required string NewPassword { get; init; }

    /// <summary>
    /// Confirm new password
    /// </summary>
    public required string ConfirmPassword { get; init; }
}
