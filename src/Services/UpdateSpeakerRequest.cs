namespace CoreCodeCamp.Services;

public class UpdateSpeakerRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string MiddleName { get; set; } = string.Empty;
    public string Company { get; set; } = string.Empty;
    public string? CompanyUrl { get; set; }
    public string? BlogUrl { get; set; }
    public string Twitter { get; set; } = string.Empty;
    public string GitHub { get; set; } = string.Empty;
}
