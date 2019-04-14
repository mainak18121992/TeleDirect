using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceNowClassLibrary.Models
{

    public class GetUserCred
    {
        public GetUserCredResult result { get; set; }
    }

    public class GetUserCredResult
    {
        public string user_password { get; set; }
        public string user_name { get; set; }
        public string name { get; set; }
    }



    public class CreateSessionRequest
    {
        public string message { get; set; }
    }


    public class CreateSessionResponse
    {
        public CreateSessionResponseResult result { get; set; }
    }

    public class CreateSessionResponseResult
    {
        public string sys_id { get; set; }
        public string number { get; set; }
        public string short_description { get; set; }
        public bool transfer_change { get; set; }
        public string opened_by { get; set; }
        public int state { get; set; }
        public string sys_updated_on { get; set; }
        public int position { get; set; }
        public string queue { get; set; }
        public string average_wait_time { get; set; }
        public string group { get; set; }
    }


    public class PostMessageToGroupRquest
    {
        public string message { get; set; }
    }



    public class PostMessageToGroupResponse
    {
        public PostMessageToGroupResponseResult result { get; set; }
    }

    public class PostMessageToGroupResponseResult
    {
        public object[] mentions { get; set; }
        public string profile { get; set; }
        public object[] links { get; set; }
        public object[] attachments { get; set; }
        public bool system { get; set; }
        public string group { get; set; }
        public int order { get; set; }
        public string message { get; set; }
        public string state { get; set; }
        public string id { get; set; }
        public long timestamp { get; set; }
        public bool has_tags { get; set; }
        public bool is_liked { get; set; }
        public string formatted_message { get; set; }
        public int reply_order_chunk { get; set; }
        public string to_profile { get; set; }
        public string created_by { get; set; }
        public bool chat_message { get; set; }
        public string reflected_field { get; set; }
        public bool has_links { get; set; }
        public bool has_attachments { get; set; }
        public string sys_id { get; set; }
        public string reply_to { get; set; }
        public string created_on { get; set; }
        public bool is_private { get; set; }
    }



    public class GetMessageToGroupResponse
    {
        public GetMessageToGroupResponseResult[] result { get; set; }
    }

    public class GetMessageToGroupResponseResult
    {
        public object[] mentions { get; set; }
        public string profile { get; set; }
        public object[] links { get; set; }
        public object[] attachments { get; set; }
        public bool system { get; set; }
        public string group { get; set; }
        public string context { get; set; }
        public int order { get; set; }
        public string message { get; set; }
        public string state { get; set; }
        public string id { get; set; }
        public long timestamp { get; set; }
        public bool has_tags { get; set; }
        public bool is_liked { get; set; }
        public string formatted_message { get; set; }
        public int reply_order_chunk { get; set; }
        public string to_profile { get; set; }
        public string created_by { get; set; }
        public bool chat_message { get; set; }
        public string reflected_field { get; set; }
        public bool has_links { get; set; }
        public bool has_attachments { get; set; }
        public string sys_id { get; set; }
        public string reply_to { get; set; }
        public string created_on { get; set; }
        public bool is_private { get; set; }
    }

}
