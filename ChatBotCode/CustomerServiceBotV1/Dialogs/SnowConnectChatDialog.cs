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
    public class SnowConnectChatDialog : IDialog<object>
    {
        private string _userSysID;
        private Dictionary<string, string> _userDict = new Dictionary<string, string>();
        //private string[] _authUser = new string[2];
        private Dictionary<string, string> _chatSessionDetails = new Dictionary<string, string>();
        public SnowConnectChatDialog(string sys_id, Dictionary<string, string> uDict)
        {
            this._userSysID = sys_id;
            this._userDict = uDict;
            this._chatSessionDetails.Add("message", null);
            this._chatSessionDetails.Add("Agent_Name", null);
        }
        public Task StartAsync(IDialogContext context)
        {
            try
            {
                context.PostAsync("Establishing connection with agent.... Please wait..");
                ServiceNowActivity serviceNowConnect = new ServiceNowActivity();
                //this._authUser = serviceNowConnect.GetUserCred(_userSysID);
                CreateSessionRequest toast = new CreateSessionRequest() { message = String.Format("Chat Request initiated from {0}, Email ID : {1}, Sys ID : {2} at {3}.", _userDict["Name"], _userDict["Email"], _userSysID, DateTime.Now) };
                this._chatSessionDetails.Add("Group_ID", serviceNowConnect.CreateChatSession(toast));
                context.PostAsync("Session established...");
                do
                {
                    this._chatSessionDetails["Agent_Name"] = serviceNowConnect.GetAgentName(this._chatSessionDetails["Group_ID"]);
                }
                while (this._chatSessionDetails["Agent_Name"] == null);

                context.PostAsync(String.Format("Hello {0}. Hope you are doing well. I am {1}. How may I assist you today ?", this._userDict["Name"], this._chatSessionDetails["Agent_Name"]));

                Thread thread = new Thread(() => {
                    int messageCount = 0;
                    string[] messageFromAgent = new string[3];
                    while (true)
                    {
                        messageFromAgent = serviceNowConnect.GetMessageFromLiveAgent(this._chatSessionDetails["Group_ID"], this._chatSessionDetails["Agent_Name"]);
                        if (messageFromAgent != null)
                        {
                            int value;
                            bool isSuccess = int.TryParse(messageFromAgent[2], out value);
                            if (value > messageCount)
                            {
                                if (messageFromAgent[1] != null)
                                {
                                    if (messageFromAgent[0] == "Session_End")
                                    {
                                        messageCount = 0;
                                        value = 0;
                                        context.PostAsync("Agent has closed the session.");
                                        GetMessageReceivedAsync(context);
    
                                    }
                                    else if (messageFromAgent[0] == "Session_Start_Agent")
                                    {
                                        messageCount = value;
                                    }
                                    else
                                    {
                                        messageCount = value;
                                        context.PostAsync(messageFromAgent[0].ToString());
                                    }
                                }
                            }
                        }
                        Thread.Sleep(2500);
                    }

                });
                thread.IsBackground = true;
                thread.Start();


                context.Wait(MessageReceivedAsync);
            }
            catch (Exception e)
            {
                e.ToString();
            }

            return Task.CompletedTask;
        }

        public async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;

            bool postMessageStatus = false;

            ServiceNowActivity serviceNowConnect = new ServiceNowActivity();
            PostMessageToGroupRquest messageObjToPost = new PostMessageToGroupRquest() { message = activity.Text };
            do
            {
                postMessageStatus = serviceNowConnect.PostChatToLiveAgent(messageObjToPost, this._chatSessionDetails["Group_ID"]);
            }
            while (postMessageStatus == false);

            
            context.Wait(this.MessageReceivedAsync);

        }

        public Task GetMessageReceivedAsync(IDialogContext context)
        {
            context.Done<string>("returnfromSnowConnectChatDialog");
            return Task.CompletedTask;
        }
    }
}