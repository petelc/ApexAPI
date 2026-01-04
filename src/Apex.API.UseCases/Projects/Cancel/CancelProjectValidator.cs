using FluentValidation;

namespace Apex.API.UseCases.Projects.Cancel;

/// <summary>
/// Validator for CancelProjectCommand
/// </summary>
public class CancelProjectCommandValidator : AbstractValidator<CancelProjectCommand>
{
    public CancelProjectCommandValidator()
    {
        RuleFor(x => x.ProjectId)
            .NotEmpty()
            .WithMessage("ProjectId is required.");

        RuleFor(x => x.Reason)
            .NotEmpty()
            .WithMessage("Reason is required when cancelling project.")
            .MinimumLength(10)
            .WithMessage("Reason must be at least 10 characters.")
            .MaximumLength(500)
            .WithMessage("Reason cannot exceed 500 characters.");
    }
}
