using Microsoft.EntityFrameworkCore;

namespace CoreCodeCamp.Data
{
    public class CampRepository : ICampRepository
    {
        private readonly CampContext _context;

        public CampRepository(CampContext context)
        {
            _context = context;
        }

        public void Add<T>(T entity) where T : class
        {
            _context.Add(entity);
        }

        public void Delete<T>(T entity) where T : class
        {
            _context.Remove(entity);
        }

        public async Task<bool> SaveChangesAsync()
        {
            // Only return success if at least one row was changed
            return (await _context.SaveChangesAsync()) > 0;
        }

        public async Task<Camp[]> GetAllCampsByEventDateAsync(DateTime dateTime, bool includeTalks = false)
        {
            IQueryable<Camp> query = _context.Camps
                .Include(c => c.Location);

            if (includeTalks)
            {
                query = query
                  .Include(c => c.Talks)
                  .ThenInclude(t => t.Speaker);
            }

            query = query.OrderByDescending(c => c.EventDate)
              .Where(c => c.EventDate.Date == dateTime.Date);

            return await query.ToArrayAsync();
        }

        public async Task<Camp[]> GetAllCampsAsync(bool includeTalks = false)
        {
            IQueryable<Camp> query = _context.Camps
                .Include(c => c.Location);

            if (includeTalks)
            {
                query = query
                  .Include(c => c.Talks)
                  .ThenInclude(t => t.Speaker);
            }

            query = query.OrderByDescending(c => c.EventDate);

            return await query.ToArrayAsync();
        }

        public async Task<Camp?> GetCampAsync(string city, bool includeTalks = false)
        {
            IQueryable<Camp> query = _context.Camps
                .Include(c => c.Location);

            if (includeTalks)
            {
                query = query.Include(c => c.Talks)
                  .ThenInclude(t => t.Speaker);
            }

            // Query It
            query = query.Where(c => c.City == city);

            return await query.FirstOrDefaultAsync();
        }

        public async Task<Talk[]> GetTalksByCityAsync(string city, bool includeSpeakers = false)
        {
            IQueryable<Talk> query = _context.Talks;

            if (includeSpeakers)
            {
                query = query
                  .Include(t => t.Speaker);
            }

            // Add Query
            query = query
              .Where(t => t.Camp.City == city)
              .OrderByDescending(t => t.Title);

            return await query.ToArrayAsync();
        }

        public async Task<Talk?> GetTalkByCityAsync(string city, int talkId, bool includeSpeakers = false)
        {
            IQueryable<Talk> query = _context.Talks;

            if (includeSpeakers)
            {
                query = query
                  .Include(t => t.Speaker);
            }

            // Add Query
            query = query
              .Where(t => t.TalkId == talkId && t.Camp.City == city);

            return await query.FirstOrDefaultAsync();
        }

        public async Task<Speaker[]> GetSpeakersByCityAsync(string city)
        {
            IQueryable<Speaker> query = _context.Talks
              .Where(t => t.Camp.City == city)
              .Select(t => t.Speaker)
              .Where(s => s != null)
              .OrderBy(s => s.LastName)
              .Distinct();

            return await query.ToArrayAsync();
        }

        public async Task<Speaker[]> GetAllSpeakersAsync()
        {
            var query = _context.Speakers
              .OrderBy(t => t.LastName);

            return await query.ToArrayAsync();
        }


        public async Task<Speaker?> GetSpeakerAsync(int speakerId)
        {
            var query = _context.Speakers
              .Where(t => t.SpeakerId == speakerId);

            return await query.FirstOrDefaultAsync();
        }

        public async Task<Location?> GetLocationByIdAsync(int locationId)
        {
            return await _context.Set<Location>().FindAsync(locationId);
        }
    }
}
