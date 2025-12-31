using FluentValidation;
using Apex.API.UseCases.Requests.Create;

namespace Apex.API.UseCases.Requests.Validators;

/// <summary>
/// Validator for CreateRequestCommand
/// </summary>
public class CreateRequestCommandValidator : AbstractValidator<CreateRequestCommand>
{
    public CreateRequestCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
                .WithMessage("Title is required.")
            .MinimumLength(3)
                .WithMessage("Title must be at least 3 characters.")
            .MaximumLength(200)
                .WithMessage("Title cannot exceed 200 characters.");

        RuleFor(x => x.Description)
            .NotEmpty()
                .WithMessage("Description is required.")
            .MinimumLength(10)
                .WithMessage("Description must be at least 10 characters.")
            .MaximumLength(2000)
                .WithMessage("Description cannot exceed 2000 characters.");

        RuleFor(x => x.Priority)
            .Must(BeValidPriority)
                .When(x => !string.IsNullOrWhiteSpace(x.Priority))
                .WithMessage("Priority must be one of: Low, Medium, High, Urgent");

        RuleFor(x => x.DueDate)
            .Must(BeInFuture)
                .When(x => x.DueDate.HasValue)
                .WithMessage("Due date must be in the future.");
    }

    private bool BeValidPriority(string? priority)
    {
        if (string.IsNullOrWhiteSpace(priority))
            return true;

        var validPriorities = new[] { "Low", "Medium", "High", "Urgent" };
        return validPriorities.Contains(priority, StringComparer.OrdinalIgnoreCase);
    }

    private bool BeInFuture(DateTime? dueDate)
    {
        if (!dueDate.HasValue)
            return true;

        return dueDate.Value > DateTime.UtcNow;
    }
}
