using FluentValidation;

namespace Apex.API.UseCases.Users.Register;

/// <summary>
/// Validator for RegisterUserCommand
/// </summary>
public class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
                .WithMessage("Email is required.")
            .EmailAddress()
                .WithMessage("Email must be a valid email address.")
            .MaximumLength(255)
                .WithMessage("Email cannot exceed 255 characters.");

        RuleFor(x => x.Password)
            .NotEmpty()
                .WithMessage("Password is required.")
            .MinimumLength(8)
                .WithMessage("Password must be at least 8 characters.")
            .MaximumLength(100)
                .WithMessage("Password cannot exceed 100 characters.")
            .Matches(@"[A-Z]")
                .WithMessage("Password must contain at least one uppercase letter.")
            .Matches(@"[a-z]")
                .WithMessage("Password must contain at least one lowercase letter.")
            .Matches(@"[0-9]")
                .WithMessage("Password must contain at least one number.")
            .Matches(@"[^a-zA-Z0-9]")
                .WithMessage("Password must contain at least one special character.");

        RuleFor(x => x.FirstName)
            .NotEmpty()
                .WithMessage("First name is required.")
            .MinimumLength(1)
                .WithMessage("First name must be at least 1 character.")
            .MaximumLength(100)
                .WithMessage("First name cannot exceed 100 characters.");

        RuleFor(x => x.LastName)
            .NotEmpty()
                .WithMessage("Last name is required.")
            .MinimumLength(1)
                .WithMessage("Last name must be at least 1 character.")
            .MaximumLength(100)
                .WithMessage("Last name cannot exceed 100 characters.");

        RuleFor(x => x.PhoneNumber)
            .Matches(@"^\+?[1-9]\d{1,14}$")
                .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber))
                .WithMessage("Phone number must be a valid international format (E.164).");

        RuleFor(x => x.TimeZone)
            .Must(BeValidTimeZone)
                .When(x => !string.IsNullOrWhiteSpace(x.TimeZone))
                .WithMessage("Time zone must be a valid IANA time zone identifier.");
    }

    private bool BeValidTimeZone(string? timeZone)
    {
        if (string.IsNullOrWhiteSpace(timeZone))
            return true;

        try
        {
            TimeZoneInfo.FindSystemTimeZoneById(timeZone);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
