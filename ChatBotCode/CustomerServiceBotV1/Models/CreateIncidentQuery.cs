using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Bot.Builder.FormFlow;

namespace LuisBot.Models
{
    [Serializable]
    public class CreateIncidentQuery
    {
        [Prompt("What would you like to mark the urgency of the issue as: Low, Medium, High")]
        public string Urgency { get; set; }

        [Prompt("When did you notice this issue?")]
        public DateTime startDateTime { get; set; }

        [Numeric(1, int.MaxValue)]
        [Prompt("Could you please specify the number of affected users like 1 for single user or 2 and above for multiple users?")]
        public int ImpactedUser { get; set; }

        [Prompt("If you would like me to add any additional comment to the ticket, please type in the entire content before hitting the Enter key. you could use Shift+Enter to move to the next line of your comment.")]
        public string additionalComments { get; set; }
    }
}