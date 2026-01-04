using FluentValidation;

namespace Apex.API.UseCases.Projects.Update;

/// <summary>
/// Validator for UpdateProjectCommand
/// </summary>
public class UpdateProjectCommandValidator : AbstractValidator<UpdateProjectCommand>
{
    public UpdateProjectCommandValidator()
    {
        RuleFor(x => x.ProjectId)
            .NotEmpty()
            .WithMessage("ProjectId is required.");

        When(x => !string.IsNullOrWhiteSpace(x.Name), () =>
        {
            RuleFor(x => x.Name)
                .MinimumLength(3)
                .WithMessage("Name must be at least 3 characters.")
                .MaximumLength(200)
                .WithMessage("Name cannot exceed 200 characters.");
        });

        When(x => !string.IsNullOrWhiteSpace(x.Description), () =>
        {
            RuleFor(x => x.Description)
                .MinimumLength(10)
                .WithMessage("Description must be at least 10 characters.")
                .MaximumLength(2000)
                .WithMessage("Description cannot exceed 2000 characters.");
        });

        When(x => x.Budget.HasValue, () =>
        {
            RuleFor(x => x.Budget!.Value)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Budget cannot be negative.");
        });

        When(x => x.StartDate.HasValue && x.EndDate.HasValue, () =>
        {
            RuleFor(x => x)
                .Must(x => x.StartDate!.Value <= x.EndDate!.Value)
                .WithMessage("Start date must be before or equal to end date.");
        });

        When(x => !string.IsNullOrWhiteSpace(x.Priority), () =>
        {
            RuleFor(x => x.Priority)
                .Must(BeValidPriority)
                .WithMessage("Priority must be one of: Low, Medium, High, Urgent");
        });
    }

    private bool BeValidPriority(string? priority)
    {
        if (string.IsNullOrWhiteSpace(priority))
            return true;

        var validPriorities = new[] { "Low", "Medium", "High", "Urgent" };
        return validPriorities.Contains(priority, StringComparer.OrdinalIgnoreCase);
    }
}
