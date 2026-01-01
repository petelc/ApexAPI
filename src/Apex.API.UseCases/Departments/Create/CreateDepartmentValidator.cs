using FluentValidation;

namespace Apex.API.UseCases.Departments.Create;

public class CreateDepartmentCommandValidator : AbstractValidator<CreateDepartmentCommand>
{
    public CreateDepartmentCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
                .WithMessage("Department name is required.")
            .MinimumLength(2)
                .WithMessage("Department name must be at least 2 characters.")
            .MaximumLength(100)
                .WithMessage("Department name cannot exceed 100 characters.");

        RuleFor(x => x.Description)
            .NotEmpty()
                .WithMessage("Description is required.")
            .MaximumLength(500)
                .WithMessage("Description cannot exceed 500 characters.");
    }
}