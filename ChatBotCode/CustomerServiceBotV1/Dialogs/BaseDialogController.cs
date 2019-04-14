using System;
using System.Threading.Tasks;
using System.Configuration;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.Dialogs;
using System.Net.Http;
using System.Collections.Generic;
using System.Threading;
using ServiceNowClassLibrary;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Builder.PersonalityChat;
using Microsoft.Bot.Builder.PersonalityChat.Core;
using System.Linq;

namespace LuisBot.Dialogs
{
    [Serializable]
    public class BaseDialogController : LuisDialog<object>
    {
        private string _userSysID;
        private Dictionary<string, string> _userDict = new Dictionary<string, string>();
        private PersonalityChatDialogOptions personalityChatDialogOptions = new PersonalityChatDialogOptions(string.Empty, PersonalityChatPersona.Professional);
        public BaseDialogController(string sys_id, Dictionary<string, string> uDict) : base(new LuisService(new LuisModelAttribute(
            ConfigurationManager.AppSettings["LuisAppId"],
            ConfigurationManager.AppSettings["LuisAPIKey"],
            domain: ConfigurationManager.AppSettings["LuisAPIHostName"])))
        {
            this._userSysID = sys_id;
            this._userDict = uDict;
        }

        //public BaseDialogController(string sys_id, Dictionary<string, string> uDict) : base(new LuisService(new LuisModelAttribute(
        //    "b150772e-a7d0-4f7b-b591-be6f4b6fc451",
        //    "90b712326ca644198f0bd3392a0d6b84",
        //    domain: "westus.api.cognitive.microsoft.com")))
        //{
        //    this._userSysID = sys_id;
        //    this._userDict = uDict;
        //}


        [LuisIntent("CreateTicket")]
        public async Task CreateTicketIntent(IDialogContext context, LuisResult result)
        {
            await this.CreateTicketInitiate(context, result);
            context.Call(new CreateTicketDialog(this._userSysID), this.ResumeAfterNextDialog);
        }

        [LuisIntent("GetStatus")]
        public async Task GetStatusIntent(IDialogContext context, LuisResult result)
        {
            string tkt_Num = null;
            //await this.ShowLuisResult(context, result);
            for (int i = 0; i < result.Entities.Count; i++)
            {
                if (result.Entities[i].Type == "IncidentNumber" || result.Entities[i].Type == "RequestNumber") tkt_Num = result.Entities[i].Entity.ToString().ToUpper();
            }
            context.Call(new BasicGetTicketStatusDialog(this._userSysID, tkt_Num), this.ResumeAfterNextDialog);
        }

        [LuisIntent("GetUserIssue")]
        public async Task GetIssueIntent(IDialogContext context, LuisResult result)
        {
            await this.ShowLuisResult(context, result);
        }

        [LuisIntent("None")]
        public async Task NoneIntent(IDialogContext context, IAwaitable<IMessageActivity> activity, LuisResult result)
        {
            var replyTyping = context.MakeMessage();
            replyTyping.Type = ActivityTypes.Typing;
            var message = await activity as Activity;
            var response = context.MakeMessage();
            var personalityChatService = new PersonalityChatService(this.personalityChatDialogOptions);
            var personalityChatResults = await personalityChatService.QueryServiceAsync(message.Text);
            if (personalityChatDialogOptions.RespondOnlyIfChat && !personalityChatResults.IsChatQuery)
            {
                return;
            }
            string personalityChatResponse = GetResponse(personalityChatResults);
            if (!string.IsNullOrEmpty(personalityChatResponse))
            {
                response.Text = GetResponse(personalityChatResults).ToString();
            }
            else
            {
                response.Text = "Sorry! I do not have any answer for you query right now. Sorry for the inconvenience. \n Please let me know how can I help you further.";
            }
            await context.PostAsync(replyTyping);
            await context.PostAsync(replyTyping);
            await context.PostAsync(response);

            context.Wait(MessageReceived);
        }

        [LuisIntent("Greeting")]
        public async Task GreetingIntent(IDialogContext context, LuisResult result)
        {
            await context.PostAsync($"Hi {this._userDict["Name"]} ! How can I assist you today?");
            context.Wait(MessageReceived);
        }

        [LuisIntent("Cancel")]
        public async Task CancelIntent(IDialogContext context, LuisResult result)
        {
            await context.PostAsync($"OK {this._userDict["Name"]}. I will stop now.");
            context.Wait(MessageReceived);
        }

        [LuisIntent("Help")]
        public async Task HelpIntent(IDialogContext context, LuisResult result)
        {
            //await this.ShowLuisResult(context, result);
            await context.PostAsync("Sure. Transferring your call to live agent... Please keep patience..");
            context.Call(new SnowConnectChatDialog(this._userSysID, this._userDict), this.ResumeAfterNextDialog);
        }

        private async Task ShowLuisResult(IDialogContext context, LuisResult result)
        {
            await context.PostAsync($"You have reached {result.Intents[0].Intent}. You said: {result.Query}");
            context.Done<object>(null);
        }
        private async Task CreateTicketInitiate(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("Please provide a brief description of the issue encountered along with the product name. Eg: Outlook is extremely slow");
        }

        private async Task ResumeAfterNextDialog(IDialogContext context, IAwaitable<object> result)
        {
            //await context.PostAsync("Is there anything else that I can help you with ? Please Respond with only 'Yes' or 'No'...");
            CardAction yesAction = new CardAction()
            {
                DisplayText = "YES",
                Text = "YES",
                Title = "YES",
                Value = "YES"
            };
            CardAction noAction = new CardAction()
            {
                DisplayText = "NO",
                Text = "NO",
                Title = "NO",
                Value = "NO"
            };
            List<CardAction> cardActions = new List<CardAction>();
            cardActions.Add(yesAction);
            cardActions.Add(noAction);
            SuggestedActions suggestedActions = new SuggestedActions()
            {
                Actions = cardActions
            };
            var replyNext = context.MakeMessage();
            replyNext.Text = "Is there anything else that I can help you with ? Please Respond with only 'Yes' or 'No'...";
            replyNext.SuggestedActions = suggestedActions;
            await context.PostAsync(replyNext);
            context.Done<string>("returnfrombasicdialogcontroller");
        }

        public virtual string GetResponse(PersonalityChatResults personalityChatResults)
        {
            var matchedScenarios = personalityChatResults?.ScenarioList;

            string response = string.Empty;

            if (matchedScenarios != null)
            {
                var topScenario = matchedScenarios.FirstOrDefault();

                if (topScenario?.Responses != null && topScenario.Score > this.personalityChatDialogOptions.ScenarioThresholdScore && topScenario.Responses.Count > 0)
                {
                    Random randomGenerator = new Random();
                    int randomIndex = randomGenerator.Next(topScenario.Responses.Count);

                    response = topScenario.Responses[randomIndex];
                }
            }
            return response;
        }
    }
}