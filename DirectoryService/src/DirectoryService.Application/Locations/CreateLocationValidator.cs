using DirectoryService.Application.Shared.Validation;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Shared;
using FluentValidation;

namespace DirectoryService.Application.Locations;

public class CreateLocationValidator : AbstractValidator<CreateLocationCommand>
{
    public CreateLocationValidator()
    {
        RuleFor(x => x.CreateLocationDto).NotNull().WithError(GeneralErrors.ValueIsRequired("CreateLocationDto"));

        RuleFor(x => x.CreateLocationDto.Name).MustBeValueObject(LocationName.Create);
        RuleFor(x => x.CreateLocationDto.Timezone).MustBeValueObject(Timezone.Create);
        
        RuleFor(x => x.CreateLocationDto.Address).MustBeValueObject(a => Address.Create(a.City, a.Street, a.HouseNumber));
    }
}