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


    [KernelFunction("GetLocationLatLong")]
    [Description("Determine the location latitude and longitude based on user request")]
    [return: Description("A location point consisting of a latitude and longitude")]
    public async Task<LocationPoint> DetermineDamageAsync([Description("data url of auto damage")] string dataUrlImage,
                                                           KernelArguments arguments,
                                                           Kernel kernel)
    {
        var chatGpt = kernel.Services.GetService<IChatCompletionService>();
        ArgumentNullException.ThrowIfNull(chatGpt, nameof(chatGpt));

        var chatHistory = new ChatHistory(PromptService.GetPromptByName("WeatherLatLongSystemPrompt"));
        //chatHistory.AddUserMessage(WeatherLocation);

        var searchAnswer = await chatGpt.GetChatMessageContentAsync(chatHistory, DefaultSettings.AISearchRequestSettings, kernel);

        var parts = searchAnswer.Content.Split(',');
        var lp = new LocationPoint { Latitude = parts[0].Trim(), Longitude = parts[1].Trim() };
        arguments["LocationPoint"] = lp;
        
        return lp;
    }
}