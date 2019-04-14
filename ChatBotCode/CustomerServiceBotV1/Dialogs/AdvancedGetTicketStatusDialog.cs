using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System.Threading;
using ServiceNowClassLibrary.Models;
using ServiceNowClassLibrary;

namespace LuisBot.Dialogs
{
    [Serializable]
    public class AdvancedGetTicketStatusDialog : IDialog<object>
    {
        private string _userSysID;
        private string _ticketNumber;
        public AdvancedGetTicketStatusDialog(string sys_id, string inci_Number)
        {
            this._userSysID = sys_id;
            this._ticketNumber = inci_Number;
        }

        public Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);
            return Task.CompletedTask;
        }
        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {

        }
    }
}