using DirectoryService.Application.Shared.Validation;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Shared;
using FluentValidation;

namespace DirectoryService.Application.Departments;

public class CreateDepartmentValidator : AbstractValidator<CreateDepartmentCommand>
{
    public CreateDepartmentValidator()
    {
        RuleFor(x => x.CreateDepartmentDto).NotNull().WithError(GeneralErrors.ValueIsRequired("CreateDepartmentDto"));

        RuleFor(x => x.CreateDepartmentDto.Name).MustBeValueObject(DepartmentName.Create);
        RuleFor(x => x.CreateDepartmentDto.Identifier).MustBeValueObject(DepartmentIdentifier.Create);

        RuleFor(x => x.CreateDepartmentDto.LocationIds)
            .NotEmpty()
            .WithError(GeneralErrors.ValueIsRequired("LocationIds"))
            .Must(ids => ids.Distinct().Count() == ids.Count())
            .WithError(GeneralErrors.Failure("LocationIds not unique"));
    }
}