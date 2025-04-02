using System;
using Assistants.API.Core;
using Microsoft.AspNetCore.Mvc;
using MinimalApi.Services;
using System.Runtime.CompilerServices;
using Microsoft.Agents.Hosting.AspNetCore;
using Microsoft.Agents.Protocols.Primitives;
using Assistants.Hub.API.Assistants.RAG;
using Assistants.Hub.API.Assistants;
using Microsoft.AspNetCore.Builder;
using Microsoft.SemanticKernel.Services;

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
            api.MapPost("chat/rag/{agentName}", ProcessRagRequest);

            api.MapPost("chat/agent", ProcessAgentRequestV2);

            api.MapGet("status", ProcessStatusGet);
            return app;
        }
        private static async IAsyncEnumerable<ChatChunkResponse> ProcessWeatherRequest(ChatTurn[] request, [FromServices] WeatherChatService weatherChatService, [EnumeratorCancellation] CancellationToken cancellationToken)
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

        private static async IAsyncEnumerable<ChatChunkResponse> ProcessRagRequest(string agentName, ChatTurn[] request, [FromServices] RAGChatService aiService, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            await foreach (var chunk in aiService.ReplyPlannerAsync(agentName, request).WithCancellation(cancellationToken))
            {
                yield return chunk;
            }
        }
        private static async IAsyncEnumerable<ChatChunkResponse> ProcessAgentRequestV2(ChatTurn[] request, [FromServices] AutoAdvisorAgent agent, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            await foreach (var chunk in agent.Execute(request).WithCancellation(cancellationToken))
            {
                yield return chunk;
            }
        }

        private static async Task<IResult> ProcessStatusGet()
        {
            return Results.Ok("OK");
        }

        private static async Task ProcessAgentRequest(HttpRequest request, HttpResponse response,[FromServices] IBotHttpAdapter adapter, IBot bot, CancellationToken cancellationToken)
        {
            await adapter.ProcessAsync(request, response, bot, cancellationToken);
        }
    }
}