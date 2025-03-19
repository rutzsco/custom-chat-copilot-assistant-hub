using Microsoft.SemanticKernel;
using MinimalApi.Services.Search;

namespace Assistants.API.Core
{
    public record ChatChunkResponse(string Text, ChatChunkResponseResult? FinalResult = null);
    public record ChatChunkResponseResult(string Answer, List<ExecutionStepResult> ThoughtProcess, string Error = null);

    public record class ChatRequest(Guid ChatId, Guid ChatTurnId, ChatMessageContent[] ChatMessageContent, Dictionary<string, string> OptionFlags);

    public record ChatTurn(string User, IEnumerable<ChatFile> Files, string? Assistant = null);


    public record ChatFile(string Name, string DataUrl);


    public class RequestDiagnosticsBuilder
    {
        // Aggregate all the function call results
        public List<ExecutionStepResult> FunctionCallResults = new();

        public void AddFunctionCallResult(string name, string result)
        {
            FunctionCallResults.Add(new ExecutionStepResult(name, result));
        }

        public void AddFunctionCallResult(string name, string result, List<KnowledgeSource> sources)
        {
            FunctionCallResults.Add(new ExecutionStepResult(name, result, sources));
        }
    }

    public record ExecutionStepResult(string Name, string Result, List<KnowledgeSource> Sources = null);

}
