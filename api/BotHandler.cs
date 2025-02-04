using Microsoft.Agents.Protocols.Adapter;
using Microsoft.Agents.Protocols.Primitives;

namespace Assistants.Hub.API
{
    public class BotHandler : ActivityHandler
    {
        public BotHandler()
        {
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            IActivity message = MessageFactory.Text("AI FLOW!");


            // Send the response message back to the user. 
            await turnContext.SendActivityAsync(message, cancellationToken);
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            // When someone (or something) connects to the bot, a MembersAdded activity is received.
            // For this sample,  we treat this as a welcome event, and send a message saying hello.
            // For more details around the membership lifecycle, please see the lifecycle documentation.
            IActivity message = MessageFactory.Text("Hello. I'm your travel agent. Currently, I am only responsible for the flight tracking updates. I'm here to help!");

            // Send the response message back to the user. 
            await turnContext.SendActivityAsync(message, cancellationToken);
        }
    }
}
