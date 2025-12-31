using FluentValidation;
using Apex.API.UseCases.Requests.Cancel;

namespace Apex.API.UseCases.Requests.Validators;

/// <summary>
/// Validator for CancelRequestCommand
/// </summary>
public class CancelRequestCommandValidator : AbstractValidator<CancelRequestCommand>
{
    public CancelRequestCommandValidator()
    {
        RuleFor(x => x.RequestId)
            .NotEmpty()
                .WithMessage("RequestId is required.");

    }
}