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

internal sealed class WeatherChatService
{
    private readonly ILogger<WeatherChatService> _logger;
    private readonly IConfiguration _configuration;
    private readonly OpenAIClientFacade _openAIClientFacade;

    public WeatherChatService(OpenAIClientFacade openAIClientFacade,
                              ILogger<WeatherChatService> logger,
                              IConfiguration configuration)
    {
        _openAIClientFacade = openAIClientFacade;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task<ChatChunkResponse> ReplyStaticAsync(ChatTurn[] chatMessages, CancellationToken cancellationToken = default)
    {
        var sw = Stopwatch.StartNew();

        // Kernel setup
        var kernel = _openAIClientFacade.GetKernel(false);
       

        var s0 = kernel.Plugins.GetFunction("Weather", "GetLocation");
        var s1 = kernel.Plugins.GetFunction("Weather", "GetLocationLatLong");
        var s2 = kernel.Plugins.GetFunction("Weather", "GetForcast");

        var context = new KernelArguments();
        context["chatMessages"] = chatMessages;
        context["question"] = chatMessages.LastOrDefault()?.User;

        // RAG Steps
        await kernel.InvokeAsync(s0, context);
        await kernel.InvokeAsync(s1, context);
        await kernel.InvokeAsync(s2, context);

        // Chat Step
        var chatGpt = kernel.Services.GetService<IChatCompletionService>();


        var chatHistory = new ChatHistory(PromptService.GetPromptByName("WeatherChatSystemPrompt"));
        var userMessage = await PromptService.RenderPromptAsync(kernel, PromptService.GetPromptByName("WeatherChatUserPrompt"), context);
        chatHistory.AddUserMessage(userMessage);

        var sb = new StringBuilder();
        await foreach (StreamingChatMessageContent chatUpdate in chatGpt.GetStreamingChatMessageContentsAsync(chatHistory, DefaultSettings.AIChatRequestSettings, null, cancellationToken))
        {
            if (chatUpdate.Content != null)
            {
                await Task.Delay(1);
                sb.Append(chatUpdate.Content);
            }
        }
        sw.Stop();

        return new ChatChunkResponse(sb.ToString(), null);
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

        yield return new ChatChunkResponse(sb.ToString(), new ChatChunkResponseResult(sb.ToString()));
    }
}