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
using ServiceNowClassLibrary.Interfaces;
using System.Threading;

namespace ServiceNowClassLibrary
{
    /// <summary>
    /// This IAgentSession class manages a session with a queue/agent using the Service Now Connect Chat
    /// </summary>
    /// <seealso cref="ServiceNowClassLibrary.Interfaces.IAgentSession" />
    public class SNOWSession : IAgentSession
    {
        private readonly HttpClient httpClient;
        private string agentName = string.Empty;
        private string groupID = string.Empty;
        private enum _messageType
        {
            SystemMessage,
            ReplyFromAgent,
            SessionCloseByAgent,
            SessionTerminated,
            AgentPickedUpMessage,
            Invalid
        }
        /// <summary>
        /// Indicates if the session is active.
        /// </summary>
        private bool connected = false;

        /// <summary>
        /// A list of messages to be forwarded to the live agent once the connection has been established.
        /// </summary>
        private List<string> initialMessageList = new List<string>();

        /// <summary>
        /// Callback Action for module termination.
        /// </summary>
        private Action terminationCallback;
        public SNOWSession()
        {
            httpClient = new HttpClient();
        }
        #region Event Handlers
        /// <summary>
        /// Informs the Service Controller that a message from the XMPP session has been received.
        /// </summary>
        public event EventHandler MessageFromAgent;
        /// <summary>
        /// Informs the Service Controller of a log message.
        /// </summary>
        public event EventHandler LogMessage;
        /// <summary>
        /// Informs the Service Controller that the XMPP session has been closed.
        /// </summary>
        public event EventHandler SessionEnded;
        /// <summary>
        /// Informs the Service Controller that a connection to the queue has been established.
        /// This generally indicates that messages can now be forwarded to the agent.
        /// </summary>
        public event EventHandler SessionInitiated;
        /// <summary>
        /// informs the service controller that a message should be sent to the user.
        /// the message is described by a phraseid, so localization does not need to be handled in this module.
        /// </summary>
        public event EventHandler PhraseFromSession;
        /// <summary>
        /// Informs the Service Controller that an agent has responded.
        /// Passes the agent's handle.
        /// </summary>
        public event EventHandler AgentConnected;
        /// <summary>
        /// Informs the service controller that a connection error has occured.
        /// </summary>
        public event EventHandler ConnectionError;
        #endregion Event Handlers

        public void CloseSession()
        {
            connected = false;
            LogMessage("Attempting to close XMPP session...", null);
        }

        public void SendMessage(string message)
        {
            throw new NotImplementedException();
        }

        public void SendMessage(PostMessageToGroupRquest message, string groupID)
        {
            if (connected)
            {
                LogMessage("Sending message to agent/queue : " + message, null);
                Dictionary<string, dynamic> sendMessagetoSnow = new Dictionary<string, dynamic>();
                sendMessagetoSnow = SendMessageToSnow(message, groupID);
                if (sendMessagetoSnow["Status"] == true)
                {
                    LogMessage("Sent message to agent/queue : " + message, null);
                }
                else
                {
                    LogMessage("Sending message'" + message + "' failed with error :" + sendMessagetoSnow["Error"] + "and error code :"+ sendMessagetoSnow["StatusCode"], null);
                }
            }
            else
            {
                LogMessage("Sending message failed. Session not established." + message, null);
            }

        }

        public void StartSession(string uri, string initialMessage)
        {
            throw new NotImplementedException();
        }

        public void StartSession(string uri, List<string> initialMessageList)
        {
            throw new NotImplementedException();
        }

        public void StartSession(CreateSessionRequest toast, string user)
        {
            Dictionary<string, dynamic> startSnowSession = new Dictionary<string, dynamic>();
            Dictionary<string, dynamic> agentDetails = new Dictionary<string, dynamic>();
            startSnowSession = StartSnowSession(toast);
            if(startSnowSession["Status"] == true)
            {
                groupID = startSnowSession["GroupID"];
                SessionInitiated(groupID, null);
                do
                {
                    agentDetails = GetAgentName(groupID);
                    if(agentDetails["Success"] == true)
                    {
                        connected = true;
                        agentName = agentDetails["AgentName"];
                        AgentConnected(agentName, null);
                        break;
                    }
                }
                while (this.agentName == null);
                string message = String.Format("Hello {0}. Hope you are doing well. I am {1}. How may I assist you today ?", user, agentName);
                MessageFromAgent(message, null);

                Thread thread = new Thread(() => {
                    while (connected)
                    {
                        int messageCount = 0;
                        Dictionary<string, dynamic> messageFromAgent = new Dictionary<string, dynamic>();
                        messageFromAgent = GetMessageFromSnow(groupID, agentName);
                        if (messageFromAgent["Success"] == true)
                        {
                            int value;
                            bool isSuccess = int.TryParse(messageFromAgent["MessageCounter"], out value);
                            if (value > messageCount)
                            {
                                if (messageFromAgent["MessageType"] == _messageType.SessionTerminated)
                                {
                                    messageCount = 0;
                                    value = 0;
                                    LogMessage("Agent has terminated the session.", null);
                                    SessionEnded("Terminated by agent", null);

                                }
                                else if (messageFromAgent["MessageType"] == _messageType.SessionCloseByAgent)
                                {
                                    messageCount = 0;
                                    value = 0;
                                    LogMessage("Agent has left the session.", null);
                                    SessionEnded("Agent left the session.", null);
                                }
                                else if (messageFromAgent["MessageType"] == _messageType.ReplyFromAgent)
                                {
                                    messageCount = value;
                                    LogMessage("Received Message from Agent.", null);
                                    MessageFromAgent(messageFromAgent["Message"], null);
                                }
                                else if (messageFromAgent["MessageType"] == _messageType.AgentPickedUpMessage)
                                {
                                    messageCount = value;
                                    LogMessage("Agent has picked up the session.", null);
                                }
                                else if (messageFromAgent["MessageType"] == _messageType.SystemMessage)
                                {
                                    messageCount = value;
                                    PhraseFromSession(messageFromAgent["Message"], null);
                                }
                            }
                        }
                        Thread.Sleep(1000);
                    }
                });
                thread.IsBackground = true;
                thread.Start();
            }
            else
            {
                LogMessage("Failed to start session", null);
                ConnectionError(1, null);
            }
        }

        public void Terminate(Action a)
        {
            LogMessage("Terminating SNOW Session Controller.", null);
            terminationCallback = a;
            CloseSession();
            Terminated();
        }
        private void Terminated()
        {
            terminationCallback();
        }

        private Dictionary<string, dynamic> GetMessageFromSnow(string groupID, string agent)
        {
            Dictionary<string, dynamic> messageFromSession = new Dictionary<string, dynamic>();
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
                    messageFromSession.Add("Success", false);
                    messageFromSession.Add("Message", null);
                    messageFromSession.Add("MessageType", _messageType.Invalid);
                    messageFromSession.Add("Error", "HTTPS Request Failed.");
                    messageFromSession.Add("StatusCode", response.StatusCode);
                }
                else
                {
                    var getMessageFromAgentRequestResult = response.Content.ReadAsStringAsync();
                    var messageFromAgentObj = JsonConvert.DeserializeObject<GetMessageToGroupResponse>(getMessageFromAgentRequestResult.Result);
                    if (messageFromAgentObj.result[0].created_by == agent)
                    {
                        if (messageFromAgentObj.result[0].formatted_message.Contains("left the support session"))
                        {
                            messageFromSession.Add("Success", true);
                            messageFromSession.Add("Message", messageFromAgentObj.result[0].formatted_message);
                            messageFromSession.Add("MessageType", _messageType.SessionTerminated);
                            messageFromSession.Add("Error", null);
                            messageFromSession.Add("StatusCode", response.StatusCode);
                            messageFromSession.Add("MessageCounter", messageFromAgentObj.result.Count());
                        }
                        else if (messageFromAgentObj.result[0].formatted_message.Contains("closed the support session"))
                        {
                            messageFromSession.Add("Success", true);
                            messageFromSession.Add("Message", messageFromAgentObj.result[0].formatted_message);
                            messageFromSession.Add("MessageType", _messageType.SessionCloseByAgent);
                            messageFromSession.Add("Error", null);
                            messageFromSession.Add("StatusCode", response.StatusCode);
                            messageFromSession.Add("MessageCounter", messageFromAgentObj.result.Count());
                        }
                        else if (messageFromAgentObj.result[0].formatted_message.Contains("Thank you for contacting support"))
                        {
                            messageFromSession.Add("Success", true);
                            messageFromSession.Add("Message", messageFromAgentObj.result[0].formatted_message);
                            messageFromSession.Add("MessageType", _messageType.AgentPickedUpMessage);
                            messageFromSession.Add("Error", null);
                            messageFromSession.Add("StatusCode", response.StatusCode);
                            messageFromSession.Add("MessageCounter", messageFromAgentObj.result.Count());
                        }
                        else
                        {
                            messageFromSession.Add("Success", true);
                            messageFromSession.Add("Message", messageFromAgentObj.result[0].formatted_message);
                            messageFromSession.Add("MessageType", _messageType.ReplyFromAgent);
                            messageFromSession.Add("Error", null);
                            messageFromSession.Add("StatusCode", response.StatusCode);
                            messageFromSession.Add("MessageCounter", messageFromAgentObj.result.Count());
                        }

                    }
                    else
                    {
                        messageFromSession.Add("Success", true);
                        messageFromSession.Add("Message", messageFromAgentObj.result[0].formatted_message);
                        messageFromSession.Add("MessageType", _messageType.SystemMessage);
                        messageFromSession.Add("Error", null);
                        messageFromSession.Add("StatusCode", response.StatusCode);
                        messageFromSession.Add("MessageCounter", messageFromAgentObj.result.Count());
                    }
                }
            }
            catch (Exception e)
            {
                messageFromSession.Add("Success", false);
                messageFromSession.Add("Message", null);
                messageFromSession.Add("MessageType", _messageType.Invalid);
                messageFromSession.Add("Error", e.ToString());
                messageFromSession.Add("StatusCode", e.HResult);
            }

            return messageFromSession;
        }

        private Dictionary<string, dynamic> SendMessageToSnow(PostMessageToGroupRquest message, string groupID)
        {
            Dictionary<string, dynamic> postMessageStatus = new Dictionary<string, dynamic>();
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
                    postMessageStatus.Add("Status", false);
                    postMessageStatus.Add("Error", "HTTPS Request failed.");
                    postMessageStatus.Add("StatusCode", response.StatusCode);
                }
                else
                {
                    var postChatToAgentRequestResult = response.Content.ReadAsStringAsync();
                    var postChatToAgentDetails = JsonConvert.DeserializeObject<PostMessageToGroupResponse>(postChatToAgentRequestResult.Result);
                    if (postChatToAgentDetails.result.formatted_message == message.message)
                    {
                        postMessageStatus.Add("Status", true);
                        postMessageStatus.Add("Error", null);
                        postMessageStatus.Add("StatusCode", response.StatusCode);
                    }
                }
            }
            catch (Exception e)
            {
                
                postMessageStatus.Add("Status", false);
                postMessageStatus.Add("Error", e.ToString());
                postMessageStatus.Add("StatusCode", e.HResult);
            }
            return postMessageStatus;
        }

        private Dictionary<string, dynamic> StartSnowSession(CreateSessionRequest toast)
        {
            Dictionary<string, dynamic> startSessionStatus = new Dictionary<string, dynamic>();
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
                if (response.IsSuccessStatusCode)
                {
                    SessionInitiated(null, null);
                    var chatSessionRequestResult = response.Content.ReadAsStringAsync();
                    var chatSessionDetails = JsonConvert.DeserializeObject<CreateSessionResponse>(chatSessionRequestResult.Result);
                    startSessionStatus.Add("Success", true);
                    startSessionStatus.Add("GroupID", chatSessionDetails.result.group);
                    startSessionStatus.Add("Error", null);
                    startSessionStatus.Add("StatusCode", response.StatusCode);
                }
                else
                {
                    startSessionStatus.Add("Success", false);
                    startSessionStatus.Add("GroupID", null);
                    startSessionStatus.Add("Error", "HTTPS Call failed.");
                    startSessionStatus.Add("StatusCode", response.StatusCode);
                }

            }
            catch (Exception e)
            {
                startSessionStatus.Add("Success", true);
                startSessionStatus.Add("GroupID", null);
                startSessionStatus.Add("Error", e.ToString());
                startSessionStatus.Add("StatusCode", e.HResult);
            }

            return startSessionStatus;
        }

        private Dictionary<string, dynamic> GetAgentName(string groupID)
        {
            Dictionary<string, dynamic> agentDetails = new Dictionary<string, dynamic>();
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
                    agentDetails.Add("AgentName", null);
                    agentDetails.Add("Success", false);
                    agentDetails.Add("Error", "HTTPS Call failed.");
                    agentDetails.Add("StatusCode", response.StatusCode);
                }
                else
                {
                    var getMessageFromAgentRequestResult = response.Content.ReadAsStringAsync();
                    var messageFromAgentObj = JsonConvert.DeserializeObject<GetMessageToGroupResponse>(getMessageFromAgentRequestResult.Result);
                    if (messageFromAgentObj.result[0].created_by != "system" && messageFromAgentObj.result[0].created_by != "nlp_bot")
                    {
                        
                        agentDetails.Add("AgentName", messageFromAgentObj.result[0].created_by.ToString());
                        agentDetails.Add("Success", true);
                        agentDetails.Add("Error", null);
                        agentDetails.Add("StatusCode", response.StatusCode);
                    }
                }
                
            }
            catch (Exception e)
            {
                agentDetails.Add("AgentName", null);
                agentDetails.Add("Success", false);
                agentDetails.Add("Error", e.ToString());
                agentDetails.Add("StatusCode", e.HResult);
            }
            return agentDetails;
        }
    }
}
