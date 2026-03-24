namespace CoreCodeCamp.Services;

public class InMemoryKnowledgeBase : IKnowledgeBase
{
    private readonly List<(string Id, string Text)> _docs = new();

    public Task AddDocumentAsync(string id, string text)
    {
        if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("id required", nameof(id));
        if (text is null) text = string.Empty;

        _docs.Add((id, text));
        return Task.CompletedTask;
    }

    public Task<IEnumerable<string>> RetrieveRelevantAsync(string query, int k = 3)
    {
        if (k <= 0) k = 3;
        if (string.IsNullOrWhiteSpace(query))
        {
            return Task.FromResult(_docs.Take(k).Select(d => d.Text) as IEnumerable<string>);
        }

        var stopWords = new HashSet<string> { "who", "is", "the", "a", "an", "and", "or", "but", "in", "on", "at", "to", "for", "with", "by", "this", "that", "these", "those", "can", "you", "me", "my", "your", "tell", "more", "about" };
        var terms = query.ToLowerInvariant()
            .Split(new[] { ' ', '\t', '\r', '\n', ',', '.', ';', ':', '?', '!', '(', ')', '[', ']', '\"', '\'' }, StringSplitOptions.RemoveEmptyEntries)
            .Where(t => !stopWords.Contains(t))
            .ToList();

        if (!terms.Any())
        {
            // If only stop words or punctuation were in the query, fall back to literal query search
            terms = new List<string> { query.ToLowerInvariant() };
        }

        var scored = _docs
            .Select(d =>
            {
                var text = d.Text.ToLowerInvariant();
                var score = terms.Sum(t => text.Contains(t) ? 1 : 0);
                return (d.Text, score);
            })
            .Where(x => x.score > 0)
            .OrderByDescending(x => x.score)
            .Take(k)
            .Select(x => x.Text)
            .ToList();

        if (!scored.Any())
        {
            scored = _docs
                .OrderByDescending(d => d.Id)
                .Take(k)
                .Select(d => d.Text)
                .ToList();
        }

        return Task.FromResult(scored.AsEnumerable());
    }

    public Task<IEnumerable<KnowledgeDocument>> ListDocumentsAsync()
    {
        var docs = _docs.Select(d => new KnowledgeDocument(d.Id, d.Text)).ToList();
        return Task.FromResult(docs as IEnumerable<KnowledgeDocument>);
    }
}
