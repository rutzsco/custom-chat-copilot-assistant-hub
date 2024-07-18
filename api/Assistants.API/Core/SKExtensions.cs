using Microsoft.SemanticKernel.ChatCompletion;

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
    }
}
