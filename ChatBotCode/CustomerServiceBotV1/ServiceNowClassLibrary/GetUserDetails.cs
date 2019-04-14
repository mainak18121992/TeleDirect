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
        /// Public function to get the user details from snow.
        /// </summary>
        /// <param name="id">Need to pass the user email ID to this method which will act as a unique reference 
        /// for that user in SNOW to get the sys id.</param>
        /// <returns>It returns a Data Dictionary of type <string, dynamic> with three values in it.  
        /// 1. UserModel - an object of UserModel Class
        /// 2. Success State - Boolean (true/false)
        /// 3. Exception if any.</returns>
        public Dictionary<string, dynamic> GetUser(string id)
        {
            Dictionary<string, dynamic> userOutput = new Dictionary<string, dynamic>();
            //Dictionary<UserModel, string> userOutput = new Dictionary<UserModel, string>();
            UserModel user = new UserModel();
            string GetUserUrl = String.Format(ServiceNowConstants.GetUserDetailsFromSnow, ServiceNowConstants.snowinstance, id);
            var authHeaders = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes(ServiceNowConstants.SnowApiUserName + ":" + ServiceNowConstants.SnowApiPassword)));
            httpClient.DefaultRequestHeaders.Authorization = authHeaders;
            httpClient.DefaultRequestHeaders.Add("Accept", GlobalConstants.ContentTypeJson);
            var request = new HttpRequestMessage(HttpMethod.Get, GetUserUrl);
            try
            {
                var response = httpClient.SendAsync(request).Result;
                if (!response.IsSuccessStatusCode) return null;
                var userRequestResult = response.Content.ReadAsStringAsync();
                user = JsonConvert.DeserializeObject<UserModel>(userRequestResult.Result);

                userOutput.Add("UserModel", user);
                userOutput.Add("Success", true);
                userOutput.Add("Exception", null);
                
            }
            catch (Exception e)
            {
                e.ToString();
                userOutput.Add("UserModel", null);
                userOutput.Add("Success", false);
                userOutput.Add("Exception", e.ToString());
                return null;
            }

            //return user;
            return userOutput;
        }
    }
}
