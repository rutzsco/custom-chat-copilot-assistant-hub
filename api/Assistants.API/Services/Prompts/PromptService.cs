using Microsoft.SemanticKernel;
using System.Reflection;

namespace Assistants.API.Services.Prompts
{
    public static class PromptService
    {
        //public static string WeatherUserPrompt = "WeatherUserPrompt";
        //public static string ChatUserPrompt = "RAGChatUserPrompt";
        //public static string RAGSearchSystemPrompt = "RAGSearchQuerySystemPrompt";
        //public static string RAGSearchUserPrompt = "RAGSearchUserPrompt";

        //public static string ChatSimpleSystemPrompt = "ChatSimpleSystemPrompt";
        //public static string ChatSimpleUserPrompt = "ChatSimpleUserPrompt";

        public static string GetPromptByName(string prompt)
        {
            var resourceName = $"Assistants.API.Services.Prompts.{prompt}.txt";
            var assembly = Assembly.GetExecutingAssembly();
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                    throw new ArgumentException($"The resource {resourceName} was not found.");

                using (StreamReader reader = new StreamReader(stream))
                    return reader.ReadToEnd();
            }
        }

        public static async Task<string> RenderPromptAsync(Kernel kernel, string prompt, KernelArguments arguments)
        {
            var ptf = new KernelPromptTemplateFactory();
            var pt = ptf.Create(new PromptTemplateConfig(prompt));
            string intentUserMessage = await pt.RenderAsync(kernel, arguments);
            return intentUserMessage;
        }
    }
}
