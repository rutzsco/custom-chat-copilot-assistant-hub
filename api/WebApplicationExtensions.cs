﻿using System;
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
            api.MapPost("chat/autobodydamageanalysis", ProcessAutoDamageAnalysis);
            api.MapPost("chat/servicenow", ProcessServiceNowRequest);

            api.MapGet("status", ProcessStatusGet);
            return app;
        }
        private static async IAsyncEnumerable<ChatChunkResponse> ProcessWeatherRequest(ChatTurn[] request, [FromServices] ServiceNowChatService weatherChatService, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            await foreach (var chunk in weatherChatService.ReplyPlannerAsync(request).WithCancellation(cancellationToken))
            {
                yield return chunk;
            }
        }

        private static async IAsyncEnumerable<ChatChunkResponse> ProcessAutoDamageAnalysis (ChatTurn[] request, [FromServices] AutoDamageAnalysisChatService autoDamageAnalysisChatService, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            await foreach (var chunk in autoDamageAnalysisChatService.ReplyPlannerAsync(request).WithCancellation(cancellationToken))
            {
                yield return chunk;
            }
        }

        private static async IAsyncEnumerable<ChatChunkResponse> ProcessServiceNowRequest(ChatTurn[] request, [FromServices] ServiceNowChatService aiService, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            await foreach (var chunk in aiService.ReplyPlannerAsync(request).WithCancellation(cancellationToken))
            {
                yield return chunk;
            }
        }

        private static async Task<IResult> ProcessStatusGet(string latitude, string longitude)
        {
            return Results.Ok("OK");
        }
    }
}