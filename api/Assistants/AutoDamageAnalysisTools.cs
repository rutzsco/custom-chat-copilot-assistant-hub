using System.ComponentModel;
using System.Text.Json.Serialization;
using Assistants.API.Core;
using Assistants.API.Services.Prompts;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Newtonsoft.Json.Linq;

namespace MinimalApi.Services.Skills;

public class AutoDamageAnalysisTools
{
    [KernelFunction("GetMakeAndModelAnalysis")]
    [Description("Determine the make and model of vehicle for a given image.")]
    [return: Description("the make and model of the vehicle")]
    public async Task<string> DetermineDamageAsync([Description("image name")] string imageName,
        KernelArguments arguments,
        Kernel kernel)
    {
        var chatGpt = kernel.Services.GetService<IChatCompletionService>();
        ArgumentNullException.ThrowIfNull(chatGpt, nameof(chatGpt));

        var chatHistory = new ChatHistory(PromptService.GetPromptByName("AutoBodyChatSystemPrompt"));
        //chatHistory.AddUserMessage(WeatherLocation);

        var searchAnswer = await chatGpt.GetChatMessageContentAsync(chatHistory, DefaultSettings.AISearchRequestSettings, kernel);
        return searchAnswer.Content;
    }
}