using System.Threading.Tasks;
using System.Web.Http;

using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.Dialogs;
using System.Web.Http.Description;
using System.Net.Http;
using Autofac;
using System.Text;
using System.Linq;
using LuisBot.Dialogs;

namespace Microsoft.Bot.Sample.LuisBot
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        /// <summary>
        /// POST: api/Messages
        /// receive a message from a user and send replies
        /// </summary>
        /// <param name="activity"></param>
        [ResponseType(typeof(void))]
        public virtual async Task<HttpResponseMessage> Post([FromBody] Activity activity)
        {
            // check if activity is of type message
            if (activity != null && activity.GetActivityType() == ActivityTypes.Message)
            {
                await Conversation.SendAsync(activity, () => new RootDialog());
            }
            else
            {
                HandleSystemMessage(activity);
            }
            return new HttpResponseMessage(System.Net.HttpStatusCode.Accepted);
        }

        private Activity HandleSystemMessage(Activity message)
        {
            if (message.Type == ActivityTypes.DeleteUserData)
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == ActivityTypes.ConversationUpdate)
            {
                IConversationUpdateActivity update = message;
                using (var scope = Microsoft.Bot.Builder.Dialogs.Internals.DialogModule.BeginLifetimeScope(Conversation.Container, message))
                {
                    var client = scope.Resolve<IConnectorClient>();
                    if (update.MembersAdded.Any())
                    {
                        if (message.MembersAdded[0].Name.ToUpper() != "ALICE" && message.MembersAdded[0].Name.ToUpper() != "CUSTOMERSERVICEBOTV1")
                        //if (message.MembersAdded[0].Name.ToUpper() == "ALICE" || message.MembersAdded[0].Name.ToUpper() == "CUSTOMERSERVICEBOTV1" || message.MembersAdded[0].Name.ToUpper() == "USER")
                        {
                            var reply = message.CreateReply();
                            StringBuilder toastMessage = new StringBuilder();
                            toastMessage.AppendLine($"Welcome to Virtual Service Desk.");
                            toastMessage.AppendLine("I'm Alice, your virtual assistant.");
                            toastMessage.AppendLine("Please note, the conversation is logged for training purposes." +
                                "\nDo not enter sensitive or restricted data (such as passwords or confidential data). \nType 'Hi' to start...");
                            reply.Text = toastMessage.ToString();
                            client.Conversations.ReplyToActivity(reply);
                        }
                    }
                }
            }
            else if (message.Type == ActivityTypes.ContactRelationUpdate)
            {
                // Handle add/remove from contact lists
                // Activity.From + Activity.Action represent what happened
            }
            else if (message.Type == ActivityTypes.Typing)
            {
                // Handle knowing tha the user is typing
            }
            else if (message.Type == ActivityTypes.Ping)
            {
            }

            return null;
        }
    }
}