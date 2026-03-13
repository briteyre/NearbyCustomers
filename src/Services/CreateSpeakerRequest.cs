namespace CoreCodeCamp.Services;

public class CreateSpeakerRequest
{
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public string? MiddleName { get; set; }
    public string? Company { get; set; }
    public string? CompanyUrl { get; set; }
    public string? BlogUrl { get; set; }
    public string? Twitter { get; set; }
    public string? GitHub { get; set; }
}
