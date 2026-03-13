using CoreCodeCamp.Data;
using System;
using System.Linq;

namespace CoreCodeCamp.Services;

public class CampService(ICampRepository repository, ILogger<CampService> logger) : ICampService
{
    private readonly ICampRepository _repository = repository;
    private readonly ILogger<CampService> _logger = logger;

    public async Task<Camp[]> GetAllCampsAsync()
    {
        try
        {
            var camps = await _repository.GetAllCampsAsync();
            return camps ?? Array.Empty<Camp>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving camps");
            return Array.Empty<Camp>();
        }
    }

    public async Task<Camp?> GetCampAsync(string city)
    {
        _logger.LogInformation("Getting camp for {City}", city);
        return await _repository.GetCampAsync(city);
    }

    public async Task<Camp> CreateCampAsync(CreateCampRequest request)
    {
        _logger.LogInformation("Creating camp {City}", request.City);

        // If client doesn't provide a LocationId it will default to 1 (see CreateCampRequest)
        var location = await _repository.GetLocationByIdAsync(request.LocationId)
          ?? throw new ArgumentException("Location does not exist.");

        var camp = CampServiceMapper.ToCamp(request);
        camp.Location = location;

        _repository.Add(camp);
        await _repository.SaveChangesAsync();

        _logger.LogInformation("Camp {City} created successfully", camp.City);
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

        return await _repository.SaveChangesAsync();
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
