using System;
using System.Threading.Tasks;
using CoreCodeCamp.Data;

namespace CoreCodeCamp.Services;

public interface ICampService
{
    Task<Camp[]> GetAllCampsAsync();
    Task<Camp?> GetCampAsync(string city);
    Task<Camp> CreateCampAsync(CreateCampRequest request);
    Task<bool> UpdateCampAsync(string city, UpdateCampRequest request);
    Task<Speaker> CreateSpeakerAsync(CreateSpeakerRequest request);
    Task<Speaker[]> GetAllSpeakersAsync();
    Task<bool> UpdateSpeakerAsync(int speakerId, UpdateSpeakerRequest request);
    Task<bool> DeleteSpeakerByNameAsync(string firstName, string lastName);
}
