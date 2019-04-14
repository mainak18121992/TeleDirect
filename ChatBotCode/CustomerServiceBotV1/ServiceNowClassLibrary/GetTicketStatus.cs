using System.Text;
using System.Threading.Tasks;
using ServiceNowClassLibrary.Models;
using ServiceNowClassLibrary.GlobalResources;
using System.Net.Http.Headers;
using System.Net.Http;
using Newtonsoft.Json;
using System.IO;
using System.Net.Mail;
using System.Net;
using System;
using System.Collections.Generic;

namespace ServiceNowClassLibrary
{
    public partial class ServiceNowActivity
    {
       
        //private ValidIncidentModel _querriedIncident = new ValidIncidentModel();
        private enum _ticketStatus
        {
            Valid,
            NotForUser,
            InValid,
            Failed
        }
        public Dictionary<string, dynamic> GetTicketWithStatusByNumber(string incident, string userID)
        {
            Dictionary<string, dynamic> validTicketOutput = new Dictionary<string, dynamic>();
            Dictionary<string, dynamic> validTicket = new Dictionary<string, dynamic>();
            validTicket = GetTicketOnValidation(incident, userID);
            if (validTicket.ContainsValue(_ticketStatus.Valid))
            {
                validTicketOutput.Add("Ticket_Details", validTicket["TicketInfo"]);
                validTicketOutput.Add("Success", true);
                validTicketOutput.Add("For_User", _ticketStatus.Valid.ToString());
            }
            else if (validTicket.ContainsValue(_ticketStatus.NotForUser))
            {
                validTicketOutput.Add("Ticket_Details", null);
                validTicketOutput.Add("Success", true);
                validTicketOutput.Add("For_User", _ticketStatus.NotForUser.ToString());
            }
            else if (validTicket.ContainsValue(_ticketStatus.InValid))
            {
                validTicketOutput.Add("Ticket_Details", null);
                validTicketOutput.Add("Success", true);
                validTicketOutput.Add("For_User", _ticketStatus.InValid.ToString());
            }
            else if (validTicket.ContainsValue(_ticketStatus.Failed))
            {
                validTicketOutput.Add("Ticket_Details", null);
                validTicketOutput.Add("Success", false);
                validTicketOutput.Add("Exception", validTicket["Exception"]);
            }

            return validTicketOutput;
        }

        public Dictionary<string, dynamic> GetTicketWithStatusByUser(string userID)
        {
            Dictionary<string, dynamic> getIncidentlist = new Dictionary<string, dynamic>();
            getIncidentlist = GetTicketListForUser(userID);
            return getIncidentlist;
        }

        public Dictionary<string, dynamic> AddCommentToActiveTicket(string incident, string notes)
        {
            Dictionary<string, dynamic> addComment = new Dictionary<string, dynamic>();
            string updateTime = null;
            string ticket_SysID = GetTicketSysID(incident);
            if (ticket_SysID != null)
            {
                string updateIncidentNotesUrl = string.Format(ServiceNowConstants.UpdateIncidentWorkNotes, ServiceNowConstants.snowinstance, GetTicketSysID(incident));
                var authHeaders = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes(ServiceNowConstants.SnowApiUserName + ":" + ServiceNowConstants.SnowApiPassword)));
                httpClient.DefaultRequestHeaders.Authorization = authHeaders;
                httpClient.DefaultRequestHeaders.Add("Accept", GlobalConstants.ContentTypeJson);
                var request = new HttpRequestMessage(HttpMethod.Put, updateIncidentNotesUrl);
                var addComments = new AdditionalComments() { comments = notes, work_notes = notes };
                var addCommentsJsonString = JsonConvert.SerializeObject(addComments);
                request.Content = new StringContent(addCommentsJsonString, Encoding.UTF8, GlobalConstants.ContentTypeJson);
                try
                {
                    var response = httpClient.SendAsync(request).Result;
                    var result = response.Content.ReadAsStringAsync();
                    var updatedInfoOfIncident = JsonConvert.DeserializeObject<GetUpdatedTimeForAdditionalNotesResult>(result.Result);
                    updateTime = updatedInfoOfIncident.Result.sys_updated_on.ToString();
                    addComment.Add("Success", true);
                }
                catch (Exception e)
                {
                    addComment.Add("Success", false);
                    addComment.Add("Exception", e.ToString());
                }
            }
            else
            {
                addComment.Add("Success", false);
                addComment.Add("Exception", "Ticket sys id not available.");
            }
            

            return addComment;
        }



        private string GetTicketSysID(string incident)
        {
            string ticketSysID = null;
            string getTicketSysIDUrl = String.Format(ServiceNowConstants.GetIncidentSysId, ServiceNowConstants.snowinstance, incident);
            var authHeaders = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes(ServiceNowConstants.SnowApiUserName + ":" + ServiceNowConstants.SnowApiPassword)));
            httpClient.DefaultRequestHeaders.Authorization = authHeaders;
            httpClient.DefaultRequestHeaders.Add("Accept", GlobalConstants.ContentTypeJson);
            var request = new HttpRequestMessage(HttpMethod.Get, getTicketSysIDUrl);
            try
            {
                var response = httpClient.SendAsync(request).Result;
                if (response.IsSuccessStatusCode)
                {
                    var incidentValidationResult = response.Content.ReadAsStringAsync();
                    GetIncidentModel _detailedIncident = JsonConvert.DeserializeObject<GetIncidentModel>(incidentValidationResult.Result);
                    if (_detailedIncident.result[0].sys_id != null)
                    {
                        ticketSysID = _detailedIncident.result[0].sys_id.ToString();
                    }
                }

            }
            catch (Exception e)
            {
                e.ToString();
            }

            return ticketSysID;
        }
        private Dictionary<string, dynamic> GetTicketListForUser(string userID)
        {
            Dictionary<string, dynamic> dictionary = new Dictionary<string, dynamic>();
            GetIncidentModel incidentListByUser = new GetIncidentModel();
            string getIncidentsByUserUrl = String.Format(ServiceNowConstants.GetIncidentListforUser, ServiceNowConstants.snowinstance, userID);
            var authHeaders = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes(ServiceNowConstants.SnowApiUserName + ":" + ServiceNowConstants.SnowApiPassword)));
            httpClient.DefaultRequestHeaders.Authorization = authHeaders;
            httpClient.DefaultRequestHeaders.Add("Accept", GlobalConstants.ContentTypeJson);
            var request = new HttpRequestMessage(HttpMethod.Get, getIncidentsByUserUrl);
            try
            {
                var response = httpClient.SendAsync(request).Result;
                if (response.IsSuccessStatusCode)
                {
                    var incidentListResult = response.Content.ReadAsStringAsync();
                    incidentListByUser = JsonConvert.DeserializeObject<GetIncidentModel>(incidentListResult.Result);
                    dictionary.Add("Ticket_List", incidentListByUser);
                    dictionary.Add("Success", true);
                }

            }
            catch (Exception e)
            {
                dictionary.Add("Ticket_List", null);
                dictionary.Add("Success", false);
                dictionary.Add("Exception", e.ToString());
            }
            return dictionary;
        }
        private Dictionary<string, dynamic> GetTicketOnValidation(string incident, string userID)
        {
            Dictionary<string, dynamic> _detailedIncidentDict = new Dictionary<string, dynamic>();
            GetIncidentModel _detailedIncident = new GetIncidentModel();
            string validateIncidentUrl = String.Format(ServiceNowConstants.ValidateIncident, ServiceNowConstants.snowinstance, incident);
            var authHeaders = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes(ServiceNowConstants.SnowApiUserName + ":" + ServiceNowConstants.SnowApiPassword)));
            httpClient.DefaultRequestHeaders.Authorization = authHeaders;
            httpClient.DefaultRequestHeaders.Add("Accept", GlobalConstants.ContentTypeJson);
            var request = new HttpRequestMessage(HttpMethod.Get, validateIncidentUrl);
            try
            {
                var response = httpClient.SendAsync(request).Result;
                if (response.IsSuccessStatusCode)
                {
                    var incidentValidationResult = response.Content.ReadAsStringAsync();
                    _detailedIncident = JsonConvert.DeserializeObject<GetIncidentModel>(incidentValidationResult.Result);
                    if (_detailedIncident.result[0].sys_id != null)
                    {
                        if (_detailedIncident.result[0].caller_id.value == userID)
                        {
                            _detailedIncidentDict.Add("TicketInfo", _detailedIncident);
                            _detailedIncidentDict.Add("Validity", _ticketStatus.Valid);
                        }
                        else
                        {
                            _detailedIncidentDict.Add("TicketInfo", _detailedIncident);
                            _detailedIncidentDict.Add("Validity", _ticketStatus.NotForUser);
                        }
                    }
                    else
                    {
                        _detailedIncidentDict.Add("TicketInfo", _detailedIncident);
                        _detailedIncidentDict.Add("Validity", _ticketStatus.InValid);
                    }
                }
                
            }
            catch (Exception e)
            {
                _detailedIncidentDict.Add("TicketInfo", _detailedIncident);
                _detailedIncidentDict.Add("Validity", _ticketStatus.Failed);
                _detailedIncidentDict.Add("Exception", e.ToString());
            }
            return _detailedIncidentDict;
        }
        private GetIncidentModel GetTicketWithStatusByProduct(string product, string userID)
        {
            GetIncidentModel incidentListByProd = new GetIncidentModel();
            string getIncidentsByProdUrl = String.Format(ServiceNowConstants.GetIncidentListforProdLastFiveDays, ServiceNowConstants.snowinstance, product, userID, DateTime.Today.ToString("yyyy-MM-dd"));
            var authHeaders = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes(ServiceNowConstants.SnowApiUserName + ":" + ServiceNowConstants.SnowApiPassword)));
            httpClient.DefaultRequestHeaders.Authorization = authHeaders;
            httpClient.DefaultRequestHeaders.Add("Accept", GlobalConstants.ContentTypeJson);
            var request = new HttpRequestMessage(HttpMethod.Get, getIncidentsByProdUrl);
            try
            {
                var response = httpClient.SendAsync(request).Result;
                if (response.IsSuccessStatusCode)
                {
                    var incidentListResult = response.Content.ReadAsStringAsync();
                    incidentListByProd = JsonConvert.DeserializeObject<GetIncidentModel>(incidentListResult.Result);
                }

            }
            catch (Exception e)
            {
                e.ToString();
            }
            return incidentListByProd;
        }
    }
}
