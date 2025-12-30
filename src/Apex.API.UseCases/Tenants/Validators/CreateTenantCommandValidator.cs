using FluentValidation;
using Apex.API.UseCases.Tenants.Create;

namespace Apex.API.UseCases.Tenants.Validators;

/// <summary>
/// Validator for CreateTenantCommand
/// Automatically runs before the handler via MediatR pipeline
/// </summary>
public class CreateTenantCommandValidator : AbstractValidator<CreateTenantCommand>
{
    public CreateTenantCommandValidator()
    {
        RuleFor(x => x.CompanyName)
            .NotEmpty()
                .WithMessage("Company name is required.")
            .MinimumLength(2)
                .WithMessage("Company name must be at least 2 characters.")
            .MaximumLength(200)
                .WithMessage("Company name cannot exceed 200 characters.");

        RuleFor(x => x.Subdomain)
            .NotEmpty()
                .WithMessage("Subdomain is required.")
            .MinimumLength(3)
                .WithMessage("Subdomain must be at least 3 characters.")
            .MaximumLength(63)
                .WithMessage("Subdomain cannot exceed 63 characters.")
            .Matches(@"^[a-z0-9]([a-z0-9-]{0,61}[a-z0-9])?$")
                .WithMessage("Subdomain must start and end with alphanumeric characters and can only contain lowercase letters, numbers, and hyphens.")
            .Must(NotContainReservedWords)
                .WithMessage("Subdomain cannot use reserved words like 'admin', 'api', 'www', etc.");

        RuleFor(x => x.AdminEmail)
            .NotEmpty()
                .WithMessage("Admin email is required.")
            .EmailAddress()
                .WithMessage("Admin email must be a valid email address.")
            .MaximumLength(255)
                .WithMessage("Admin email cannot exceed 255 characters.");

        RuleFor(x => x.AdminFirstName)
            .NotEmpty()
                .WithMessage("Admin first name is required.")
            .MinimumLength(1)
                .WithMessage("Admin first name must be at least 1 character.")
            .MaximumLength(100)
                .WithMessage("Admin first name cannot exceed 100 characters.");

        RuleFor(x => x.AdminLastName)
            .NotEmpty()
                .WithMessage("Admin last name is required.")
            .MinimumLength(1)
                .WithMessage("Admin last name must be at least 1 character.")
            .MaximumLength(100)
                .WithMessage("Admin last name cannot exceed 100 characters.");

        // ✅ FIX: Match the actual region format used in Tenant.Create (USEast, not us-east-1)
        RuleFor(x => x.Region)
            .Must(region => BeValidRegion(region))
                .When(x => !string.IsNullOrWhiteSpace(x.Region))
                .WithMessage("Region must be a valid region code (e.g., 'USEast', 'USWest', 'EUWest').");
    }

    private bool NotContainReservedWords(string subdomain)
    {
        if (string.IsNullOrWhiteSpace(subdomain))
            return true;

        var reservedWords = new[]
        {
            "admin", "api", "app", "www", "mail", "ftp", "localhost",
            "staging", "dev", "test", "prod", "production", "demo",
            "support", "help", "docs", "blog", "status", "security",
            "apex", "system", "root", "superuser"
        };

        return !reservedWords.Contains(subdomain.ToLowerInvariant());
    }

    private bool BeValidRegion(string? region)
    {
        if (string.IsNullOrWhiteSpace(region))
            return true; // Null/empty is valid (uses default "USEast")

        // ✅ Match the actual region format from Tenant.Create()
        var validRegions = new[]
        {
            "USEast", "USWest", "USCentral",
            "EUWest", "EUCentral", "EUNorth",
            "APSoutheast", "APNortheast"
        };

        return validRegions.Contains(region);
    }
}