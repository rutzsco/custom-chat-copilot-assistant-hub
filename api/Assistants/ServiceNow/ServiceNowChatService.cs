using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using Assistants.API.Core;
using Assistants.API.Services.Prompts;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace MinimalApi.Services;

internal sealed class ServiceNowChatService
{
    private readonly ILogger<ServiceNowChatService> _logger;
    private readonly IConfiguration _configuration;
    private readonly OpenAIClientFacade _openAIClientFacade;

    public ServiceNowChatService(OpenAIClientFacade openAIClientFacade,
                                 ILogger<ServiceNowChatService> logger,
                                 IConfiguration configuration)
    {
        _openAIClientFacade = openAIClientFacade;
        _logger = logger;
        _configuration = configuration;
    }

    public async IAsyncEnumerable<ChatChunkResponse> ReplyPlannerAsync(ChatTurn[] chatMessages, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var sw = Stopwatch.StartNew();

        // Kernel setup
        var kernel = _openAIClientFacade.BuildKernel("ServiceNow");

        var context = new KernelArguments();
        context["chatMessages"] = chatMessages;
        context["question"] = chatMessages.LastOrDefault()?.User;

        // Chat Step
        var chatGpt = kernel.Services.GetService<IChatCompletionService>();
        ArgumentNullException.ThrowIfNull(chatGpt, nameof(chatGpt));

        var chatHistory = new ChatHistory(PromptService.GetPromptByName("ServiceNowSystemPrompt"));
        var userMessage = await PromptService.RenderPromptAsync(kernel, PromptService.GetPromptByName("ServiceNowUserPrompt"), context);
        chatHistory.AddUserMessage(userMessage);

        var executionSettings = new OpenAIPromptExecutionSettings { FunctionChoiceBehavior = FunctionChoiceBehavior.Auto() };
        var sb = new StringBuilder();
        await foreach (StreamingChatMessageContent responseChunk in chatGpt.GetStreamingChatMessageContentsAsync(chatHistory, executionSettings, kernel, cancellationToken))
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