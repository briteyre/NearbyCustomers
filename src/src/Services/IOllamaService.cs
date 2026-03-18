using System.Threading.Tasks;

namespace CoreCodeCamp.Services;

public interface IOllamaService
{
    Task<string> AskAsync(string prompt, string? model = null);
}
