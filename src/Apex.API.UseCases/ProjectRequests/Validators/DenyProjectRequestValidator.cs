using FluentValidation;

namespace Apex.API.UseCases.ProjectRequests.Deny;

/// <summary>
/// Validator for DenyProjectRequestCommand
/// </summary>
public class DenyRequestCommandValidator : AbstractValidator<DenyProjectRequestCommand>
{
    public DenyRequestCommandValidator()
    {
        RuleFor(x => x.Reason)
            .NotEmpty()
                .WithMessage("Denial reason is required.")
            .MinimumLength(10)
                .WithMessage("Denial reason must be at least 10 characters.")
            .MaximumLength(1000)
                .WithMessage("Denial reason cannot exceed 1000 characters.");
    }
}