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
        //public string[] GetUserCred(string uID)
        //{
        //    string[] userCred = new string[2];
        //    string getUserCredUrl = String.Format(ServiceNowConstants.GetUserCred, ServiceNowConstants.snowinstance, uID);
        //    var authHeaders = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes(ServiceNowConstants.SnowApiUserName + ":" + ServiceNowConstants.SnowApiPassword)));
        //    httpClient.DefaultRequestHeaders.Authorization = authHeaders;
        //    httpClient.DefaultRequestHeaders.Add("Accept", GlobalConstants.ContentTypeJson);
        //    var request = new HttpRequestMessage(HttpMethod.Get, getUserCredUrl);
        //    try
        //    {
        //        var response = httpClient.SendAsync(request).Result;
        //        if (!response.IsSuccessStatusCode) return null;
        //        var userCredRequestResult = response.Content.ReadAsStringAsync();
        //        var userCreddetails = JsonConvert.DeserializeObject<GetUserCred>(userCredRequestResult.Result);
        //        userCred[0] = userCreddetails.result.user_name.ToString();
        //        //userCred[1] = this.DecryptBase64(userCreddetails.result.user_password.ToString());
        //        userCred[1] = userCreddetails.result.name.Replace(" ", String.Empty).ToString();
        //    }
        //    catch (Exception e)
        //    {
        //        return null;
        //    }

        //    return userCred;
        //}

        public string CreateChatSession(CreateSessionRequest toast)
        {
            string groupID = null;
            //string[] authCred = GetUserCred(uID);
            string createChatSessionUrl = String.Format(ServiceNowConstants.CreateChatSession, ServiceNowConstants.snowinstance, ServiceNowConstants.CustomerSupportQueueID);
            //var authHeaders = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes(authCred[0] + ":" + authCred[1])));
            var authHeaders = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes(ServiceNowConstants.SnowApiUserName + ":" + ServiceNowConstants.SnowApiPassword)));
            var inpuJsonString = JsonConvert.SerializeObject(toast);
            httpClient.DefaultRequestHeaders.Authorization = authHeaders;
            httpClient.DefaultRequestHeaders.Add("Accept", GlobalConstants.ContentTypeJson);
            var request = new HttpRequestMessage(HttpMethod.Post, createChatSessionUrl) { Content = new StringContent(inpuJsonString, Encoding.UTF8, GlobalConstants.ContentTypeJson) };
            try
            {
                var response = httpClient.SendAsync(request).Result;
                if (!response.IsSuccessStatusCode) return null;
                var chatSessionRequestResult = response.Content.ReadAsStringAsync();
                var chatSessionDetails = JsonConvert.DeserializeObject<CreateSessionResponse>(chatSessionRequestResult.Result);
                groupID = chatSessionDetails.result.group;
            }
            catch (Exception e)
            {
                e.ToString();
            }
            return groupID;
        }

        public bool PostChatToLiveAgent(PostMessageToGroupRquest message, string groupID)
        {
            bool postStatus = false;
            string postChatToAgentUrl = String.Format(ServiceNowConstants.PostMessageToChatID, ServiceNowConstants.snowinstance, groupID);
            //var authHeaders = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes(authCred[0] + ":" + authCred[1])));
            var authHeaders = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes(ServiceNowConstants.SnowApiUserName + ":" + ServiceNowConstants.SnowApiPassword)));
            var inpuJsonString = JsonConvert.SerializeObject(message);
            httpClient.DefaultRequestHeaders.Authorization = authHeaders;
            httpClient.DefaultRequestHeaders.Add("Accept", GlobalConstants.ContentTypeJson);
            var request = new HttpRequestMessage(HttpMethod.Post, postChatToAgentUrl) { Content = new StringContent(inpuJsonString, Encoding.UTF8, GlobalConstants.ContentTypeJson) };
            try
            {
                var response = httpClient.SendAsync(request).Result;
                if (!response.IsSuccessStatusCode)
                {
                    return postStatus;
                }
                else
                {
                    var postChatToAgentRequestResult = response.Content.ReadAsStringAsync();
                    var postChatToAgentDetails = JsonConvert.DeserializeObject<PostMessageToGroupResponse>(postChatToAgentRequestResult.Result);
                    if(postChatToAgentDetails.result.formatted_message == message.message)
                    {
                        postStatus = true;
                    }    
                }            
            }
            catch (Exception e)
            {
                e.ToString();
            }

            return postStatus;
        }

        public string GetAgentName(string groupID)
        {
            string agentName = null;
            string getMessageFromAgentUrl = String.Format(ServiceNowConstants.GetMessageFromChat, ServiceNowConstants.snowinstance, groupID);
            //var authHeaders = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes(authCred[0] + ":" + authCred[1])));
            var authHeaders = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes(ServiceNowConstants.SnowApiUserName + ":" + ServiceNowConstants.SnowApiPassword)));
            httpClient.DefaultRequestHeaders.Authorization = authHeaders;
            httpClient.DefaultRequestHeaders.Add("Accept", GlobalConstants.ContentTypeJson);
            var request = new HttpRequestMessage(HttpMethod.Get, getMessageFromAgentUrl);
            try
            {
                var response = httpClient.SendAsync(request).Result;
                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }
                var getMessageFromAgentRequestResult = response.Content.ReadAsStringAsync();
                var messageFromAgentObj = JsonConvert.DeserializeObject<GetMessageToGroupResponse>(getMessageFromAgentRequestResult.Result);
                if (messageFromAgentObj.result[0].created_by != "system" && messageFromAgentObj.result[0].created_by != "nlp_bot")
                {
                    agentName = messageFromAgentObj.result[0].created_by.ToString();
                }
            }
            catch (Exception e)
            {
                e.ToString();
            }
            return agentName;
        }
        public string[] GetMessageFromLiveAgent(string groupID, string agent)
        {
            string[] messageFromAgent = new string[3];

            string getMessageFromAgentUrl = String.Format(ServiceNowConstants.GetMessageFromChat, ServiceNowConstants.snowinstance, groupID);
            //var authHeaders = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes(authCred[0] + ":" + authCred[1])));
            var authHeaders = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes(ServiceNowConstants.SnowApiUserName + ":" + ServiceNowConstants.SnowApiPassword)));
            httpClient.DefaultRequestHeaders.Authorization = authHeaders;
            httpClient.DefaultRequestHeaders.Add("Accept", GlobalConstants.ContentTypeJson);
            var request = new HttpRequestMessage(HttpMethod.Get, getMessageFromAgentUrl);
            try
            {
                var response = httpClient.SendAsync(request).Result;
                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }
                var getMessageFromAgentRequestResult = response.Content.ReadAsStringAsync();
                var messageFromAgentObj = JsonConvert.DeserializeObject<GetMessageToGroupResponse>(getMessageFromAgentRequestResult.Result);
                if (messageFromAgentObj.result[0].created_by == agent)
                {
                    if(messageFromAgentObj.result[0].formatted_message.Contains("left the support session") || messageFromAgentObj.result[0].formatted_message.Contains("closed the support session"))
                    {
                        messageFromAgent[0] = "Session_End";
                        messageFromAgent[1] = messageFromAgentObj.result[0].created_by.ToString();
                        messageFromAgent[2] = messageFromAgentObj.result.Count().ToString();
                    }
                    else if(messageFromAgentObj.result[0].formatted_message.Contains("Thank you for contacting support"))
                    {
                        messageFromAgent[0] = "Session_Start_Agent";
                        messageFromAgent[1] = messageFromAgentObj.result[0].created_by.ToString();
                        messageFromAgent[2] = messageFromAgentObj.result.Count().ToString();
                    }
                    else
                    {
                        messageFromAgent[0] = messageFromAgentObj.result[0].formatted_message;
                        messageFromAgent[1] = messageFromAgentObj.result[0].created_by.ToString();
                        messageFromAgent[2] = messageFromAgentObj.result.Count().ToString();
                    }
                    
                }
                else
                {
                    messageFromAgent[0] = null;
                    messageFromAgent[1] = null;
                    messageFromAgent[2] = messageFromAgentObj.result.Count().ToString();
                }
            }
            catch (Exception e)
            {
                e.ToString();
            }


            return messageFromAgent;
        }

        private string DecryptBase64(string encryptedString)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(encryptedString);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);

        }
    }
}


//if(messageFromAgentObj.result[0].created_by != "system" && messageFromAgentObj.result[0].created_by != "nlp_bot")
//                {
//                    messageFromAgent[0] = messageFromAgentObj.result[0].formatted_message;
//                    messageFromAgent[1] = messageFromAgentObj.result[0].created_by.ToString();
//                    messageFromAgent[2] = messageFromAgentObj.result.Count().ToString();
//                }
//                else if(messageFromAgentObj.result[0].created_by == "system")
//                {
//                    if(messageFromAgentObj.result[0].formatted_message.Contains("left the support session") || messageFromAgentObj.result[0].formatted_message.Contains("closed the support session"))
//                    {
//                        messageFromAgent[0] = "Session_End";
//                        messageFromAgent[1] = messageFromAgentObj.result[0].created_by.ToString();
//                        messageFromAgent[2] = messageFromAgentObj.result.Count().ToString();
//                    }
//                    else
//                    {
//                        messageFromAgent[0] = messageFromAgentObj.result[0].formatted_message;
//                        messageFromAgent[1] = messageFromAgentObj.result[0].created_by.ToString();
//                        messageFromAgent[2] = messageFromAgentObj.result.Count().ToString();
//                    }
//                }
//                else
//                {
//                    messageFromAgent[0] = "No Message";
//                    messageFromAgent[1] = null;
//                    messageFromAgent[2] = messageFromAgentObj.result.Count().ToString();
//                }