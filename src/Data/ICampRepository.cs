namespace CoreCodeCamp.Data
{
    public interface ICampRepository
    {
        // General 
        void Add<T>(T entity) where T : class;
        void Delete<T>(T entity) where T : class;
        Task<bool> SaveChangesAsync();

        // Camps
        Task<Camp[]> GetAllCampsAsync(bool includeTalks = false);
        Task<Camp> GetCampAsync(string city, bool includeTalks = false);
        Task<Camp[]> GetAllCampsByEventDateAsync(DateTime dateTime, bool includeTalks = false);

        // Talks
        Task<Talk> GetTalkByCityAsync(string city, int talkId, bool includeSpeakers = false);
        Task<Talk[]> GetTalksByCityAsync(string city, bool includeSpeakers = false);

        // Speakers
        Task<Speaker[]> GetSpeakersByCityAsync(string city);
        Task<Speaker?> GetSpeakerAsync(int speakerId);
        Task<Speaker[]> GetAllSpeakersAsync();

        // Location
        Task<Location?> GetLocationByIdAsync(int locationId);
    }
}