using CoreCodeCamp.Data;
using Riok.Mapperly.Abstractions;

namespace CoreCodeCamp.Services;

[Mapper]
internal static partial class CampServiceMapper
{
    [MapperIgnoreSource(nameof(CreateCampRequest.LocationId))]
    [MapperIgnoreTarget(nameof(Camp.Location))]
    [MapperIgnoreTarget(nameof(Camp.Talks))]
    public static partial Camp ToCamp(CreateCampRequest request);

    public static partial void UpdateCamp(UpdateCampRequest request, Camp camp);

    public static Speaker ToSpeaker(CreateSpeakerRequest request)
    {
        return new Speaker
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            MiddleName = request.MiddleName ?? string.Empty,
            Company = request.Company ?? string.Empty,
            CompanyUrl = request.CompanyUrl,
            BlogUrl = request.BlogUrl,
            Twitter = request.Twitter ?? string.Empty,
            GitHub = request.GitHub ?? string.Empty
        };
    }

    public static partial void UpdateSpeaker(UpdateSpeakerRequest request, Speaker speaker);
}
