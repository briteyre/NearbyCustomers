using System.ComponentModel;
using System.Text;
using Microsoft.SemanticKernel;

namespace CoreCodeCamp.Services.Plugins;

public class KnowledgeBasePlugin(IKnowledgeBase kb)
{
    private readonly IKnowledgeBase _kb = kb;

    [KernelFunction]
    [Description("Retrieves relevant documents from the knowledge base based on a query")]
    public async Task<string> RetrieveDocs(
        [Description("The search query")] string query,
        [Description("Maximum number of documents to retrieve")] int topK = 3)
    {
        if (string.IsNullOrWhiteSpace(query))
            return string.Empty;

        var docs = await _kb.RetrieveRelevantAsync(query, topK);
        var docList = docs
            .Where(d => !string.IsNullOrWhiteSpace(d))
            .Select(d => d.Length > 700 ? d[..700] : d)
            .ToList();

        if (!docList.Any())
            return string.Empty;

        var result = new StringBuilder();
        result.AppendLine("RETRIEVED DOCUMENTS:");
        result.AppendLine();

        for (int i = 0; i < docList.Count; i++)
        {
            result.AppendLine($"[Document {i + 1}]");
            result.AppendLine(docList[i]);
            result.AppendLine();
        }

        return result.ToString();
    }
}
