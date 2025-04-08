using System.Data;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using Assistants.API.Core;
using Assistants.API.Services.Prompts;
using Azure.AI.OpenAI;
using Azure.Core;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using Microsoft.SemanticKernel.Connectors.OpenAI;


namespace Assistants.Hub.API.Assistants.Weather;

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

    public async IAsyncEnumerable<ChatChunkResponse> ExecuteChatAsync(ChatTurn[] chatMessages, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var sw = Stopwatch.StartNew();

        // Kernel setup
        var kernel = _openAIClientFacade.BuildKernel("Weather");

        var context = new KernelArguments();
        context["chatMessages"] = chatMessages;
        context["question"] = chatMessages.LastOrDefault()?.User;

        // Chat Step
        var chatGpt = kernel.Services.GetService<IChatCompletionService>();
        ArgumentNullException.ThrowIfNull(chatGpt, nameof(chatGpt));

        var chatHistory = new ChatHistory(PromptService.GetPromptByName("WeatherChatSystemPrompt"));
        var userMessage = await PromptService.RenderPromptAsync(kernel, PromptService.GetPromptByName("WeatherChatUserPrompt"), context);
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

        yield return new ChatChunkResponse(string.Empty, new ChatChunkResponseResult(sb.ToString(), null));
    }

    public async IAsyncEnumerable<ChatChunkResponse> ExecuteAgentAsync(ChatTurn[] chatMessages, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var sw = Stopwatch.StartNew();

        // Kernel setup
        var kernel = _openAIClientFacade.BuildKernel("WEATHER");

        // Create the agent
        ChatCompletionAgent agent = new()
        {
            Name = "WeatherAgent",
            Instructions = PromptService.GetPromptByName("WeatherChatSystemPrompt"),
            Kernel = kernel,
            Arguments = new KernelArguments(new OpenAIPromptExecutionSettings()
            {
                FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(),
            }),
        };

        var context = new KernelArguments();
        context["chatMessages"] = chatMessages;
        context["question"] = chatMessages.LastOrDefault()?.User;

        // Generate the agent response(s)
        var sb = new StringBuilder();
        ChatHistoryAgentThread agentThread = new();
        await foreach (StreamingChatMessageContent responseChunk in agent.InvokeStreamingAsync(new ChatMessageContent(AuthorRole.User, chatMessages.LastOrDefault()?.User), agentThread, new() { KernelArguments = context }))
        {
            if (responseChunk.Content != null)
            {
                sb.Append(responseChunk.Content);
                yield return new ChatChunkResponse(responseChunk.Content);
                await Task.Yield();
            }
        }

        yield return new ChatChunkResponse(string.Empty, new ChatChunkResponseResult(sb.ToString(), null));
    }
}