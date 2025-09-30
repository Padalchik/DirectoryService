using DirectoryService.Application.Shared.Validation;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Shared;
using FluentValidation;

namespace DirectoryService.Application.Departments.UpdateLocations;

public class UpdateLocationsValidator : AbstractValidator<UpdateLocationsCommand>
{
    public UpdateLocationsValidator()
    {
        RuleFor(x => x.UpdateLocationsDto).NotNull().WithError(GeneralErrors.ValueIsRequired("UpdateLocationsDto"));

        RuleFor(x => x.UpdateLocationsDto.LocationIds)
            .NotEmpty()
            .WithError(GeneralErrors.ValueIsRequired("LocationIds"))
            .Must(ids => ids.Distinct().Count() == ids.Count())
            .WithError(GeneralErrors.Failure("LocationIds not unique"));
    }
}