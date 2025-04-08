using System.Data;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using Assistants.API.Core;
using Assistants.API.Services.Prompts;
using Assistants.Hub.API.Assistants.Weather;
using Azure.AI.OpenAI;
using Azure.Core;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using Microsoft.SemanticKernel.Connectors.OpenAI;


namespace MinimalApi.Services;

internal sealed class DataMapperChatService
{
    private readonly ILogger<WeatherChatService> _logger;
    private readonly IConfiguration _configuration;
    private readonly OpenAIClientFacade _openAIClientFacade;

    public DataMapperChatService(OpenAIClientFacade openAIClientFacade, ILogger<WeatherChatService> logger, IConfiguration configuration)
    {
        _openAIClientFacade = openAIClientFacade;
        _logger = logger;
        _configuration = configuration;
    }

    public async IAsyncEnumerable<ChatChunkResponse> ReplyPlannerAsync(ChatTurn[] chatMessages, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var sw = Stopwatch.StartNew();

        // Kernel setup
        var kernel = _openAIClientFacade.BuildKernel("DATAMAPPING");

        var context = new KernelArguments();
        context["chatMessages"] = chatMessages;
        context["question"] = chatMessages.LastOrDefault()?.User;

        // Chat Step
        var chatGpt = kernel.Services.GetService<IChatCompletionService>();
        ArgumentNullException.ThrowIfNull(chatGpt, nameof(chatGpt));

        var chatHistory = new ChatHistory(PromptService.GetPromptByName("DataMapperSystemPrompt"));
        var refernceDataFile = _configuration["InputFile"];
        var userMessage = $"The source suste is SAP and the Target data product is Adavance Shipment Notice (ASN): \n {refernceDataFile.ReadFileContent()}"; 
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
}