using System;

namespace CoreCodeCamp.Services;

public class CreateCampRequest
{
    public string Name { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public DateTime? EventDate { get; set; }
    public int Length { get; set; }
    // Default to 1 when not provided by the client
    public int LocationId { get; set; } = 1;
}
