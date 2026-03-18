using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json.Serialization;

namespace CoreCodeCamp.Services;

public class OllamaService(HttpClient http, OllamaSettings settings, IKnowledgeBase? kb = null) : IOllamaService
{
    private readonly HttpClient _http = http;
    private readonly OllamaSettings _settings = settings;
    private readonly IKnowledgeBase? _kb = kb;

    public async Task<string> AskAsync(string prompt, string? model = null)
    {
        var context = new StringBuilder();
        if (_kb is not null)
        {
            var docs = await _kb.RetrieveRelevantAsync(prompt, 2);
            var docList = docs
                .Where(d => !string.IsNullOrWhiteSpace(d))
                .Select(d => d.Length > 700 ? d[..700] : d)
                .ToList();

            if (docList.Any())
            {
                context.AppendLine("CONTEXT:");
                context.AppendLine();

                for (int i = 0; i < docList.Count; i++)
                {
                    context.AppendLine($"[{i + 1}] {docList[i]}");
                    context.AppendLine();
                    context.AppendLine("---");
                    context.AppendLine();
                }

                context.AppendLine("INSTRUCTION: Use the context above as the primary source. If the context is incomplete, you may use your general knowledge. Clearly indicate when part of your answer comes from general knowledge rather than the provided context.");
                context.AppendLine();
            }
        }

        var modelName = string.IsNullOrWhiteSpace(model) ? _settings.Model : model;
        if (string.IsNullOrWhiteSpace(modelName)) modelName = "minimax-m2.5:cloud";

        var fullPrompt = context.Length > 0 ? $"{context}{prompt}" : prompt;

        var req = new OllamaRequest
        {
            Model = modelName,
            Prompt = fullPrompt,
            Stream = false,
            Options = new OllamaOptions
            {
                //NumPredict = 228,
                //Temperature = 0.1,
                //TopP = 0.8,
                //TopK = 20,
                //NumCtx = 1024
            }
        };

        var resp = await _http.PostAsJsonAsync("/api/generate", req);
        if (!resp.IsSuccessStatusCode)
        {
            var bodyText = await resp.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Ollama API error {(int)resp.StatusCode} {resp.ReasonPhrase}: {bodyText}");
        }

        var body = await resp.Content.ReadFromJsonAsync<OllamaResponse>();
        // Prefer the structured "response" property; fall back to "thinking" if present.
        return body?.Response ?? body?.Thinking ?? string.Empty;
    }

    private record OllamaRequest
    {
        [JsonPropertyName("model")]
        public string Model { get; init; } = string.Empty;

        [JsonPropertyName("prompt")]
        public string Prompt { get; init; } = string.Empty;

        [JsonPropertyName("stream")]
        public bool Stream { get; init; } = false;

        [JsonPropertyName("options")]
        public OllamaOptions? Options { get; init; }
    }

    private record OllamaOptions
    {
        [JsonPropertyName("num_predict")]
        public int NumPredict { get; init; } = 128;

        [JsonPropertyName("temperature")]
        public double Temperature { get; init; } = 0.1;

        [JsonPropertyName("top_p")]
        public double TopP { get; init; } = 0.8;

        [JsonPropertyName("top_k")]
        public int TopK { get; init; } = 20;

        [JsonPropertyName("num_ctx")]
        public int NumCtx { get; init; } = 1024;
    }

    private record OllamaResponse
    {
        [JsonPropertyName("model")]
        public string? Model { get; init; }

        [JsonPropertyName("remote_model")]
        public string? RemoteModel { get; init; }

        [JsonPropertyName("remote_host")]
        public string? RemoteHost { get; init; }

        [JsonPropertyName("created_at")]
        public DateTimeOffset? CreatedAt { get; init; }

        [JsonPropertyName("response")]
        public string? Response { get; init; }

        [JsonPropertyName("thinking")]
        public string? Thinking { get; init; }

        [JsonPropertyName("done")]
        public bool? Done { get; init; }

        [JsonPropertyName("done_reason")]
        public string? DoneReason { get; init; }

        [JsonPropertyName("total_duration")]
        public long? TotalDuration { get; init; }

        [JsonPropertyName("prompt_eval_count")]
        public int? PromptEvalCount { get; init; }

        [JsonPropertyName("eval_count")]
        public int? EvalCount { get; init; }
    }
}

