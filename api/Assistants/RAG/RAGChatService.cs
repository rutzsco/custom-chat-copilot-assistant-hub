using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using Assistants.API.Core;
using Assistants.API.Services.Prompts;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace Assistants.Hub.API.Assistants.RAG;

internal sealed class RAGChatService
{
    private readonly ILogger<RAGChatService> _logger;
    private readonly IConfiguration _configuration;
    private readonly OpenAIClientFacade _openAIClientFacade;

    public RAGChatService(OpenAIClientFacade openAIClientFacade,
                                 ILogger<RAGChatService> logger,
                                 IConfiguration configuration)
    {
        _openAIClientFacade = openAIClientFacade;
        _logger = logger;
        _configuration = configuration;
    }

    public async IAsyncEnumerable<ChatChunkResponse> ReplyPlannerAsync(string agentName, ChatTurn[] chatMessages, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var sw = Stopwatch.StartNew();

        // Kernel setup
        var kernel = _openAIClientFacade.BuildKernel("RAG");
        var chatGpt = kernel.Services.GetService<IChatCompletionService>();
        ArgumentNullException.ThrowIfNull(chatGpt, nameof(chatGpt));
        var agent = AgentDefinitionService.GetAgentDefinition(agentName);
        //var context = new KernelArguments();
        //context["VectorSearchSettings"] = agent.VectorSearchSettings;

        kernel.Data["VectorSearchSettings"] = agent.VectorSearchSettings;

        // Build Chat History
        var chatHistory = new ChatHistory(PromptService.GetPromptByName(agent.SystemPrompt));
        foreach (var turn in chatMessages)
        {
            chatHistory.AddUserMessage(turn.User);
            if(!string.IsNullOrEmpty(turn.Assistant))
                chatHistory.AddAssistantMessage(turn.Assistant);
        }

        // Execute Chat Completion
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