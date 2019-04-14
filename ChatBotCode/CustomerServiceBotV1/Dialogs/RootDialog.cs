using System;
using System.Threading.Tasks;

using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.Dialogs;
using System.Net.Http;
using System.Collections.Generic;
using System.Threading;
using ServiceNowClassLibrary;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using System.Text;
using ServiceNowClassLibrary.Models;

namespace LuisBot.Dialogs
{
    [Serializable]
    public class RootDialog : IDialog<object>
    {
        private string userSysID = null;
        private Dictionary<string, string> userDict = new Dictionary<string, string>();

        /// <summary>
        /// The start of the code that represents the conversational dialog.
        /// </summary>
        /// <param name="context">The dialog context.</param>
        /// <returns>
        /// A task that represents the dialog start.
        /// </returns>
        public async Task StartAsync(IDialogContext context)
        {
            //var user = context.Activity.From.Name;
            string user = "Mainak Chatterjee";
            UserModel userModel = new UserModel();
            userModel = AuthenticateUser(user);
            if(userModel == null)
            {
                await context.PostAsync("Sorry....!!!! You are not authorised to use this service. Please write a request to your Manager.");
                context.Done<object>(null);
            }
            else
            {
                this.userSysID = userModel.result[0].sys_id;
                this.userDict.Add("Name", userModel.result[0].name);
                this.userDict.Add("Email", userModel.result[0].email);
                this.userDict.Add("Phone", userModel.result[0].phone);
                //this.userSysID = "c9884ac30a0a0bdb3879454e8f02774e";

                if (userSysID == null)
                {
                    await context.PostAsync("Sorry....!!!! You are not authorised to use this service. Please write a request to your Manager.");
                    context.Done<object>(null);
                }
                else
                {
                    await context.PostAsync($"Hi {user} ! How can I assist you today?");
                    context.Wait(this.MessageReceivedAsync);
                }
            }
             
        }



        /// <summary>
        /// Messages the received asynchronous.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="result">The result.</param>
        /// <returns></returns>
        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            /* When MessageReceivedAsync is called, it's passed an IAwaitable<IMessageActivity>. To get the message,
             *  await the result. */
            var message = await result;
            context.Call(new BaseDialogController(this.userSysID, this.userDict), this.ResumeAfterLUISDialog);
        }




        /// <summary>
        /// Resumes the after luis dialog.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="result">The result.</param>
        /// <returns></returns>
        private async Task ResumeAfterLUISDialog(IDialogContext context, IAwaitable<object> result)
        {
            context.Wait(ContinueRootDialog);
        }

        /// <summary>
        /// Continues the root dialog.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="result">The result.</param>
        /// <returns></returns>
        private async Task ContinueRootDialog(IDialogContext context, IAwaitable<object> result)
        {
            try
            {
                var message = await result as Activity;
                if (message.Text.ToString().ToLower().Contains("yes"))
                {
                    await context.PostAsync("Sure... How can I help you... ?");
                    context.Call(new BaseDialogController(this.userSysID, this.userDict), this.ResumeAfterLUISDialog);
                }
                else
                {
                    await context.PostAsync("Thank you for contacting Global Service Desk.");
                }

            }
            catch (Exception ex)
            {
                await context.PostAsync($"Failed with message: {ex.Message}");
            }
            finally
            {
                context.Wait(this.MessageReceivedAsync);
            }
        }

        /// <summary>
        /// Authenticates the user.
        /// </summary>
        /// <param name="uID">The user email id as identifier identifier.</param>
        /// <returns>An object of ServiceNowClassLibrary.Models.UserModel </returns>
        private UserModel AuthenticateUser(string uID)
        {
            ServiceNowActivity getUserDetails = new ServiceNowActivity();
            ServiceNowClassLibrary.Models.UserModel authenticatedUser = new ServiceNowClassLibrary.Models.UserModel();
            Dictionary<string, dynamic> userOutput = new Dictionary<string, dynamic>();
            userOutput = getUserDetails.GetUser(uID);
            if(userOutput["Success"] == true)
            {
                authenticatedUser = userOutput["UserModel"];
            }
            else
            {
                //Log error
                string Error = userOutput["Exception"];
            }
            
            if (authenticatedUser != null)
            {
                return authenticatedUser;
            }
            else
            {
                return null;
            }
        }
 
    }
}