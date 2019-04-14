using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System.Threading;
using Microsoft.Bot.Builder.FormFlow;
using LuisBot.Models;
using ServiceNowClassLibrary.Models;
using ServiceNowClassLibrary;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;

namespace LuisBot.Dialogs
{
    [Serializable]
    public class CreateTicketDialog : LuisDialog<object>
    {
        private string _userSysID;

        public CreateTicketDialog(string sys_id) : base(new LuisService(new LuisModelAttribute(
            ConfigurationManager.AppSettings["LuisAppId"],
            ConfigurationManager.AppSettings["LuisAPIKey"],
            domain: ConfigurationManager.AppSettings["LuisAPIHostName"])))
        {
            this._userSysID = sys_id;
        }
        //public CreateTicketDialog(string sys_id) : base(new LuisService(new LuisModelAttribute(
        //    "b150772e-a7d0-4f7b-b591-be6f4b6fc451",
        //    "90b712326ca644198f0bd3392a0d6b84",
        //    domain: "westus.api.cognitive.microsoft.com")))
        //{
        //    this._userSysID = sys_id;

        //}

        private CreateIncidentModel _incident = new CreateIncidentModel();
        private int falsecount = 0;

        [LuisIntent("GetUserIssue")]
        public async Task GetIssueIntent(IDialogContext context, LuisResult result)
        {
            if (falsecount == 0)
            {
                for (int i = 0; i < result.Entities.Count(); i++)
                {
                    if (result.Entities[i].Type == "Product")
                    {
                        this._incident.subcategory = result.Entities[0].Entity.ToString();
                        this._incident.short_description = this._incident.short_description + result.Query;
                        break;
                    }
                }

                if (this._incident.subcategory == null)
                {
                    falsecount = 1;
                    await context.PostAsync("Looks like I am unable to capture the Product affected. Request you to specify a valid product name such as Outlook, Internet Explorer, Adobe Acrobat etc.");
                }
                else
                {
                    var createIncidentFormDialog = FormDialog.FromForm(this.BuildCreateIncidentForm, FormOptions.PromptInStart);
                    context.Call(createIncidentFormDialog, this.ResumeAftercreateIncidentFormDialog);
                }
            }
            else
            {
                for (int i = 0; i < result.Entities.Count(); i++)
                {
                    if (result.Entities[i].Type == "Product")
                    {
                        this._incident.subcategory = result.Entities[0].Entity.ToString();
                        this._incident.short_description = this._incident.short_description + " / " + result.Query;
                        break;
                    }
                }
                var createIncidentFormDialog = FormDialog.FromForm(this.BuildCreateIncidentForm, FormOptions.PromptInStart);
                context.Call(createIncidentFormDialog, this.ResumeAftercreateIncidentFormDialog);
            }
        }

        private IForm<CreateIncidentQuery> BuildCreateIncidentForm()
        {
            OnCompletionAsyncDelegate<CreateIncidentQuery> processCreateIncident = async (context, incidentState) =>
            {
                await context.PostAsync("Thank you for the details shared. I'm getting a ticket logged for you right away.");
            };

            return new FormBuilder<CreateIncidentQuery>()
                .Field(nameof(CreateIncidentQuery.Urgency))
                .AddRemainingFields()
                .OnCompletion(processCreateIncident)
                .Build();
        }

        private async Task ResumeAftercreateIncidentFormDialog(IDialogContext context, IAwaitable<CreateIncidentQuery> result)
        {
            try
            {
                var incidentParameter = await result;
                ServiceNowClassLibrary.ServiceNowActivity serviceNowActivity = new ServiceNowClassLibrary.ServiceNowActivity();

                _incident.caller_id = _userSysID;
                if(incidentParameter.Urgency.ToLower().Contains("low")) _incident.urgency = 3;
                else if (incidentParameter.Urgency.ToLower().Contains("medium")) _incident.urgency = 2;
                else if (incidentParameter.Urgency.ToLower().Contains("high")) _incident.urgency = 1;

                if (incidentParameter.ImpactedUser <= 5) _incident.impact = 3;
                else if (incidentParameter.ImpactedUser > 5 && incidentParameter.ImpactedUser <= 20) _incident.impact = 2;
                else if (incidentParameter.ImpactedUser > 20) _incident.impact = 1;

                _incident.comments = "Issue Start Time : "+ incidentParameter.startDateTime.ToLongDateString() + ". Additional Comment by user :" + incidentParameter.additionalComments;

                Dictionary<string, dynamic> incidentOutput = new Dictionary<string, dynamic>();
                incidentOutput = serviceNowActivity.CreateIncident(_incident);

                if(incidentOutput["Success"] == true)
                {
                    string incidentNumber = incidentOutput["Ticket_Number"];
                    await context.PostAsync("An incident has been logged for you with the details provided. A Service Desk Agent will reach out to you shortly to assist you further. This issue can be reference via the case number " + incidentNumber);
                }
                else
                {
                    string error = incidentOutput["Exception"];
                    await context.PostAsync($"Oops! Something went wrong. Cannot create the incident npw. Please try after sometime."); 
                }
                
                
            }
            catch (FormCanceledException ex)
            {
                string reply;

                if (ex.InnerException == null)
                {
                    reply = "Oops! Something went wrong. ";
                }
                else
                {
                    reply = $"Oops! Something went wrong :( Technical Details: {ex.InnerException.Message}";
                }

                await context.PostAsync(reply);
            }
            finally
            {
                context.Done<string>("returnfromcreateticketdialog");
            }
        }

        
    }
}
