using System.ComponentModel;
using System.Text.Json.Serialization;
using Assistants.API.Core;
using Assistants.API.Services.Prompts;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Newtonsoft.Json.Linq;

namespace MinimalApi.Services.Skills;

public class WeatherPlugins
{
    [KernelFunction("GetForcast")]
    [Description("Determine the location latitude and longitude based on user request")]
    [return: Description("A weather forcast")]
    public async Task<string> RetrieveWeatherForcastAsync([Description("chat History")] ChatTurn[] chatMessages,
                                                  LocationPoint LocationPoint,
                                                  KernelArguments arguments,
                                                  Kernel kernel)
    {
        string url = $"https://api.weather.gov/points/{LocationPoint.Latitude},{LocationPoint.Longitude}";

        var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Add("User-Agent", "app");
        HttpResponseMessage response = await httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();
        // Parse the response body
        string responseBody = await response.Content.ReadAsStringAsync();
        JObject json = JObject.Parse(responseBody);

        // Extract the forecast URL from the JSON response
        string forecastUrl = json["properties"]["forecast"].ToString();

        HttpResponseMessage forecastResponse = await httpClient.GetAsync(forecastUrl);
        forecastResponse.EnsureSuccessStatusCode();
        string forecastResponseBody = await forecastResponse.Content.ReadAsStringAsync();
        arguments["WeatherForcast"] = forecastResponseBody;
        return forecastResponseBody;
    }

    [KernelFunction("GetLocationLatLong")]
    [Description("Determine the location latitude and longitude based on user request")]
    [return: Description("A location point consisting of a latitude and longitude")]
    public async Task<LocationPoint> DetermineLatLongAsync([Description("chat History")] ChatTurn[] chatMessages,
                                                           string WeatherLocation,
                                                           KernelArguments arguments,
                                                           Kernel kernel)
    {
        var chatGpt = kernel.Services.GetService<IChatCompletionService>();

        var chatHistory = new ChatHistory(PromptService.GetPromptByName("WeatherLatLongSystemPrompt"));
        chatHistory.AddUserMessage(WeatherLocation);

        var searchAnswer = await chatGpt.GetChatMessageContentAsync(chatHistory, DefaultSettings.AISearchRequestSettings, kernel);

        var parts = searchAnswer.Content.Split(',');
        var lp = new LocationPoint { Latitude = parts[0].Trim(), Longitude = parts[1].Trim() };
        arguments["LocationPoint"] = lp;
        
        return lp;
    }

    [KernelFunction("GetLocation")]
    [Description("Determine the location based on user request")]
    [return: Description("A location in the form of a city or zip code.")]
    public async Task<string> DetermineLocationAsync([Description("chat History")] ChatTurn[] chatMessages,
                                                     KernelArguments arguments,
                                                     Kernel kernel)
    {
        var chatGpt = kernel.Services.GetService<IChatCompletionService>();

        var sp = PromptService.GetPromptByName("WeatherLocationSystemPrompt");
        var chatHistory = new ChatHistory(sp).AddChatHistory(chatMessages);

        var userMessage = await PromptService.RenderPromptAsync(kernel, "{{$question}}", arguments);
        chatHistory.AddUserMessage(userMessage);

        var searchAnswer = await chatGpt.GetChatMessageContentAsync(chatHistory, DefaultSettings.AISearchRequestSettings, kernel);
        arguments["WeatherLocation"] = searchAnswer.Content;

        return searchAnswer.Content;
    }
}

public class LocationPoint
{
    [JsonPropertyName("latitude")]
    public string Latitude { get; set; }

    [JsonPropertyName("longitude")]
    public string Longitude { get; set; }
}