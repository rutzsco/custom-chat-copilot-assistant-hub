

using Microsoft.AspNetCore.Mvc;
using MinimalApi.Services;

namespace Assistants.API.Core
{
    internal static class WebApplicationExtensions
    {
        internal static WebApplication MapApi(this WebApplication app)
        {
            var api = app.MapGroup("api");
            api.MapPost("chat/weather", ProcessWeatherRequest);

            api.MapGet("weather", ProcessWeatherPluginGet);
            return app;
        }

        private static async Task<IResult> ProcessWeatherRequest(ChatTurn[] request, [FromServices] WeatherChatService weatherChatService)
        {
            var r = await weatherChatService.ReplyPlannerAsync(request);
            return Results.Ok(r);
        }

        private static async Task<IResult> ProcessWeatherPluginGet(string latitude, string longitude)
        {
            return Results.Ok("OK");
        }
    }
}
