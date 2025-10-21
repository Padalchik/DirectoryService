using DirectoryService.Application.Shared.Validation;
using DirectoryService.Domain.Shared;
using FluentValidation;

namespace DirectoryService.Application.Departments.MoveToDepartment;

public class MoveToDepartmentValidator : AbstractValidator<MoveToDepartmentCommand>
{
    public MoveToDepartmentValidator()
    {
        RuleFor(x => x.MoveToDepartmentDto).NotNull().WithError(GeneralErrors.ValueIsRequired("MoveToDepartmentDto"));

        RuleFor(x => x.DepartmentId)
            .NotEmpty()
            .WithError(GeneralErrors.ValueIsRequired("DepartmentId"));
    }
}