using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System.Threading;
using ServiceNowClassLibrary;
using ServiceNowClassLibrary.Models;

namespace LuisBot.Dialogs
{
    [Serializable]
    public class SupportDialog : IDialog<object>
    {
        private string _userSysID;
        private Dictionary<string, string> _userDict = new Dictionary<string, string>();
        //private string[] _authUser = new string[2];
        private Dictionary<string, string> _chatSessionDetails = new Dictionary<string, string>();
        public SupportDialog(string sys_id, Dictionary<string, string> uDict)
        {
            this._userSysID = sys_id;
            this._userDict = uDict;
        }
        public Task StartAsync(IDialogContext context)
        {
            //try
            //{
            //    context.PostAsync("Establishing connection with agent.... Please wait..");
            //    ServiceNowActivity serviceNowConnect = new ServiceNowActivity();
            //    //this._authUser = serviceNowConnect.GetUserCred(_userSysID);
            //    CreateSessionRequest toast = new CreateSessionRequest() { message = String.Format("Chat Request initiated from {0}, Email ID : {1}, Sys ID : {2} at {3}.",_userDict["Name"], _userDict["Email"], _userSysID, DateTime.Now) };
            //    this._chatSessionDetails.Add("Group_ID", serviceNowConnect.CreateChatSession(toast));
            //    context.PostAsync("Session established...");
            //    do
            //    {
            //        var messageFromAgent = serviceNowConnect.GetMessageFromLiveAgent(this._chatSessionDetails["Group_ID"]);
            //        if (!this._chatSessionDetails.ContainsKey("Agent_Name"))
            //        {
            //            this._chatSessionDetails.Add("Agent_Name", messageFromAgent[1]);
            //        }
            //        else
            //        {
            //            this._chatSessionDetails["Agent_Name"] = messageFromAgent[1];
            //        }

            //    }
            //    while (this._chatSessionDetails["Agent_Name"] == null || this._chatSessionDetails["Agent_Name"] == "system" || this._chatSessionDetails["Agent_Name"] == "nlp_bot");

            //    context.PostAsync(String.Format("Hello {0}. Hope you are doing well. I am {1}. How may I assist you today ?", this._userDict["Name"], this._chatSessionDetails["Agent_Name"]));
            //    context.Wait(MessageReceivedAsync);
            //}
            //catch(Exception e)
            //{
            //    e.ToString();
            //}
            context.Wait(MessageReceivedAsync);
            return Task.CompletedTask;
        }

            public async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
            {
                var activity = await result as Activity;
                //bool postMessageStatus = false;
                //string[] messageFromAgent = new string[2];
                //ServiceNowActivity serviceNowConnect = new ServiceNowActivity();
                //PostMessageToGroupRquest messageObjToPost = new PostMessageToGroupRquest() { message = activity.Text };
                //do
                //{
                //    postMessageStatus = serviceNowConnect.PostChatToLiveAgent(messageObjToPost, this._chatSessionDetails["Group_ID"]);
                //}
                //while (postMessageStatus == false);

                //do
                //{
                //    messageFromAgent = serviceNowConnect.GetMessageFromLiveAgent(this._chatSessionDetails["Group_ID"]);
                //    if(messageFromAgent[1] == this._chatSessionDetails["Agent_Name"])
                //    {
                //        await context.PostAsync(messageFromAgent[0]);
                //    }
                //    else
                //    {
                //        messageFromAgent[1] = null;
                //    }

                //}
                //while (messageFromAgent[1] == null || messageFromAgent[1] == "system" || messageFromAgent[1] == "nlp_bot");
                context.Wait(this.MessageReceivedAsync);

            }
    }
}