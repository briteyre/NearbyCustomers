namespace CoreCodeCamp.Data
{
    public class Speaker
    {
        public int SpeakerId { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public string MiddleName { get; set; } = string.Empty;
        public string Company { get; set; } = string.Empty;
        public string? CompanyUrl { get; set; }
        public string? BlogUrl { get; set; }
        public string Twitter { get; set; } = string.Empty;
        public string GitHub { get; set; } = string.Empty;

    }
}