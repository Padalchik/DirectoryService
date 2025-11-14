using DirectoryService.Application.Shared.Validation;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Shared;
using FluentValidation;

namespace DirectoryService.Application.Locations.CreateLocation;

public class CreateLocationValidator : AbstractValidator<CreateLocationCommand>
{
    public CreateLocationValidator()
    {
        RuleFor(x => x.CreateLocationRequest).NotNull().WithError(GeneralErrors.ValueIsRequired("CreateLocationDto"));

        RuleFor(x => x.CreateLocationRequest.Name).MustBeValueObject(LocationName.Create);
        RuleFor(x => x.CreateLocationRequest.Timezone).MustBeValueObject(Timezone.Create);

        RuleFor(x => x.CreateLocationRequest.Address).MustBeValueObject(a => Address.Create(a.City, a.Street, a.HouseNumber));
    }
}