using DirectoryService.Domain.Locations;
using FluentValidation;

namespace DirectoryService.Application.Locations;

public class CreateLocationValidator : AbstractValidator<CreateLocationCommand>
{
    public CreateLocationValidator()
    {
        RuleFor(x => x.CreateLocationDto).NotNull().WithMessage("CreateLocationDto = null");

        RuleFor(x => x.CreateLocationDto.Name).MinimumLength(Constants.MIN_LOCATION_NAME_LENGTH).WithMessage("");
        RuleFor(x => x.CreateLocationDto.Name).MaximumLength(Constants.MAX_LOCATION_NAME_LENGTH).WithMessage("");
        
        RuleFor(x => x.CreateLocationDto.Name)
    }
}