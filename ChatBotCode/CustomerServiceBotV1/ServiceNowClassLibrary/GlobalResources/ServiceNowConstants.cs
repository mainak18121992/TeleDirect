using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceNowClassLibrary.GlobalResources
{
    public class ServiceNowConstants
    {
        /// <summary>
        /// SNOW Instance details needs to be added here. It can be taken from database as well.
        /// <remarks>Mainak Chatterjee (mchatterjee@hpe.com), 2018-05-03</remarks>
        /// <param name="snowinstance">Get it from SNOW Instance URL. Ex. https://dev22303.service-now.com</param>
        /// <param name="SnowApiUserName">UserName needs to be created which will have a specific role (rest_service) so that it can be used to make https requests.</param>
        /// <param name="SnowApiPassword">Password for the Username</param>
        /// <param name="CustomerSupportQueueID">This is a specific queue id which can be obtain from SNOW. All the agents will be logged into this queue to take chats from end user.</param>
        /// <returns></returns>
        /// </summary>
        public const string snowinstance = "dev55877";
        public const string SnowApiUserName = "admin";
        public const string SnowApiPassword = "Password@123";
        public const string CustomerSupportQueueID = "099aca2d4fc223008b85eb118110c78f"; 

        /// <summary>
        /// Service Now Url Constants Service Request
        ///<remarks>Mainak Chatterjee (mchatterjee@hpe.com), 2018-05-03</remarks>
        /// <param name="GetServiceRequestStatus">Get the service request status by passing the request number.</param>
        /// <param name="GetServiceRequestListByUser">Get the list of service request created by the user by passing the user sysID. It will give a list in decending order of create date.</param>
        /// <param name="GetServiceRequestListByUser">Get the list of service request created for the user by passing the user sysID. It will give a list in decending order of create date.</param>
        /// </summary>
        public const string GetServiceRequestStatus = "https://{0}.service-now.com/api/now/table/sc_request?sysparm_query=number%3D{1}&sysparm_limit=1",
            GetServiceRequestListByUser = "https://{0}.service-now.com/api/now/table/sc_request?sysparm_query=opened_by%3D{1}&sysparm_order=sys_created_on&sysparm_order_direction=desc&sysparm_limit=10",
            GetServiceRequestListForUser = "https://{0}.service-now.com/api/now/table/sc_request?sysparm_query=requested_for%3D{1}&sysparm_order=sys_created_on&sysparm_order_direction=desc&sysparm_limit=10";
        //GetServiceRequestWorkNotes = "https://{0}.service-now.com/api/now/table/sys_audit?sysparm_query=tablenameSTARTSWITHsc_request%5EdocumentkeySTARTSWITH{1}ORDERBYDESCsys_created_on%5Efieldname%3Dwork_notes&sysparm_fields=newvalue&sysparm_limit=10";

        /// <summary>
        /// Service Now Url Constants Incident
        /// <remarks>Mainak Chatterjee (mchatterjee@hpe.com), 2018-05-03</remarks>
        /// <param name="CreateIncident">Create incident by making a POST request to this URL</param>
        /// <param name="GetIncidentSysId">Get the sys id of an incident by passing the incident number.</param>
        /// <param name="UpdateIncidentWorkNotes">Update the worknotes by making a POST request to the url and passing the sysID of the incident.</param>
        /// <param name="ValidateIncident">Get the entire information of an incident by passing the incident number to validate the given incident number.</param>
        /// <param name="GetIncidentListforProdLastFiveDays">Get the list of inicidents by a user for a particular product in last 5 days.</param>
        /// <param name="GetIncidentListforUser">Get the list of incidents created by a user order by created on date decending.</param>
        /// </summary>
        public const string CreateIncident = "https://{0}.service-now.com/api/now/table/incident",
            GetIncidentSysId = "https://{0}.service-now.com/api/now/table/incident?sysparm_query=number={1}&sysparm_fields=sys_id",
            UpdateIncidentWorkNotes = "https://{0}.service-now.com/api/now/table/incident/{1}?sysparm_fields=sys_updated_on",
            ValidateIncident = "https://{0}.service-now.com/api/now/table/incident?sysparm_query=number={1}",
            GetIncidentListforProdLastFiveDays = "https://{0}.service-now.com/api/now/table/incident?sysparm_query=subcategoryLIKE{1}%5Ecaller_id%3D{2}%5Esys_created_on%3E%3D{3}&sysparm_limit=10",
            GetIncidentListforUser = "https://{0}.service-now.com/api/now/table/incident?sysparm_query=caller_id%3D{1}%5EORDERBYDESCsys_created_on";
        
        /// <summary>
        /// Service Now Url Constants User Information
        /// <remarks>Mainak Chatterjee (mchatterjee@hpe.com), 2018-05-03</remarks>
        /// <param name="GetUserDetailsFromSnow">Get the user details from snow by passing the name of the user. It will help to validate the user as well as it give the user sysID which is mandatory.</param>
        /// </summary>
        public const string GetUserDetailsFromSnow = "https://{0}.service-now.com/api/now/table/sys_user?sysparm_query=name%3D{1}&sysparm_limit=1";

        /// <summary>
        /// Service Now Chat Transfer related
        /// <remarks>Mainak Chatterjee (mchatterjee@hpe.com), 2018-05-03</remarks>
        /// <param name="CreateChatSession">Create a chat session to a particular chat queue.</param>
        /// <param name="PostMessageToChatID">Once you create a chat session it will return a Chat ID for that session. You have to POST the messages to that ID using this URL.</param>
        /// <param name="GetMessageFromChat">Get the messages form a specific CHAT ID.</param>
        /// </summary>
        public const string CreateChatSession = "https://{0}.service-now.com/api/now/connect/support/queues/{1}/sessions",
            PostMessageToChatID = "https://{0}.service-now.com/api/now/connect/conversations/{1}/messages",
            GetMessageFromChat = "https://{0}.service-now.com/api/now/connect/conversations/{1}/messages";

        public const string GetKBArticles = "https://{0}.service-now.com/api/now/table/kb_knowledge?sysparm_query=GOTO123TEXTQUERY321={1}&sysparm_limit=10";

    }
}
