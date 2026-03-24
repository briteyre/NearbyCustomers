using System.ComponentModel;
using System.Text;
using Microsoft.SemanticKernel;
using CoreCodeCamp.Data;

namespace CoreCodeCamp.Services.Plugins;

public class SpeakerMatcherPlugin(ICampRepository repository)
{
    private readonly ICampRepository _repository = repository;

    [KernelFunction]
    [Description("Finds speakers matching specific expertise, topics, or companies")]
    public async Task<string> FindSpeakersByTopic(
        [Description("Topic, expertise area, technology, or company name to search for")] string topic,
        [Description("Maximum number of speakers to return")] int limit = 5)
    {
        if (string.IsNullOrWhiteSpace(topic))
            return "No topic provided. Please specify a topic or expertise area.";

        var allSpeakers = await _repository.GetAllSpeakersAsync();
        if (allSpeakers == null || allSpeakers.Length == 0)
            return "No speakers found in the database.";

        var matchedSpeakers = ScoreSpeakers(allSpeakers, topic)
            .OrderByDescending(x => x.Score)
            .Take(limit)
            .Where(x => x.Score > 0)
            .ToList();

        if (!matchedSpeakers.Any())
            return $"No speakers found matching '{topic}'. Try a different search term.";

        var result = new StringBuilder();
        result.AppendLine($"SPEAKERS MATCHING '{topic.ToUpper()}':");
        result.AppendLine();

        for (int i = 0; i < matchedSpeakers.Count; i++)
        {
            var match = matchedSpeakers[i];
            result.AppendLine($"[{i + 1}] {match.Speaker.FirstName} {match.Speaker.LastName} (Match: {match.Score})");
            
            if (!string.IsNullOrWhiteSpace(match.Speaker.Company))
                result.AppendLine($"    Company: {match.Speaker.Company}");
            
            if (!string.IsNullOrWhiteSpace(match.Speaker.Twitter))
                result.AppendLine($"    Twitter: @{match.Speaker.Twitter}");
            
            if (!string.IsNullOrWhiteSpace(match.Speaker.GitHub))
                result.AppendLine($"    GitHub: {match.Speaker.GitHub}");
            
            if (!string.IsNullOrWhiteSpace(match.Speaker.BlogUrl))
                result.AppendLine($"    Blog: {match.Speaker.BlogUrl}");
            
            result.AppendLine();
        }

        return result.ToString();
    }

    [KernelFunction]
    [Description("Finds speakers available for a specific camp/event")]
    public async Task<string> FindSpeakersForCamp(
        [Description("Camp city name")] string city,
        [Description("Optional topic filter")] string? topicFilter = null,
        [Description("Maximum number of speakers to return")] int limit = 10)
    {
        if (string.IsNullOrWhiteSpace(city))
            return "Camp city is required.";

        var speakers = await _repository.GetSpeakersByCityAsync(city);
        if (speakers == null || speakers.Length == 0)
            return $"No speakers found for camp in {city}.";

        // If a topic filter is provided, score and sort by relevance
        var speakersList = topicFilter != null
            ? ScoreSpeakers(speakers, topicFilter)
                .OrderByDescending(x => x.Score)
                .Select(x => x.Speaker)
                .Take(limit)
                .ToList()
            : speakers.Take(limit).ToList();

        if (!speakersList.Any())
            return $"No speakers found for {city} matching '{topicFilter}'.";

        var result = new StringBuilder();
        result.AppendLine($"SPEAKERS FOR {city.ToUpper()}:");
        if (topicFilter != null)
            result.AppendLine($"(Filtered by: {topicFilter})");
        result.AppendLine();

        for (int i = 0; i < speakersList.Count; i++)
        {
            var speaker = speakersList[i];
            result.AppendLine($"[{i + 1}] {speaker.FirstName} {speaker.LastName}");
            
            if (!string.IsNullOrWhiteSpace(speaker.Company))
                result.AppendLine($"    Company: {speaker.Company}");
            
            if (!string.IsNullOrWhiteSpace(speaker.Twitter))
                result.AppendLine($"    Twitter: @{speaker.Twitter}");
            
            if (!string.IsNullOrWhiteSpace(speaker.GitHub))
                result.AppendLine($"    GitHub: {speaker.GitHub}");
            
            result.AppendLine();
        }

        return result.ToString();
    }

    /// <summary>
    /// Scores speakers based on keyword matches in their profile data.
    /// Returns tuples of (Speaker, Score) for ranking.
    /// </summary>
    private static List<(Speaker Speaker, int Score)> ScoreSpeakers(Speaker[] speakers, string topic)
    {
        var searchTerms = ExtractSearchTerms(topic);
        var results = new List<(Speaker, int)>();

        foreach (var speaker in speakers)
        {
            var score = 0;

            // Build searchable text from speaker profile
            var searchableText = new StringBuilder();
            searchableText.Append(speaker.FirstName.ToLowerInvariant()).Append(" ");
            searchableText.Append(speaker.LastName.ToLowerInvariant()).Append(" ");
            searchableText.Append(speaker.Company?.ToLowerInvariant() ?? "").Append(" ");
            searchableText.Append(speaker.GitHub?.ToLowerInvariant() ?? "").Append(" ");
            searchableText.Append(speaker.Twitter?.ToLowerInvariant() ?? "");

            var text = searchableText.ToString();

            // Score: exact company name match gets more weight
            if (!string.IsNullOrWhiteSpace(speaker.Company) && 
                speaker.Company.Equals(topic, StringComparison.OrdinalIgnoreCase))
            {
                score += 10;
            }

            // Score individual search terms
            foreach (var term in searchTerms)
            {
                if (text.Contains(term))
                    score += 2;
            }

            if (score > 0)
                results.Add((speaker, score));
        }

        return results;
    }

    /// <summary>
    /// Extracts searchable terms from the topic query, filtering out common stop words.
    /// </summary>
    private static List<string> ExtractSearchTerms(string topic)
    {
        var stopWords = new HashSet<string> 
        { 
            "the", "a", "an", "and", "or", "but", "in", "on", "at", "to", "for", 
            "with", "by", "from", "up", "about", "into", "through", "during", "is", "are"
        };

        return topic.ToLowerInvariant()
            .Split(new[] { ' ', '\t', '-', '_', ',' }, StringSplitOptions.RemoveEmptyEntries)
            .Where(t => !stopWords.Contains(t) && t.Length > 2)
            .Distinct()
            .ToList();
    }
}