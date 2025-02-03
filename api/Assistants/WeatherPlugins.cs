using System.ComponentModel;
using Assistants.API.Core;
using Assistants.API.Services.Prompts;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MinimalApi.Services.Skills;

public class WeatherPlugins
{
    private readonly IHttpClientFactory _httpClientFactory;

    public WeatherPlugins(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    [KernelFunction("GetForecast")]
    [Description("Get weather forecast for a specified location point")]
    [return: Description("A weather forecast in JSON format")]
    public async Task<string> RetrieveWeatherForecastAsync(
        [Description("Location coordinates")] LocationPoint locationPoint,
        KernelArguments arguments)
    {
        using var httpClient = _httpClientFactory.CreateClient("WeatherAPI");
        httpClient.DefaultRequestHeaders.Add("User-Agent", "app");

        // Get forecast URL
        var response = await httpClient.GetAsync($"points/{locationPoint.Latitude},{locationPoint.Longitude}");
        response.EnsureSuccessStatusCode();

        var responseBody = await response.Content.ReadAsStringAsync();
        var json = JObject.Parse(responseBody);

        if (json["properties"]?["forecast"]?.ToString() is not string forecastUrl)
        {
            throw new InvalidOperationException("Invalid forecast response format");
        }

        // Get forecast data
        var forecastResponse = await httpClient.GetAsync(forecastUrl);
        forecastResponse.EnsureSuccessStatusCode();

        var forecastResponseBody = await forecastResponse.Content.ReadAsStringAsync();
        arguments["WeatherForecast"] = forecastResponseBody;

        return forecastResponseBody;
    }

    [KernelFunction("GetLocationLatLong")]
    [Description("Determine latitude and longitude from a location description")]
    [return: Description("Location coordinates with latitude and longitude")]
    public async Task<LocationPoint> DetermineLatLongAsync(
        [Description("Location name or zip code")] string weatherLocation,
        KernelArguments arguments,
        Kernel kernel)
    {
        var chatGpt = kernel.Services.GetService<IChatCompletionService>();
        var chatHistory = new ChatHistory(PromptService.GetPromptByName("WeatherLatLongSystemPrompt"));
        chatHistory.AddUserMessage(weatherLocation);

        var searchAnswer = await chatGpt.GetChatMessageContentAsync(
            chatHistory, DefaultSettings.AISearchRequestSettings, kernel);

        var parts = searchAnswer.Content.Split(',');
        if (parts.Length != 2)
        {
            throw new ArgumentException(
                "Invalid location format. Expected 'latitude, longitude'.");
        }

        var lp = new LocationPoint
        {
            Latitude = parts[0].Trim(),
            Longitude = parts[1].Trim()
        };

        arguments["LocationPoint"] = lp;
        return lp;
    }

    [KernelFunction("GetLocation")]
    [Description("Extract location from conversation history")]
    [return: Description("Identified location name or zip code")]
    public async Task<string> DetermineLocationAsync(
        [Description("Chat history for context")] ChatTurn[] chatMessages,
        KernelArguments arguments,
        Kernel kernel)
    {
        var chatGpt = kernel.Services.GetService<IChatCompletionService>();
        var systemPrompt = PromptService.GetPromptByName("WeatherLocationSystemPrompt");
        var chatHistory = new ChatHistory(systemPrompt).AddChatHistory(chatMessages);

        var userMessage = await PromptService.RenderPromptAsync(
            kernel, "{{$input}}", arguments);
        chatHistory.AddUserMessage(userMessage);

        var searchAnswer = await chatGpt.GetChatMessageContentAsync(
            chatHistory, DefaultSettings.AISearchRequestSettings, kernel);

        arguments["WeatherLocation"] = searchAnswer.Content;
        return searchAnswer.Content;
    }
}

public class LocationPoint
{
    [JsonProperty("latitude")]
    public string Latitude { get; set; }

    [JsonProperty("longitude")]
    public string Longitude { get; set; }
}