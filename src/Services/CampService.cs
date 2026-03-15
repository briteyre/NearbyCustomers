using CoreCodeCamp.Data;
using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;


namespace CoreCodeCamp.Services;

public class CampService(ICampRepository repository, ILogger<CampService> logger, IDistributedCache cache, IOptions<CacheSettings> cacheSettings) : ICampService
{
    private readonly ICampRepository _repository = repository;
    private readonly ILogger<CampService> _logger = logger;
    private readonly IDistributedCache _cache = cache;
    private readonly CacheSettings _cacheSettings = cacheSettings?.Value ?? new CacheSettings();

    private const string AllCampsKey = "Camps:All";
    private static string GetCampKey(string city) => $"Camps:City:{city}";

    public async Task<Camp[]> GetAllCampsAsync()
    {
        try
        {
            var cached = await _cache.GetStringAsync(AllCampsKey).ConfigureAwait(false);
            if (!string.IsNullOrEmpty(cached))
            {
                try
                {
                    var cachedCamps = JsonSerializer.Deserialize<Camp[]>(cached);
                    if (cachedCamps is not null)
                    {
                        return cachedCamps;
                    }
                }
                catch (JsonException)
                {
                    // data is invalid — reload from repository
                }
            }

            var camps = await _repository.GetAllCampsAsync();
            var result = camps ?? Array.Empty<Camp>();

            var json = JsonSerializer.Serialize(result);
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_cacheSettings.AllCampsMinutes)
            };
            await _cache.SetStringAsync(AllCampsKey, json, options).ConfigureAwait(false);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving camps");
            return Array.Empty<Camp>();
        }
    }

    public async Task<Camp?> GetCampAsync(string city)
    {
        try
        {
            _logger.LogInformation("Getting camp for {City}", city);
            var key = GetCampKey(city);
            var cached = await _cache.GetStringAsync(key).ConfigureAwait(false);
            if (!string.IsNullOrEmpty(cached))
            {
                try
                {
                    var cachedCamp = JsonSerializer.Deserialize<Camp>(cached);
                    if (cachedCamp is not null)
                    {
                        return cachedCamp;
                    }
                }
                catch (JsonException)
                {
                    // data is invalid — reload from repository
                }
            }

            var camp = await _repository.GetCampAsync(city);
            if (camp is not null)
            {
                var json = JsonSerializer.Serialize(camp);
                var options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_cacheSettings.CampMinutes)
                };
                await _cache.SetStringAsync(key, json, options).ConfigureAwait(false);
            }

            return camp;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving camp for {City}", city);
            return null;
        }
    }

    public async Task<Camp> CreateCampAsync(CreateCampRequest request)
    {
        _logger.LogInformation("Creating camp {City}", request.City);

        var location = await _repository.GetLocationByIdAsync(request.LocationId)
          ?? throw new ArgumentException("Location does not exist.");

        var camp = CampServiceMapper.ToCamp(request);
        camp.Location = location;

        _repository.Add(camp);
        await _repository.SaveChangesAsync();

        // Invalidate caches for camps
        await _cache.RemoveAsync(AllCampsKey).ConfigureAwait(false);
        await _cache.RemoveAsync(GetCampKey(camp.City)).ConfigureAwait(false);

        _logger.LogInformation("Camp for city {City} created successfully", camp.City);
        return camp;
    }

    public async Task<bool> UpdateCampAsync(string city, UpdateCampRequest request)
    {
        _logger.LogInformation("Updating camp {City}", city);

        var camp = await _repository.GetCampAsync(city);
        if (camp == null)
        {
            _logger.LogWarning("Camp {City} not found", city);
            return false;
        }

        CampServiceMapper.UpdateCamp(request, camp);

        var result = await _repository.SaveChangesAsync();
        if (result)
        {
            // Invalidate caches for updated camp
            await _cache.RemoveAsync(AllCampsKey).ConfigureAwait(false);
            await _cache.RemoveAsync(GetCampKey(city)).ConfigureAwait(false);
        }

        return result;
    }

    public async Task<bool> DeleteSpeakerByNameAsync(string firstName, string lastName)
    {
        _logger.LogInformation("Deleting speaker {FirstName} {LastName}", firstName, lastName);

        var speakers = await _repository.GetAllSpeakersAsync();
        var speaker = speakers?.FirstOrDefault(s =>
            string.Equals(s.FirstName, firstName, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(s.LastName, lastName, StringComparison.OrdinalIgnoreCase));

        if (speaker == null)
        {
            _logger.LogWarning("Speaker {FirstName} {LastName} not found", firstName, lastName);
            return false;
        }

        _repository.Delete(speaker);
        return await _repository.SaveChangesAsync();
    }

    public async Task<Speaker> CreateSpeakerAsync(CreateSpeakerRequest request)
    {
        _logger.LogInformation("Creating speaker {FirstName} {LastName}", request.FirstName, request.LastName);

        var speaker = CampServiceMapper.ToSpeaker(request);

        _repository.Add(speaker);
        await _repository.SaveChangesAsync();

        _logger.LogInformation("Speaker {FirstName} {LastName} created with ID {SpeakerId}", speaker.FirstName, speaker.LastName, speaker.SpeakerId);
        return speaker;
    }

    public async Task<Speaker[]> GetAllSpeakersAsync()
    {
        _logger.LogInformation("Getting all speakers");
        var speakers = await _repository.GetAllSpeakersAsync();
        return speakers ?? Array.Empty<Speaker>();
    }

    public async Task<bool> UpdateSpeakerAsync(int speakerId, UpdateSpeakerRequest request)
    {
        _logger.LogInformation("Updating speaker {SpeakerId}", speakerId);

        var speaker = await _repository.GetSpeakerAsync(speakerId);
        if (speaker == null)
        {
            _logger.LogWarning("Speaker {SpeakerId} not found", speakerId);
            return false;
        }

        CampServiceMapper.UpdateSpeaker(request, speaker);

        return await _repository.SaveChangesAsync();
    }
}
