using Microsoft.SemanticKernel;

namespace Assistants.API.Core
{
    public record ChatChunkResponse(string Text, ChatChunkResponseResult? FinalResult = null);
    public record ChatChunkResponseResult(string Answer, string? Error = null);

    public record class ChatRequest(Guid ChatId, Guid ChatTurnId, ChatMessageContent[] ChatMessageContent, Dictionary<string, string> OptionFlags);

    public record ChatTurn(string User, string? Assistant = null);

}
