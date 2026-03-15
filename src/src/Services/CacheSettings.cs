namespace CoreCodeCamp.Services;

public class CacheSettings
{
    // Absolute expiration in minutes for the cached list of all camps
    public int AllCampsMinutes { get; set; } = 5;

    // Absolute expiration in minutes for a single camp cache entry
    public int CampMinutes { get; set; } = 5;
}
