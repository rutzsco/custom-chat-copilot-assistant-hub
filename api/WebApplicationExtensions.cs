using System;
using Assistants.API.Core;
using Microsoft.AspNetCore.Mvc;
using MinimalApi.Services;
using System.Runtime.CompilerServices;

namespace Assistants.API
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

        private static async IAsyncEnumerable<ChatChunkResponse> ProcessWeatherRequest(ChatTurn[] request, [FromServices] WeatherChatService weatherChatService, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            await foreach (var chunk in weatherChatService.ReplyPlannerAsync(request).WithCancellation(cancellationToken))
            {
                yield return chunk;
            }
        }

 
        private static async Task<IResult> ProcessWeatherPluginGet(string latitude, string longitude)
        {
            return Results.Ok("OK");
        }
    }
}
