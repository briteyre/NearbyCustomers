using System;

namespace CoreCodeCamp.Services;

public class UpdateCampRequest
{
    public string Name { get; set; } = string.Empty;
    public DateTime? EventDate { get; set; }
    public int Length { get; set; }
}
