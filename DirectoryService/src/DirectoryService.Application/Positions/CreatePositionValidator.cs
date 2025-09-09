using DirectoryService.Application.Shared.Validation;
using DirectoryService.Domain.Positions;
using DirectoryService.Domain.Shared;
using FluentValidation;

namespace DirectoryService.Application.Positions;

public class CreatePositionValidator : AbstractValidator<CreatePositionCommand>
{
    public CreatePositionValidator()
    {
        RuleFor(x => x.CreatePositionDto).NotNull().WithError(GeneralErrors.ValueIsRequired("CreatePositionDto"));

        RuleFor(x => x.CreatePositionDto.Name).MustBeValueObject(PositionName.Create);
        RuleFor(x => x.CreatePositionDto.Description).MustBeValueObject(PositionDescription.Create);

        RuleFor(x => x.CreatePositionDto.DepartmentIds)
            .NotEmpty()
            .WithError(GeneralErrors.ValueIsRequired("DepartmentIds"))
            .Must(ids => ids.Distinct().Count() == ids.Count())
            .WithError(GeneralErrors.Failure("DepartmentIds not unique"));
    }
}