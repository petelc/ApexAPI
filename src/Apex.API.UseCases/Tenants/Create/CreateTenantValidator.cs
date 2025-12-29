using FluentValidation;
using System.Text.RegularExpressions;

namespace Apex.API.UseCases.Tenants.Create;

/// <summary>
/// Validates CreateTenantCommand
/// </summary>
public class CreateTenantValidator : AbstractValidator<CreateTenantCommand>
{
    public CreateTenantValidator()
    {
        RuleFor(x => x.CompanyName)
            .NotEmpty()
            .WithMessage("Company name is required")
            .MaximumLength(200)
            .WithMessage("Company name cannot exceed 200 characters");

        RuleFor(x => x.Subdomain)
            .NotEmpty()
            .WithMessage("Subdomain is required")
            .Length(3, 63)
            .WithMessage("Subdomain must be between 3 and 63 characters")
            .Matches(@"^[a-z0-9]([a-z0-9-]{0,61}[a-z0-9])?$")
            .WithMessage("Subdomain must contain only lowercase letters, numbers, and hyphens. Cannot start or end with hyphen.")
            .Must(NotBeReservedSubdomain)
            .WithMessage("This subdomain is reserved and cannot be used");

        RuleFor(x => x.AdminEmail)
            .NotEmpty()
            .WithMessage("Admin email is required")
            .EmailAddress()
            .WithMessage("Invalid email address format");

        RuleFor(x => x.AdminFirstName)
            .NotEmpty()
            .WithMessage("Admin first name is required")
            .MaximumLength(100)
            .WithMessage("First name cannot exceed 100 characters");

        RuleFor(x => x.AdminLastName)
            .NotEmpty()
            .WithMessage("Admin last name is required")
            .MaximumLength(100)
            .WithMessage("Last name cannot exceed 100 characters");

        RuleFor(x => x.Region)
            .Must(BeValidRegion)
            .WithMessage("Invalid region. Valid values: USEast, USWest, EUWest, AsiaSoutheast");
    }

    /// <summary>
    /// Reserved subdomains that cannot be used by customers
    /// </summary>
    private static readonly HashSet<string> ReservedSubdomains = new(StringComparer.OrdinalIgnoreCase)
    {
        "www", "api", "admin", "support", "help", "docs", "blog",
        "mail", "smtp", "ftp", "cdn", "app", "dashboard",
        "billing", "account", "login", "signup", "register",
        "apex", "test", "staging", "demo", "dev", "localhost",
        "assets", "static", "media", "images", "files"
    };

    private static bool NotBeReservedSubdomain(string subdomain)
    {
        return !ReservedSubdomains.Contains(subdomain);
    }

    private static bool BeValidRegion(string region)
    {
        var validRegions = new[] { "USEast", "USWest", "EUWest", "AsiaSoutheast" };
        return validRegions.Contains(region, StringComparer.OrdinalIgnoreCase);
    }
}
