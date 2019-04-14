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
        /// <summary>
        /// 
        /// 
        /// </summary>
        /// <remarks>Mainak Chatterjee (mchatterjee@hpe.com) </remarks>
        /// <param name=""></param>
        /// <returns></returns>
        private enum _requestStatus
        {
            Valid,
            NotForUser,
            InValid,
            Failed
        }

        /// <summary>
        /// 
        /// 
        /// </summary>
        /// <remarks>Mainak Chatterjee (mchatterjee@hpe.com) </remarks>
        /// <param name=""></param>
        /// <returns></returns>
        public Dictionary<string, dynamic> GetRequestWithStatusByNumber(string requestID, string userID)
        {
            Dictionary<string, dynamic> validRequestOutput = new Dictionary<string, dynamic>();
            Dictionary<string, dynamic> validRequest = new Dictionary<string, dynamic>();
            validRequest = GetRequestOnValidation(requestID, userID);
            if (validRequest["Validity"] == _requestStatus.Valid)
            {
                validRequestOutput.Add("Ticket_Details", validRequest["RequestInfo"]);
                validRequestOutput.Add("Success", true);
                validRequestOutput.Add("Status", _requestStatus.Valid.ToString());
            }
            else if (validRequest["Validity"] == _requestStatus.NotForUser)
            {
                validRequestOutput.Add("Ticket_Details", null);
                validRequestOutput.Add("Success", false);
                validRequestOutput.Add("Status", _requestStatus.NotForUser.ToString());
                validRequestOutput.Add("Exception", "User queried a request which is not realated to the user.");
            }
            else if (validRequest["Validity"] == _requestStatus.InValid)
            {
                validRequestOutput.Add("Ticket_Details", null);
                validRequestOutput.Add("Success", false);
                validRequestOutput.Add("Status", _requestStatus.InValid.ToString());
                validRequestOutput.Add("Exception", "User queried a request which does not exist.");
            }
            else if (validRequest["Validity"] == _requestStatus.Failed)
            {
                validRequestOutput.Add("Ticket_Details", null);
                validRequestOutput.Add("Success", false);
                validRequestOutput.Add("Status", _requestStatus.Failed.ToString());
                validRequestOutput.Add("Exception", validRequest["Exception"]);
            }

            return validRequestOutput;
        }
        /// <summary>
        /// 
        /// 
        /// </summary>
        /// <remarks>Mainak Chatterjee (mchatterjee@hpe.com) </remarks>
        /// <param name=""></param>
        /// <returns></returns>
        public Dictionary<string, dynamic> GetRequestWithStatusByUser(string userID)
        {
            Dictionary<string, dynamic> getRequestlist = new Dictionary<string, dynamic>();
            getRequestlist = GetRequestListByUser(userID);
            return getRequestlist;
        }
        /// <summary>
        /// 
        /// 
        /// </summary>
        /// <remarks>Mainak Chatterjee (mchatterjee@hpe.com) </remarks>
        /// <param name=""></param>
        /// <returns></returns>
        public Dictionary<string, dynamic> GetRequestWithStatusForUser(string userID)
        {
            Dictionary<string, dynamic> getRequestlist = new Dictionary<string, dynamic>();
            getRequestlist = GetRequestListForUser(userID);
            return getRequestlist;
        }


        /// <summary>
        /// 
        /// 
        /// </summary>
        /// <remarks>Mainak Chatterjee (mchatterjee@hpe.com) </remarks>
        /// <param name=""></param>
        /// <returns></returns>
        private Dictionary<string, dynamic> GetRequestOnValidation(string requestID, string userID)
        {
            Dictionary<string, dynamic> _detailedRequestDict = new Dictionary<string, dynamic>();
            ServiceRequestModel _detailedRequest = new ServiceRequestModel();
            string validateRequestUrl = String.Format(ServiceNowConstants.GetServiceRequestStatus, ServiceNowConstants.snowinstance, requestID);
            var authHeaders = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes(ServiceNowConstants.SnowApiUserName + ":" + ServiceNowConstants.SnowApiPassword)));
            httpClient.DefaultRequestHeaders.Authorization = authHeaders;
            httpClient.DefaultRequestHeaders.Add("Accept", GlobalConstants.ContentTypeJson);
            var request = new HttpRequestMessage(HttpMethod.Get, validateRequestUrl);
            try
            {
                var response = httpClient.SendAsync(request).Result;
                if (response.IsSuccessStatusCode)
                {
                    var requestValidationResult = response.Content.ReadAsStringAsync();
                    _detailedRequest = JsonConvert.DeserializeObject<ServiceRequestModel>(requestValidationResult.Result);
                    if (_detailedRequest.result[0].sys_id != null)
                    {
                        if (_detailedRequest.result[0].opened_by.value == userID || _detailedRequest.result[0].requested_for.value == userID)
                        {
                            _detailedRequestDict.Add("RequestInfo", _detailedRequest);
                            _detailedRequestDict.Add("Validity", _requestStatus.Valid);
                        }
                        else
                        {
                            _detailedRequestDict.Add("RequestInfo", _detailedRequest);
                            _detailedRequestDict.Add("Validity", _requestStatus.NotForUser);
                        }
                    }
                    else
                    {
                        _detailedRequestDict.Add("RequestInfo", _detailedRequest);
                        _detailedRequestDict.Add("Validity", _requestStatus.InValid);
                    }
                }
                else
                {
                    _detailedRequestDict.Add("RequestInfo", null);
                    _detailedRequestDict.Add("Validity", _requestStatus.Failed);
                    _detailedRequestDict.Add("Exception", "Failed to make http request.");
                    _detailedRequestDict.Add("Status Code", response.StatusCode.ToString());
                }

            }
            catch (Exception e)
            {
                _detailedRequestDict.Add("RequestInfo", null);
                _detailedRequestDict.Add("Validity", _requestStatus.Failed);
                _detailedRequestDict.Add("Exception", e.ToString());
            }
            return _detailedRequestDict;
        }
        /// <summary>
        /// 
        /// 
        /// </summary>
        /// <remarks>Mainak Chatterjee (mchatterjee@hpe.com) </remarks>
        /// <param name=""></param>
        /// <returns></returns>
        private Dictionary<string, dynamic> GetRequestListForUser(string userID)
        {
            Dictionary<string, dynamic> dictionary = new Dictionary<string, dynamic>();
            ServiceRequestModel requestListForUser = new ServiceRequestModel();
            string getRequestsForUserUrl = String.Format(ServiceNowConstants.GetServiceRequestListForUser, ServiceNowConstants.snowinstance, userID);
            var authHeaders = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes(ServiceNowConstants.SnowApiUserName + ":" + ServiceNowConstants.SnowApiPassword)));
            httpClient.DefaultRequestHeaders.Authorization = authHeaders;
            httpClient.DefaultRequestHeaders.Add("Accept", GlobalConstants.ContentTypeJson);
            var request = new HttpRequestMessage(HttpMethod.Get, getRequestsForUserUrl);
            try
            {
                var response = httpClient.SendAsync(request).Result;
                if (response.IsSuccessStatusCode)
                {
                    var requestListResult = response.Content.ReadAsStringAsync();
                    requestListForUser = JsonConvert.DeserializeObject<ServiceRequestModel>(requestListResult.Result);
                    dictionary.Add("Request_List", requestListForUser);
                    dictionary.Add("Success", true);
                }
                else
                {
                    dictionary.Add("Request_List", null);
                    dictionary.Add("Success", false);
                    dictionary.Add("Exception", "Failed to make http request.");
                    dictionary.Add("Status Code", response.StatusCode.ToString());
                }

            }
            catch (Exception e)
            {
                dictionary.Add("Request_List", null);
                dictionary.Add("Success", false);
                dictionary.Add("Exception", e.ToString());
            }
            return dictionary;
        }
        /// <summary>
        /// 
        /// 
        /// </summary>
        /// <remarks>Mainak Chatterjee (mchatterjee@hpe.com) </remarks>
        /// <param name=""></param>
        /// <returns></returns>
        private Dictionary<string, dynamic> GetRequestListByUser(string userID)
        {
            Dictionary<string, dynamic> dictionary = new Dictionary<string, dynamic>();
            ServiceRequestModel requestListByUser = new ServiceRequestModel();
            string getRequestsByUserUrl = String.Format(ServiceNowConstants.GetServiceRequestListByUser, ServiceNowConstants.snowinstance, userID);
            var authHeaders = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes(ServiceNowConstants.SnowApiUserName + ":" + ServiceNowConstants.SnowApiPassword)));
            httpClient.DefaultRequestHeaders.Authorization = authHeaders;
            httpClient.DefaultRequestHeaders.Add("Accept", GlobalConstants.ContentTypeJson);
            var request = new HttpRequestMessage(HttpMethod.Get, getRequestsByUserUrl);
            try
            {
                var response = httpClient.SendAsync(request).Result;
                if (response.IsSuccessStatusCode)
                {
                    var requestListResult = response.Content.ReadAsStringAsync();
                    requestListByUser = JsonConvert.DeserializeObject<ServiceRequestModel>(requestListResult.Result);
                    dictionary.Add("Request_List", requestListByUser);
                    dictionary.Add("Success", true);
                }
                else
                {
                    dictionary.Add("Request_List", null);
                    dictionary.Add("Success", false);
                    dictionary.Add("Exception", "Failed to make http request.");
                    dictionary.Add("Status Code", response.StatusCode.ToString());
                }

            }
            catch (Exception e)
            {
                dictionary.Add("Request_List", null);
                dictionary.Add("Success", false);
                dictionary.Add("Exception", e.ToString());
            }
            return dictionary;
        }


    }
}
