using FluentValidation;

namespace CoreCodeCamp.Services.Validators;

public class UpdateSpeakerRequestValidator : AbstractValidator<UpdateSpeakerRequest>
{
    public UpdateSpeakerRequestValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.MiddleName).MaximumLength(100);
        RuleFor(x => x.Company).MaximumLength(200);
        RuleFor(x => x.CompanyUrl).Must(BeAValidUrl).When(x => !string.IsNullOrWhiteSpace(x.CompanyUrl)).WithMessage("CompanyUrl must be a valid absolute URL.");
        RuleFor(x => x.BlogUrl).Must(BeAValidUrl).When(x => !string.IsNullOrWhiteSpace(x.BlogUrl)).WithMessage("BlogUrl must be a valid absolute URL.");
        RuleFor(x => x.Twitter).MaximumLength(100);
        RuleFor(x => x.GitHub).MaximumLength(100);
    }

    private bool BeAValidUrl(string? url)
    {
        return Uri.IsWellFormedUriString(url, UriKind.Absolute);
    }
}
