using Microsoft.SemanticKernel;

namespace Assistants.API.Core
{
    public static class DefaultSettings
    {
        public static int MaxRequestTokens = 6000;
        public static int KNearestNeighborsCount = 3;

        public static PromptExecutionSettings AISearchRequestSettings = new()
        {
            ExtensionData = new Dictionary<string, object>()
        {
            { "max_tokens", 100 },
            { "temperature", 0.0 },
            { "top_p", 1 }
        }
        };
        public static PromptExecutionSettings AIChatRequestSettings = new()
        {
            ExtensionData = new Dictionary<string, object>()
        {
            { "max_tokens", 1024 },
            { "temperature", 0.0 },
            { "top_p", 1 },
        }
        };
    }
}
