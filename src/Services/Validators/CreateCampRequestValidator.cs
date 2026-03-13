using FluentValidation;

namespace CoreCodeCamp.Services.Validators;

public class CreateCampRequestValidator : AbstractValidator<CreateCampRequest>
{
    public CreateCampRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.City).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LocationId).GreaterThan(0).WithMessage("LocationId must be provided and greater than zero.");
        RuleFor(x => x.Length).GreaterThanOrEqualTo(0);
        RuleFor(x => x.EventDate).NotNull().WithMessage("EventDate is required.");
    }
}
