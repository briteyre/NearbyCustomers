using System.Collections.Generic;
using System.Threading.Tasks;

namespace CoreCodeCamp.Services;

public interface IKnowledgeBase
{
    Task AddDocumentAsync(string id, string text);
    Task<IEnumerable<string>> RetrieveRelevantAsync(string query, int k = 3);
    Task<IEnumerable<KnowledgeDocument>> ListDocumentsAsync();
}
