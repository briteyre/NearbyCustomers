using FluentValidation;

namespace CoreCodeCamp.Services.Validators;

public class UpdateCampRequestValidator : AbstractValidator<UpdateCampRequest>
{
    public UpdateCampRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Length).GreaterThanOrEqualTo(0);
        RuleFor(x => x.EventDate).NotNull().WithMessage("EventDate is required.");
    }
}
