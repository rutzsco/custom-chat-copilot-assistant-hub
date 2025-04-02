using Assistants.API.Core;
using Azure;
using Azure.AI.Projects;
using Azure.Identity;

namespace Assistants.Hub.API.Assistants
{
    public class AutoAdvisorAgent
    {
        private readonly IConfiguration _configuration;

        public AutoAdvisorAgent(IConfiguration config)
        {
            _configuration = config;
        }
        public async IAsyncEnumerable<ChatChunkResponse> Execute(ChatTurn[] chatMessages, CancellationToken cancellationToken = default)
        {
            var connectionString = _configuration["AIAgentServiceProjectConnectionString"];
            var agentId = _configuration["AIAgentServiceAgentId"];

            var turn = chatMessages.LastOrDefault();
                      
            AgentsClient client = new AgentsClient("", new DefaultAzureCredential());
            
            var agent = await client.GetAgentAsync(agentId);

            //// Step 2: Create a thread
            Response<AgentThread> threadResponse = await client.CreateThreadAsync();
            AgentThread thread = threadResponse.Value;

            // Step 3: Add a message to a thread
            Response<ThreadMessage> messageResponse = await client.CreateMessageAsync(thread.Id, MessageRole.User, "I need to solve the equation `3x + 11 = 14`. Can you help me?");
            ThreadMessage message = messageResponse.Value;
            Response<PageableList<ThreadMessage>> messagesListResponse = await client.GetMessagesAsync(thread.Id);


            // Step 4: Run the agent
            Response<ThreadRun> runResponse = await client.CreateRunAsync(thread.Id, agent.Value.Id, additionalInstructions: "");
            ThreadRun run = runResponse.Value;

            do
            {
                await Task.Delay(TimeSpan.FromMilliseconds(500));
                runResponse = await client.GetRunAsync(thread.Id, runResponse.Value.Id);
            }
            while (runResponse.Value.Status == RunStatus.Queued || runResponse.Value.Status == RunStatus.InProgress);

            Response<PageableList<ThreadMessage>> afterRunMessagesResponse = await client.GetMessagesAsync(thread.Id);
            IReadOnlyList<ThreadMessage> messages = afterRunMessagesResponse.Value.Data;

            // Note: messages iterate from newest to oldest, with the messages[0] being the most recent
            foreach (ThreadMessage threadMessage in messages)
            {
                foreach (MessageContent contentItem in threadMessage.ContentItems)
                {
                    if (contentItem is MessageTextContent textItem)
                    {
                        yield return new ChatChunkResponse(textItem.Text);
                    }
                }
            }
        }
    }
}
