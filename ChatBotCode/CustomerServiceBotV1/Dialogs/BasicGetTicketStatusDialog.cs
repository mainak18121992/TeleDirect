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
using AdaptiveCards;

namespace LuisBot.Dialogs
{
    [Serializable]
    public class BasicGetTicketStatusDialog : IDialog<object>
    {
        private string _userSysID;
        private string _ticketNumber;
        private int showTicketCounter = 0;
        public BasicGetTicketStatusDialog(string sys_id, string ticket_Number)
        {
            this._userSysID = sys_id;
            this._ticketNumber = ticket_Number;
        }
        public async Task StartAsync(IDialogContext context)
        {
            if (this._ticketNumber == null)
            {
                await context.PostAsync("Sure! Please specify the ticket number to help me look up the details.");
                context.Wait(MessageReceivedAsync);
            }
            else
            {
                if (this._ticketNumber.ToLower().Contains("inc"))
                {
                    ServiceNowActivity serviceNowActivity = new ServiceNowActivity();

                    Dictionary<string, dynamic> incidentStatusOutput = new Dictionary<string, dynamic>();
                    incidentStatusOutput = serviceNowActivity.GetTicketWithStatusByNumber(this._ticketNumber, this._userSysID);

                    if(incidentStatusOutput["Success"] == true)
                    {
                        if (incidentStatusOutput["For_User"] == "Valid")
                        {
                            AdaptiveCard card = new AdaptiveCard("1.0");
                            card.Body.Add(new AdaptiveTextBlock()
                            {
                                Text = "The ticket details are as below :",
                                Weight = AdaptiveTextWeight.Bolder,
                                Wrap = true
                            });

                            GetIncidentModel incidentModel = new GetIncidentModel();
                            incidentModel = incidentStatusOutput["Ticket_Details"];

                            if(incidentModel.result[0].state.ToString() == "2")
                            {
                                card.Body.Add(new AdaptiveTextBlock()
                                {
                                    Text = "The ticket is currently in 'In Progress' state. Our engineers are working on your issue." +
                                    "As soon as I get any update I will let you know.",
                                    Weight = AdaptiveTextWeight.Default,
                                    Wrap = true
                                });
                            }
                            else if (incidentModel.result[0].state.ToString() == "1")
                            {
                                card.Body.Add(new AdaptiveTextBlock()
                                {
                                    Text = "The ticket is currently in 'New' state. Soon your incident will get assigned to corresponding team.",
                                    Weight = AdaptiveTextWeight.Default,
                                    Wrap = true
                                });
                            }
                            
                            if (incidentModel.result[0].work_notes.ToString() != "")
                            {
                                card.Body.Add(new AdaptiveTextBlock()
                                {
                                    Text = "**Work Notes: **" + incidentModel.result[0].work_notes.ToString(),
                                    Weight = AdaptiveTextWeight.Default,
                                    Wrap = true
                                });
                            }
                            Attachment attachment = new Attachment()
                            {
                                Content = card,
                                ContentType = AdaptiveCard.ContentType
                            };
                            var message = context.MakeMessage();
                            message.Attachments.Add(attachment);

                            await context.PostAsync(message);
                            if (incidentModel.result[0].state.ToString() != "7" || incidentModel.result[0].state.ToString() != "6" || incidentModel.result[0].state.ToString() != "8")
                            {
                               // await context.PostAsync("Would you like to add any comment to the ticket ? Please Respond with only 'Yes' or 'No'.");
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
                                replyNext.Text = "Would you like to add any comment to the ticket ? Please Respond with only 'Yes' or 'No'.";
                                replyNext.SuggestedActions = suggestedActions;
                                await context.PostAsync(replyNext);
                                context.Wait(IfCommentToActiveTicket);
                            }
                            else
                            {
                                context.Done<string>("returnfromGetTicketStatusDialog");
                            }
                        }
                        else if (incidentStatusOutput["For_User"] == "InValid")
                        {
                            Dictionary<string, dynamic> ticketListbyUser = new Dictionary<string, dynamic>();
                            GetIncidentModel incidentListForUser = new GetIncidentModel();
                            ticketListbyUser = serviceNowActivity.GetTicketWithStatusByUser(_userSysID);
                            StringBuilder message = new StringBuilder();
                            if (ticketListbyUser["Success"] == true)
                            {
                                incidentListForUser = ticketListbyUser["Ticket_List"];
                                if (incidentListForUser.result.Length > 5)
                                {
                                    for (int i = 0; i < 5; i++)
                                    {
                                        message.AppendLine("\n* " + incidentListForUser.result[i].number.ToString());
                                    }
                                }
                                else
                                {
                                    for (int i = 0; i < incidentListForUser.result.Length; i++)
                                    {
                                        message.AppendLine("\n* " + incidentListForUser.result[i].number.ToString());
                                    }
                                }
                                await context.PostAsync("Oops! There is no ticket logged by this number. You could go ahead and choose one of your tickets from the list below and I could look it up for you. If" +
                                    "you would like to see more just type 'More'.");
                                await context.PostAsync(message.ToString());
                                context.Wait(MessageReceivedAsync);
                            }
                            else
                            {
                                await context.PostAsync("Oops! There is no ticket logged by this number. Try after sometime with a valid ticket number.");
                                context.Done<string>("returnfromGetTicketStatusDialog");
                            }  
                        }
                        else
                        {
                            Dictionary<string, dynamic> ticketListbyUser = new Dictionary<string, dynamic>();
                            GetIncidentModel incidentListForUser = new GetIncidentModel();
                            ticketListbyUser = serviceNowActivity.GetTicketWithStatusByUser(_userSysID);
                            StringBuilder message = new StringBuilder();
                            if (ticketListbyUser["Success"] == true)
                            {
                                incidentListForUser = ticketListbyUser["Ticket_List"];
                                if (incidentListForUser.result.Length > 5)
                                {
                                    for (int i = 0; i < 5; i++)
                                    {
                                        message.AppendLine("\n* " + incidentListForUser.result[i].number.ToString());
                                    }
                                }
                                else
                                {
                                    for (int i = 0; i < incidentListForUser.result.Length; i++)
                                    {
                                        message.AppendLine("\n* " + incidentListForUser.result[i].number.ToString());
                                    }
                                }
                                await context.PostAsync("Details of only those tickets that have been logged for you or by you will be displayed. You could go ahead and choose one of your tickets from the" +
                                    " list below and I could look it up for you. If you would like to see more just type 'More'.");
                                await context.PostAsync(message.ToString());
                            }
                            else
                            {
                                // Error log
                            }

                            context.Wait(MessageReceivedAsync);
                        }
                    }
                    else
                    {
                        string error = incidentStatusOutput["Exception"];
                        await context.PostAsync("Oops! Right now I am not able to connect to service now. Please try after sometimes.");
                        context.Done<string>("returnfromGetTicketStatusDialog");
                    }
                    
                }
                else if (this._ticketNumber.ToLower().Contains("req"))
                {
                    ServiceNowActivity serviceNowActivity = new ServiceNowActivity();
                    Dictionary<string, dynamic> requestStatusOutput = new Dictionary<string, dynamic>();
                    requestStatusOutput = serviceNowActivity.GetRequestWithStatusByNumber(this._ticketNumber, this._userSysID);
                    if(requestStatusOutput["Success"] == true)
                    {
                        ServiceRequestModel requestModel = new ServiceRequestModel();
                        requestModel = requestStatusOutput["Ticket_Details"];
                        
                        AdaptiveCard card = new AdaptiveCard("1.0");
                        card.Body.Add(new AdaptiveTextBlock()
                        {
                            Text = "The ticket details are as below :",
                            Weight = AdaptiveTextWeight.Bolder,
                            Wrap = true
                        });

                        if (requestModel.result[0].request_state.ToString() == "2")
                        {
                            card.Body.Add(new AdaptiveTextBlock()
                            {
                                Text = "The ticket is currently in 'In Progress' state. Our engineers are working on your issue." +
                                "As soon as I get any update I will let you know.",
                                Weight = AdaptiveTextWeight.Default,
                                Wrap = true
                            });
                        }
                        else
                        {
                            card.Body.Add(new AdaptiveTextBlock()
                            {
                                Text = "The ticket is currently in 'New' state. Soon your incident will get assigned to corresponding team.",
                                Weight = AdaptiveTextWeight.Default,
                                Wrap = true
                            });
                        }
                        Attachment attachment = new Attachment()
                        {
                            Content = card,
                            ContentType = AdaptiveCard.ContentType
                        };
                        var message = context.MakeMessage();
                        message.Attachments.Add(attachment);
                        await context.PostAsync(message);
                        context.Done<string>("returnfromGetTicketStatusDialog");
                    }
                    else
                    {
                        string error = requestStatusOutput["Exception"];
                        await context.PostAsync("Oops! Right now I am not able to connect to service now. Please try after sometimes.");
                        context.Done<string>("returnfromGetTicketStatusDialog");
                    }
                }
            }

        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;
            if(activity.Text.ToLower().Contains("more"))
            {
                showTicketCounter = showTicketCounter + 5;
                if(IfMoreTicket() == null)
                {
                    await context.PostAsync("That's all I can find for you.");
                    context.Done<string>("returnfromGetTicketStatusDialog");
                }
                else
                {
                    await context.PostAsync(IfMoreTicket());
                }
                
                context.Wait(MessageReceivedAsync);
            }
            else if(activity.Text.ToLower().Contains("inc"))
            {
                if (this._ticketNumber == null)
                {
                    this._ticketNumber = activity.Text.ToUpper();
                }
                if (this._ticketNumber != null)
                {
                    ServiceNowActivity serviceNowActivity = new ServiceNowActivity();
                    Dictionary<string, dynamic> validIncident = new Dictionary<string, dynamic>();
                    validIncident = serviceNowActivity.GetTicketWithStatusByNumber(this._ticketNumber, this._userSysID);

                    if (validIncident["Success"] == true && validIncident["For_User"] == "Valid")
                    {
                        try
                        {
                            AdaptiveCard card = new AdaptiveCard("1.0");
                            card.Body.Add(new AdaptiveTextBlock()
                            {
                                Text = "The ticket details are as below :",
                                Weight = AdaptiveTextWeight.Bolder,
                                Wrap = true
                            });

                            if (validIncident["Ticket_Details"].result[0].state.ToString() == "2")
                            {
                                card.Body.Add(new AdaptiveTextBlock()
                                {
                                    Text = "The ticket is currently in 'In Progress' state. Our engineers are working on your issue." +
                                    "As soon as I get any update I will let you know.",
                                    Weight = AdaptiveTextWeight.Default,
                                    Wrap = true
                                });
                            }
                            else if (validIncident["Ticket_Details"].result[0].state.ToString() == "1")
                            {
                                card.Body.Add(new AdaptiveTextBlock()
                                {
                                    Text = "The ticket is currently in 'New' state. Soon your incident will get assigned to corresponding team.",
                                    Weight = AdaptiveTextWeight.Default,
                                    Wrap = true
                                });
                            }

                            if (validIncident["Ticket_Details"].result[0].work_notes.ToString() != "")
                            {
                                card.Body.Add(new AdaptiveTextBlock()
                                {
                                    Text = "**Work Notes: **" + validIncident[""].result[0].work_notes.ToString(),
                                    Weight = AdaptiveTextWeight.Default,
                                    Wrap = true
                                });
                            }
                            Attachment attachment = new Attachment()
                            {
                                Content = card,
                                ContentType = AdaptiveCard.ContentType
                            };
                            var message = context.MakeMessage();
                            message.Attachments.Add(attachment);

                            await context.PostAsync(message);
                            if (validIncident["Ticket_Details"].result[0].state.ToString() != "7" || validIncident["Ticket_Details"].result[0].state.ToString() != "6" || validIncident["Ticket_Details"].result[0].state.ToString() != "8")
                            {
                                //await context.PostAsync("Would you like to add any comment to the ticket ? Please Respond with only 'Yes' or 'No'.");
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
                                replyNext.Text = "Would you like to add any comment to the ticket ? Please Respond with only 'Yes' or 'No'.";
                                replyNext.SuggestedActions = suggestedActions;
                                await context.PostAsync(replyNext);
                                context.Wait(IfCommentToActiveTicket);
                            }
                            else
                            {
                                context.Done<string>("returnfromGetTicketStatusDialog");
                            }
                        }
                        catch(Exception e)
                        {
                            e.ToString();
                            await context.PostAsync("Oops! Something went wrong. Please try after sometimes.");
                            context.Done<string>("returnfromGetTicketStatusDialog");
                        }
                        
                    }
                    else if (validIncident["Success"] == true && validIncident["For_User"] == "InValid")
                    {
                        Dictionary<string, dynamic> incidentListForUser = new Dictionary<string, dynamic>();
                        incidentListForUser = serviceNowActivity.GetTicketWithStatusByUser(_userSysID);
                        StringBuilder message = new StringBuilder();
                        if (incidentListForUser["Success"] == true)
                        {
                            if (incidentListForUser["Ticket_List"].result.Length > 5)
                            {
                                for (int i = 0; i < 5; i++)
                                {
                                    message.AppendLine("\n* " + incidentListForUser["Ticket_List"].result[i].number.ToString());
                                }
                            }
                            else
                            {
                                for (int i = 0; i < incidentListForUser["Ticket_List"].result.Length; i++)
                                {
                                    message.AppendLine("\n* " + incidentListForUser["Ticket_List"].result[i].number.ToString());
                                }
                            }

                            await context.PostAsync("Oops! There is no ticket logged by this number. You could go ahead and choose one of your tickets from the list below and I could look it up for you. If" +
                            "you would like to see more just type 'More'.");
                            await context.PostAsync(message.ToString());
                            context.Wait(MessageReceivedAsync);
                        }
                        else
                        {
                            await context.PostAsync("Oops! There is no ticket logged by this number."); // Find proper sentence
                            context.Done<string>("returnfromGetTicketStatusDialog");
                        } 
                    }
                    else if(validIncident["Success"] == true && validIncident["For_User"] == "NotForUser")
                    {
                        Dictionary<string, dynamic> incidentListForUser = new Dictionary<string, dynamic>();
                        incidentListForUser = serviceNowActivity.GetTicketWithStatusByUser(_userSysID);
                        StringBuilder message = new StringBuilder();
                        if (incidentListForUser["Success"] == true)
                        {
                            if (incidentListForUser["Ticket_List"].result.Length > 5)
                            {    
                                for (int i = 0; i < 5; i++)
                                {
                                    message.AppendLine("\n* " + incidentListForUser["Ticket_List"].result[i].number.ToString());
                                }
                            }
                            else
                            {
                                for (int i = 0; i < incidentListForUser["Ticket_List"].result.Length; i++)
                                {
                                    message.AppendLine("\n* " + incidentListForUser["Ticket_List"].result[i].number.ToString());
                                }
                            }

                            await context.PostAsync("Details of only those tickets that have been logged for you or by you will be displayed. You could go ahead and choose one of your tickets from the" +
                            " list below and I could look it up for you. If you would like to see more just type 'More'.");
                            await context.PostAsync(message.ToString());
                            context.Wait(MessageReceivedAsync);
                        }
                        else
                        {
                            await context.PostAsync("Details of only those tickets that have been logged for you or by you will be displayed."); // Find proper sentence
                            context.Done<string>("returnfromGetTicketStatusDialog");
                        }
                        
                    }
                    else if(validIncident["Success"] == false)
                    {
                        await context.PostAsync("Oops! Something went wrong. Please try after sometime.");
                        context.Done<string>("returnfromGetTicketStatusDialog");
                    }
                }
            }
            else if (this._ticketNumber.ToLower().Contains("req"))
            {
                if (this._ticketNumber == null)
                {
                    this._ticketNumber = activity.Text.ToUpper();
                }

                if (this._ticketNumber != null)
                {
                    ServiceNowActivity serviceNowActivity = new ServiceNowActivity();
                    Dictionary<string, dynamic> requestStatusOutput = new Dictionary<string, dynamic>();
                    requestStatusOutput = serviceNowActivity.GetRequestWithStatusByNumber(this._ticketNumber, this._userSysID);
                    if (requestStatusOutput["Success"] == true)
                    {
                        ServiceRequestModel requestModel = new ServiceRequestModel();
                        requestModel = requestStatusOutput["Ticket_Details"];

                        AdaptiveCard card = new AdaptiveCard("1.0");
                        card.Body.Add(new AdaptiveTextBlock()
                        {
                            Text = "The ticket details are as below :",
                            Weight = AdaptiveTextWeight.Bolder,
                            Wrap = true
                        });

                        if (requestModel.result[0].request_state.ToString() == "2")
                        {
                            card.Body.Add(new AdaptiveTextBlock()
                            {
                                Text = "The ticket is currently in 'In Progress' state. Our engineers are working on your issue." +
                                "As soon as I get any update I will let you know.",
                                Weight = AdaptiveTextWeight.Default,
                                Wrap = true
                            });
                        }
                        else
                        {
                            card.Body.Add(new AdaptiveTextBlock()
                            {
                                Text = "The ticket is currently in 'New' state. Soon your incident will get assigned to corresponding team.",
                                Weight = AdaptiveTextWeight.Default,
                                Wrap = true
                            });
                        }
                        Attachment attachment = new Attachment()
                        {
                            Content = card,
                            ContentType = AdaptiveCard.ContentType
                        };
                        var message = context.MakeMessage();
                        message.Attachments.Add(attachment);
                        await context.PostAsync(message);
                        context.Done<string>("returnfromGetTicketStatusDialog");
                    }
                    else
                    {
                        if (requestStatusOutput["Status"] == "NotForUser")
                        {
                            Dictionary<string, dynamic> requestListByUser = new Dictionary<string, dynamic>();
                            Dictionary<string, dynamic> requestListForUser = new Dictionary<string, dynamic>();
                            requestListByUser = serviceNowActivity.GetRequestWithStatusByUser(this._userSysID);
                            requestListForUser = serviceNowActivity.GetRequestWithStatusForUser(this._userSysID);
                            StringBuilder message = new StringBuilder();
                            if ((requestListByUser["Success"] == true && requestListByUser["Request_List"].result.Lenght > 0) || (requestListForUser["Success"] == true && requestListForUser["Request_List"].result.Lenght > 0))
                            {
                                message.AppendLine("Details of only those tickets that have been logged for you or by you will be displayed. You could go ahead and " +
                                "choose one of your tickets from the list below and I could look it up for you. Please type the request number.");
                                if (requestListByUser["Success"] == true)
                                {
                                    message.AppendLine("Request created by you -->");
                                    if (requestListByUser["Request_List"].result.Lenght > 3)
                                    {
                                        for (int i = 0; i < 3; i++)
                                        {
                                            message.AppendLine("\n* " + requestListByUser["Request_List"].result[i].number.ToString());
                                        }
                                    }
                                    else
                                    {
                                        for (int i = 0; i < requestListByUser["Request_List"].result.Lenght; i++)
                                        {
                                            message.AppendLine("\n* " + requestListByUser["Request_List"].result[i].number.ToString());
                                        }
                                    }
                                }
                                if (requestListForUser["Success"] == true)
                                {
                                    message.AppendLine("Request created for you -->");
                                    if (requestListForUser["Request_List"].result.Lenght > 3)
                                    {
                                        for (int i = 0; i < 3; i++)
                                        {
                                            message.AppendLine("\n* " + requestListForUser["Request_List"].result[i].number.ToString());
                                        }
                                    }
                                    else
                                    {
                                        for (int i = 0; i < requestListForUser["Request_List"].result.Lenght; i++)
                                        {
                                            message.AppendLine("\n* " + requestListForUser["Request_List"].result[i].number.ToString());
                                        }
                                    }
                                }
                            }
                            else
                            {
                                message.AppendLine("Details of only those tickets that have been logged for you or by you will be displayed. Please provide a Request ID" +
                                    " which either you had created or had been created for you.");
                            }
                            await context.PostAsync(message.ToString());
                            context.Wait(MessageReceivedAsync);
                        }
                        else if (requestStatusOutput["Status"] == "InValid")
                        {
                            Dictionary<string, dynamic> requestListByUser = new Dictionary<string, dynamic>();
                            Dictionary<string, dynamic> requestListForUser = new Dictionary<string, dynamic>();
                            requestListByUser = serviceNowActivity.GetRequestWithStatusByUser(this._userSysID);
                            requestListForUser = serviceNowActivity.GetRequestWithStatusForUser(this._userSysID);
                            StringBuilder message = new StringBuilder();
                            if ((requestListByUser["Success"] == true && requestListByUser["Request_List"].result.Lenght > 0) || (requestListForUser["Success"] == true && requestListForUser["Request_List"].result.Lenght > 0))
                            {
                                message.AppendLine("Oops! There is no ticket logged by this number. You could go ahead and choose one of your tickets from the list below " +
                                    "and I could look it up for you. Please type the request number.");
                                if (requestListByUser["Success"] == true)
                                {
                                    message.AppendLine("Request created by you -->");
                                    if (requestListByUser["Request_List"].result.Lenght > 3)
                                    {
                                        for (int i = 0; i < 3; i++)
                                        {
                                            message.AppendLine("\n* " + requestListByUser["Request_List"].result[i].number.ToString());
                                        }
                                    }
                                    else
                                    {
                                        for (int i = 0; i < requestListByUser["Request_List"].result.Lenght; i++)
                                        {
                                            message.AppendLine("\n* " + requestListByUser["Request_List"].result[i].number.ToString());
                                        }
                                    }
                                }
                                if (requestListForUser["Success"] == true)
                                {
                                    message.AppendLine("Request created for you -->");
                                    if (requestListForUser["Request_List"].result.Lenght > 3)
                                    {
                                        for (int i = 0; i < 3; i++)
                                        {
                                            message.AppendLine("\n* " + requestListForUser["Request_List"].result[i].number.ToString());
                                        }
                                    }
                                    else
                                    {
                                        for (int i = 0; i < requestListForUser["Request_List"].result.Lenght; i++)
                                        {
                                            message.AppendLine("\n* " + requestListForUser["Request_List"].result[i].number.ToString());
                                        }
                                    }
                                }
                            }
                            else
                            {
                                message.AppendLine("Oops! There is no ticket logged by this number. Please provide a Request ID" +
                                    " which either you had created or had been created for you.");
                            }
                            await context.PostAsync(message.ToString());
                            context.Wait(MessageReceivedAsync);
                        }
                        else if(requestStatusOutput["Status"] == "Failed")
                        {
                            await context.PostAsync("Oops! Something went wrong. Please try after sometime.");
                            context.Done<string>("returnfromGetTicketStatusDialog");
                        }
                    }
                }
            }
            else
            {
                await context.PostAsync("This is not a valid ticket number. Please provide a correct ticket number.");
                context.Wait(MessageReceivedAsync);
            }
        }

        private async Task IfCommentToActiveTicket(IDialogContext context, IAwaitable<object> result)
        {
            var message = await result as Activity;
            if (message.Text.ToString().ToLower().Contains("yes"))
            {
                await context.PostAsync("Please type in the entire comment before hitting the Enter key. You could use Shift+Enter if you would like to include multiple lines to your comment.");
                context.Wait(AddCommentToActiveTicket);
            }
            else
            {
                context.Done<string>("returnfromGetTicketStatusDialog");
            }
        }

        private async Task AddCommentToActiveTicket(IDialogContext context, IAwaitable<object> result)
        {
            var message = await result as Activity;
            try
            {
                //Add the comments to the ticket
                ServiceNowActivity serviceNowActivity = new ServiceNowActivity();
                if (serviceNowActivity.AddCommentToActiveTicket(_ticketNumber, message.ToString()) != null)
                {
                    await context.PostAsync("Successfully added the comments to your ticket.");
                }
                else
                {
                    await context.PostAsync("Oops! Unfortunately some issue occured while adding the comment to the ticket. We have stored your data. Will do it.");
                }
               
            }
            catch(Exception e)
            {
                await context.PostAsync("Oops! Unfortunately some issue occured while adding the comment to the ticket. We have stored your data. Will do it.");
                e.ToString();
            }
            finally
            {
                context.Done<string>("returnfromGetTicketStatusDialog");
            }

        }



        private string IfMoreTicket()
        {
            ServiceNowActivity serviceNowActivity = new ServiceNowActivity();
            Dictionary<string, dynamic> incidentListForUser = new Dictionary<string, dynamic>();
            incidentListForUser = serviceNowActivity.GetTicketWithStatusByUser(_userSysID);
            StringBuilder moreMessage = new StringBuilder();
            if (incidentListForUser["Success"] == true)
            {
                if (incidentListForUser["Ticket_List"].result.Length > (showTicketCounter + 5))
                {
                    for (int i = showTicketCounter; i < (showTicketCounter + 5); i++)
                    {
                        moreMessage.AppendLine("\n* " + incidentListForUser["Ticket_List"].result[i].number.ToString());
                    }
                }
                else
                {
                    for (int i = showTicketCounter; i < incidentListForUser["Ticket_List"].result.Length; i++)
                    {
                        moreMessage.AppendLine("\n* " + incidentListForUser["Ticket_List"].result[i].number.ToString());
                    }
                }
            }
            else
            {

            }
            return moreMessage.ToString();
        }

    }
}