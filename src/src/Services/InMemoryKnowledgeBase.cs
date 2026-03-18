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

        var terms = query.ToLowerInvariant()
            .Split(new[] { ' ', '\t', '\r', '\n', ',', '.', ';', ':' }, StringSplitOptions.RemoveEmptyEntries);

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

        if (!scored.Any()) scored = _docs.Take(k).Select(d => d.Text).ToList();

        return Task.FromResult(scored as IEnumerable<string>);
    }

    public Task<IEnumerable<KnowledgeDocument>> ListDocumentsAsync()
    {
        var docs = _docs.Select(d => new KnowledgeDocument(d.Id, d.Text)).ToList();
        return Task.FromResult(docs as IEnumerable<KnowledgeDocument>);
    }
}
