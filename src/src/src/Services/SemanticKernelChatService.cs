using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace CoreCodeCamp.Services;

public interface ISemanticKernelChatService
{
    Task<string> ChatAsync(string message);
}

public class SemanticKernelChatService(Kernel kernel) : ISemanticKernelChatService
{
    private readonly Kernel _kernel = kernel;

    public async Task<string> ChatAsync(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            return string.Empty;

#pragma warning disable SKEXP0001
        var settings = new OpenAIPromptExecutionSettings
        {
            ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
        };
#pragma warning restore SKEXP0001

        var result = await _kernel.InvokePromptAsync(message, new KernelArguments(settings));

        return result.ToString();
    }
}
