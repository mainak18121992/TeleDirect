using System;
using System.Collections.Generic;
using System.Linq;
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

namespace ServiceNowClassLibrary
{
    public partial class ServiceNowActivity
    {
        /// <summary>
        /// public method to create incident based on a parameter of type CreateIncidentModel.
        /// It will return a data dictionary of type <string,dynamic> with three data in it : 1. Ticket_Number, 2. Success, 3. Exception.
        /// </summary>
        /// <param name="inci">Hold the data to send to SNOW by post request. This paramenter is an object of CreateIncidentModel class.</param>
        /// <returns></returns>
        public Dictionary<string, dynamic> CreateIncident(CreateIncidentModel inci)
        {
            Dictionary<string, dynamic> incidentNumberOutput = new Dictionary<string, dynamic>();
            string incidnetNumber = null;
            //inci.category = GetCategoryforIncident(inci.subcategory);
            inci.category = "inquiry";
            string createIncidentUrl = String.Format(ServiceNowConstants.CreateIncident, ServiceNowConstants.snowinstance);
            var authHeaders = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes(ServiceNowConstants.SnowApiUserName + ":" + ServiceNowConstants.SnowApiPassword)));
            var inpuJsonString = JsonConvert.SerializeObject(inci);

            httpClient.DefaultRequestHeaders.Authorization = authHeaders;
            httpClient.DefaultRequestHeaders.Add("Accept", GlobalConstants.ContentTypeJson);
            var request = new HttpRequestMessage(HttpMethod.Post, createIncidentUrl) { Content = new StringContent(inpuJsonString, Encoding.UTF8, GlobalConstants.ContentTypeJson) };
            try
            {
                var response = httpClient.SendAsync(request).Result;
                if (!response.IsSuccessStatusCode)
                {
                    incidentNumberOutput.Add("Ticket_Number", null);
                    incidentNumberOutput.Add("Success", false);
                    incidentNumberOutput.Add("Exception", "Request failed to sent.");
                }
                else
                {
                    var incidentRequestResult = response.Content.ReadAsStringAsync();
                    var incidentDetails = JsonConvert.DeserializeObject<CreateIncidentResultModel>(incidentRequestResult.Result);
                    incidnetNumber = incidentDetails.result.number;

                    incidentNumberOutput.Add("Ticket_Number", incidnetNumber);
                    incidentNumberOutput.Add("Success", true);
                    incidentNumberOutput.Add("Exception", null);
                }
                
            }
            catch(Exception e)
            {
                e.ToString();
                incidentNumberOutput.Add("Ticket_Number", null);
                incidentNumberOutput.Add("Success", false);
                incidentNumberOutput.Add("Exception", e.ToString());                
            }
            return incidentNumberOutput;
        }

        
    }
}
