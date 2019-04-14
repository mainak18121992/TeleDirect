using Newtonsoft.Json;
using ServiceNowClassLibrary.GlobalResources;
using ServiceNowClassLibrary.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace ServiceNowClassLibrary
{
    public partial class ServiceNowActivity
    {
        public KnowledgeArticle GetKBArticle(string text)
        {
            KnowledgeArticle article = new KnowledgeArticle();

            string getKBArticle = String.Format(ServiceNowConstants.GetKBArticles, ServiceNowConstants.snowinstance, text);
            var authHeaders = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes(ServiceNowConstants.SnowApiUserName + ":" + ServiceNowConstants.SnowApiPassword)));
            httpClient.DefaultRequestHeaders.Authorization = authHeaders;
            httpClient.DefaultRequestHeaders.Add("Accept", GlobalConstants.ContentTypeJson);
            var request = new HttpRequestMessage(HttpMethod.Get, getKBArticle);
            try
            {
                var response = httpClient.SendAsync(request).Result;
                if (response.IsSuccessStatusCode)
                {
                    var incidentListResult = response.Content.ReadAsStringAsync();
                    article = JsonConvert.DeserializeObject<KnowledgeArticle>(incidentListResult.Result); 
                }

            }
            catch (Exception e)
            {
                e.ToString();
            }

            return article;
        }
    }
}
