using FluentValidation;
using Apex.API.UseCases.Requests.Assign;

namespace Apex.API.UseCases.Requests.Validators;

/// <summary>
/// Validator for AssignRequestCommand
/// </summary>
public class AssignRequestCommandValidator : AbstractValidator<AssignRequestCommand>
{
    public AssignRequestCommandValidator()
    {
        RuleFor(x => x.AssignedToUserId)
            .NotEmpty()
                .WithMessage("AssignedToUserId is required.")
            .NotEqual(Guid.Empty)
                .WithMessage("AssignedToUserId cannot be empty.");
    }
}
