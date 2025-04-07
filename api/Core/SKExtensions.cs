using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using MinimalApi.Services.Search;

namespace Assistants.API.Core
{
    public static class SKExtensions
    {
        public static ChatHistory AddChatHistory(this ChatHistory chatHistory, ChatTurn[] history)
        {
            foreach (var chatTurn in history.SkipLast(1))
            {
                chatHistory.AddUserMessage(chatTurn.User);
                if (chatTurn.Assistant != null)
                {
                    chatHistory.AddAssistantMessage(chatTurn.Assistant);
                }
            }

            return chatHistory;
        }

        public static void AddFunctionCallResult(this Kernel kernel, string name, string result, List<KnowledgeSource> sources = null)
        {
            var diagnosticsBuilder = GetRequestDiagnosticsBuilder(kernel);
            diagnosticsBuilder.AddFunctionCallResult(name, result, sources);
        }
        public static RequestDiagnosticsBuilder GetRequestDiagnosticsBuilder(this Kernel kernel)
        {
            if (!kernel.Data.ContainsKey("DiagnosticsBuilder"))
            {
                var diagnosticsBuilder = new RequestDiagnosticsBuilder();
                kernel.Data.Add("DiagnosticsBuilder", diagnosticsBuilder);
                return diagnosticsBuilder;
            }

            return kernel.Data["DiagnosticsBuilder"] as RequestDiagnosticsBuilder;
        }
        public static IEnumerable<ExecutionStepResult> GetFunctionCallResults(this Kernel kernel)
        {
            if (kernel.Data.ContainsKey("DiagnosticsBuilder"))
            {
                var diagnosticsBuilder = kernel.Data["DiagnosticsBuilder"] as RequestDiagnosticsBuilder;
                foreach (var item in diagnosticsBuilder.FunctionCallResults)
                {
                    yield return item;
                }
            }
        }

        public static IEnumerable<ExecutionStepResult> GetThoughtProcess(this Kernel kernel, string systemPrompt, string answer)
        {
            var functionCallResults = kernel.GetFunctionCallResults();
            foreach (var item in functionCallResults)
            {
                yield return item;
            }
            yield return new ExecutionStepResult("chat_completion", $"{systemPrompt} \n {answer}");
        }

        public static string ReadFileContent(this string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentException("File name cannot be null or empty.", nameof(fileName));

            if (!File.Exists(fileName))
                throw new FileNotFoundException("File not found.", fileName);

            return File.ReadAllText(fileName);
        }
    }
}
