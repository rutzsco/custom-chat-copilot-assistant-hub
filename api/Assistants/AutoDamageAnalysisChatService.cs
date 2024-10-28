using System.Data;
using System.Diagnostics;
using System.Text;
using Assistants.API.Core;
using Assistants.API.Services.Prompts;
using Azure.AI.OpenAI;
using Azure.Core;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;


namespace MinimalApi.Services;

internal sealed class AutoDamageAnalysisChatService
{
    private readonly ILogger<AutoDamageAnalysisChatService> _logger;
    private readonly IConfiguration _configuration;
    private readonly OpenAIClientFacade _openAIClientFacade;

    public AutoDamageAnalysisChatService(OpenAIClientFacade openAIClientFacade,
        ILogger<AutoDamageAnalysisChatService> logger,
        IConfiguration configuration)
    {
        _openAIClientFacade = openAIClientFacade;
        _logger = logger;
        _configuration = configuration;
    }


    public async IAsyncEnumerable<ChatChunkResponse> ReplyPlannerAsync(ChatTurn[] chatMessages, CancellationToken cancellationToken = default)
    {
        var sw = Stopwatch.StartNew();

        // Kernel setup
        var kernel = _openAIClientFacade.GetKernel(false);

        var context = new KernelArguments();
        context["chatMessages"] = chatMessages;
        context["question"] = chatMessages.LastOrDefault()?.User;

        // Chat Step
        var chatGpt = kernel.Services.GetService<IChatCompletionService>();
        ArgumentNullException.ThrowIfNull(chatGpt, nameof(chatGpt));

        var chatHistory = new ChatHistory(PromptService.GetPromptByName("WeatherChatSystemPrompt"));
        var userMessage = await PromptService.RenderPromptAsync(kernel, PromptService.GetPromptByName("WeatherChatUserPrompt"), context);
        chatHistory.AddUserMessage(userMessage);

        OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new()
        {
            ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions,
            MaxTokens = 1024

        };
  
        var sb = new StringBuilder();
        await foreach (StreamingChatMessageContent responseChunk in chatGpt.GetStreamingChatMessageContentsAsync(chatHistory, openAIPromptExecutionSettings, kernel, cancellationToken))
        {
            if (responseChunk.Content != null)
            {
                sb.Append(responseChunk.Content);
                yield return new ChatChunkResponse(responseChunk.Content);
                await Task.Yield();
            }
        }
        sw.Stop();

        yield return new ChatChunkResponse(string.Empty, new ChatChunkResponseResult(sb.ToString()));
    }
}