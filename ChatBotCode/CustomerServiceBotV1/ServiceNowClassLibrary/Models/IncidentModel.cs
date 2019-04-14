using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceNowClassLibrary.Models
{
    [Serializable]
    /// <summary>
    /// Model to gather data for Create Incident POST request.
    ///</summary>
    public class CreateIncidentModel
    {
        public string short_description { get; set; }
        public string caller_id { get; set; }
        public int urgency { get; set; }
        public int impact { get; set; }
        public string comments { get; set; }
        public string category { get; set; }
        public string subcategory { get; set; }
    }

    /// <summary>
    /// Model to gather additional comments if user wants to add to an existing incident.
    /// </summary>
    public class AdditionalComments
    {
        public string comments { get; set; }
        public string work_notes { get; set; }
    }


    /// <summary>
    /// Model to capture and deserealize the JSON response which we get once we POST the addition comment of the user to the existing incident.
    /// </summary>
    public class GetUpdatedTimeForAdditionalNotesResult
    {
        public GetUpdatedTimeForAdditionalNotes Result { get; set; }
    }
    public class GetUpdatedTimeForAdditionalNotes
    {
        public DateTime sys_updated_on { get; set; }
    }

    /// <summary>
    /// Model to capture and deserealize the JSON response which we get once we POST the gathered information to SNOW to create a new incident.
    /// </summary>
    public class CreateIncidentResultModel
    {
        public IncidentResult result { get; set; }
    }

    /// <summary>
    /// Model to capture and deserealize the JSON response which we get once we GET the information from SNOW for a specific incident.
    /// </summary>
    public class ValidIncidentModel
    {
        public IncidentResult result { get; set; }
    }

    /// <summary>
    /// Model to capture and deserealize the JSON response which we get once we GET the information from SNOW for a specific incident or for a list of incidents.
    /// </summary>
    public class GetIncidentModel
    {
        public IncidentResult[] result { get; set; }
    }


    /// <summary>
    /// Model to hold sub parameters of Incidents.
    /// </summary>
    public class IncidentResult
    {
        public string parent { get; set; }
        public string made_sla { get; set; }
        public string caused_by { get; set; }
        public string watch_list { get; set; }
        public string upon_reject { get; set; }
        public string sys_updated_on { get; set; }
        public string child_incidents { get; set; }
        public string hold_reason { get; set; }
        public string u_create_outage { get; set; }
        public string approval_history { get; set; }
        public string skills { get; set; }
        public string number { get; set; }
        public string resolved_by { get; set; }
        public string sys_updated_by { get; set; }
        public Opened_By_Incident opened_by { get; set; }
        public string user_input { get; set; }
        public string sys_created_on { get; set; }
        public Sys_Domain_Incident sys_domain { get; set; }
        public string state { get; set; }
        public string u_bot_knowledge_number { get; set; }
        public string sys_created_by { get; set; }
        public string knowledge { get; set; }
        public string order { get; set; }
        public string calendar_stc { get; set; }
        public string closed_at { get; set; }
        public Cmdb_Ci_Incident cmdb_ci { get; set; }
        public string delivery_plan { get; set; }
        public string contract { get; set; }
        public int impact { get; set; }
        public string active { get; set; }
        public string work_notes_list { get; set; }
        public string business_service { get; set; }
        public string priority { get; set; }
        public string sys_domain_path { get; set; }
        public string rfc { get; set; }
        public string time_worked { get; set; }
        public string expected_start { get; set; }
        public string opened_at { get; set; }
        public string business_duration { get; set; }
        public string group_list { get; set; }
        public string work_end { get; set; }
        public Caller_Id_Incident caller_id { get; set; }
        public string resolved_at { get; set; }
        public string approval_set { get; set; }
        public string subcategory { get; set; }
        public string work_notes { get; set; }
        public string short_description { get; set; }
        public string close_code { get; set; }
        public string correlation_display { get; set; }
        public string delivery_task { get; set; }
        public string work_start { get; set; }
        public Assignment_Group_Incident assignment_group { get; set; }
        public string additional_assignee_list { get; set; }
        public string business_stc { get; set; }
        public string description { get; set; }
        public string calendar_duration { get; set; }
        public string close_notes { get; set; }
        public string notify { get; set; }
        public string sys_class_name { get; set; }
        public Closed_By_Incident closed_by { get; set; }
        public string follow_up { get; set; }
        public string parent_incident { get; set; }
        public string sys_id { get; set; }
        public string contact_type { get; set; }
        public string incident_state { get; set; }
        public int urgency { get; set; }
        public Problem_Id_Incident problem_id { get; set; }
        public Company_Incident company { get; set; }
        public string reassignment_count { get; set; }
        public string activity_due { get; set; }
        public Assigned_To_Incident assigned_to { get; set; }
        public string severity { get; set; }
        public string comments { get; set; }
        public string approval { get; set; }
        public string sla_due { get; set; }
        public string comments_and_work_notes { get; set; }
        public string due_date { get; set; }
        public string sys_mod_count { get; set; }
        public string reopen_count { get; set; }
        public string sys_tags { get; set; }
        public string escalation { get; set; }
        public string upon_approval { get; set; }
        public string correlation_id { get; set; }
        public Location_Incident location { get; set; }
        public string category { get; set; }
    }

    public class Opened_By_Incident
    {
        public string link { get; set; }
        public string value { get; set; }
    }

    public class Sys_Domain_Incident
    {
        public string link { get; set; }
        public string value { get; set; }
    }

    public class Cmdb_Ci_Incident
    {
        public string link { get; set; }
        public string value { get; set; }
    }

    public class Caller_Id_Incident
    {
        public string link { get; set; }
        public string value { get; set; }
    }

    public class Assignment_Group_Incident
    {
        public string link { get; set; }
        public string value { get; set; }
    }

    public class Closed_By_Incident
    {
        public string link { get; set; }
        public string value { get; set; }
    }

    public class Problem_Id_Incident
    {
        public string link { get; set; }
        public string value { get; set; }
    }

    public class Company_Incident
    {
        public string link { get; set; }
        public string value { get; set; }
    }

    public class Assigned_To_Incident
    {
        public string link { get; set; }
        public string value { get; set; }
    }

    public class Location_Incident
    {
        public string link { get; set; }
        public string value { get; set; }
    }


    /// <summary>
    /// Model to get the Category of an incident.
    /// </summary>
    public class GetCategoryforIncident
    {
        public CategoryforIncidentResult[] result { get; set; }
    }

    public class CategoryforIncidentResult
    {
        public string dependent_value { get; set; }
        public string sys_mod_count { get; set; }
        public string language { get; set; }
        public string label { get; set; }
        public string sys_updated_on { get; set; }
        public string sys_domain_path { get; set; }
        public string sys_tags { get; set; }
        public string sequence { get; set; }
        public string sys_id { get; set; }
        public string inactive { get; set; }
        public string sys_updated_by { get; set; }
        public string sys_created_on { get; set; }
        public string hint { get; set; }
        public Sys_Domain_Category sys_domain { get; set; }
        public string name { get; set; }
        public string value { get; set; }
        public string sys_created_by { get; set; }
        public string element { get; set; }
    }

    public class Sys_Domain_Category
    {
        public string link { get; set; }
        public string value { get; set; }
    }

}

