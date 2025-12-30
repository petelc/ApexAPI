using FluentValidation;

namespace Apex.API.UseCases.Tenants.Create;

/// <summary>
/// Validator for CreateTenantCommand
/// </summary>
public class CreateTenantValidator : AbstractValidator<CreateTenantCommand>
{
    public CreateTenantValidator()
    {
        RuleFor(x => x.CompanyName)
            .NotEmpty().WithMessage("Company name is required.")
            .MaximumLength(200).WithMessage("Company name cannot exceed 200 characters.");

        RuleFor(x => x.Subdomain)
            .NotEmpty().WithMessage("Subdomain is required.")
            .MinimumLength(3).WithMessage("Subdomain must be at least 3 characters.")
            .MaximumLength(63).WithMessage("Subdomain cannot exceed 63 characters.")
            .Matches(@"^[a-z0-9]([a-z0-9-]{0,61}[a-z0-9])?$")
            .WithMessage("Subdomain must start and end with alphanumeric characters and can only contain lowercase letters, numbers, and hyphens.");

        RuleFor(x => x.AdminEmail)
            .NotEmpty().WithMessage("Admin email is required.")
            .EmailAddress().WithMessage("Admin email must be a valid email address.")
            .MaximumLength(255).WithMessage("Admin email cannot exceed 255 characters.");

        RuleFor(x => x.AdminFirstName)
            .NotEmpty().WithMessage("Admin first name is required.")
            .MaximumLength(100).WithMessage("Admin first name cannot exceed 100 characters.");

        RuleFor(x => x.AdminLastName)
            .NotEmpty().WithMessage("Admin last name is required.")
            .MaximumLength(100).WithMessage("Admin last name cannot exceed 100 characters.");

        RuleFor(x => x.Region)
            .Must(BeValidRegion)
            .When(x => x.Region != null)
            .WithMessage("Region must be a valid region code (e.g., 'us-east-1', 'eu-west-1').");
    }

    private bool BeValidRegion(string? region)  // ✅ Nullable parameter
    {
        if (string.IsNullOrWhiteSpace(region))
            return true; // Null/empty is valid (optional field)

        // Valid AWS-style region codes
        var validRegions = new[]
        {
            "us-east-1", "us-east-2", "us-west-1", "us-west-2",
            "eu-west-1", "eu-west-2", "eu-west-3", "eu-central-1",
            "ap-southeast-1", "ap-southeast-2", "ap-northeast-1"
        };

        return validRegions.Contains(region);
    }
}
