using FluentValidation;

namespace Apex.API.UseCases.Projects.PutOnHold;

/// <summary>
/// Validator for PutProjectOnHoldCommand
/// </summary>
public class PutProjectOnHoldCommandValidator : AbstractValidator<PutProjectOnHoldCommand>
{
    public PutProjectOnHoldCommandValidator()
    {
        RuleFor(x => x.ProjectId)
            .NotEmpty()
            .WithMessage("ProjectId is required.");

        RuleFor(x => x.Reason)
            .NotEmpty()
            .WithMessage("Reason is required when putting project on hold.")
            .MinimumLength(10)
            .WithMessage("Reason must be at least 10 characters.")
            .MaximumLength(500)
            .WithMessage("Reason cannot exceed 500 characters.");
    }
}
