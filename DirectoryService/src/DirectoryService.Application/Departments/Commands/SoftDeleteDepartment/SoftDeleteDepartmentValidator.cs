using DirectoryService.Application.Shared.Validation;
using DirectoryService.Domain.Shared;
using FluentValidation;

namespace DirectoryService.Application.Departments.Commands.SoftDeleteDepartment;

public class SoftDeleteDepartmentValidator : AbstractValidator<SoftDeleteDepartmentCommand>
{
    public SoftDeleteDepartmentValidator()
    {
        RuleFor(x => x.DepartmentId).NotNull().WithError(GeneralErrors.ValueIsRequired("DepartmentId"));
    }
}