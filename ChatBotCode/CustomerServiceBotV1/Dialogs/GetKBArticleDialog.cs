using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System.Threading;
using ServiceNowClassLibrary.Models;
using ServiceNowClassLibrary;


namespace AzureChatBot.Dialogs
{
    [Serializable]
    internal class GetKBArticleDialog : IDialog<object>
    {
        private string _userSysID;

        public GetKBArticleDialog(string userSysID)
        {
            _userSysID = userSysID;
        }

        public Task StartAsync(IDialogContext context)
        {
            context.PostAsync("Please type your query in brief...");
            context.Wait(this.MessageReceivedAsync);
            return Task.CompletedTask;
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var message = await result;
            if(message.Text != null)
            {
                ServiceNowActivity activity = new ServiceNowActivity();
                KnowledgeArticle knowledge = new KnowledgeArticle();
                knowledge = activity.GetKBArticle(message.Text);
                if (knowledge != null)
                {
                    //knowledge.result[0].
                    await context.PostAsync("Please visit the following link :");
                    await context.PostAsync("https://dev17720.service-now.com/nav_to.do?uri=%2Fkb_view.do%3Fsys_kb_id%3D" + knowledge.result[0].sys_id);
                }
                else
                {
                    await context.PostAsync("Sorry! Not able to find any article regarding your query");
                }
                context.Done<string>("DoneFormGetKBArticleDialog");
            }
        }
    }
}