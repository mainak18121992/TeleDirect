using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceNowClassLibrary.Models
{
    /// <summary>
    /// Model for capturing information related to user from Service Now.
    /// </summary>
    public class UserModel
    {
        public UserResults[] result { get; set; }
    }

    public class UserResults
    {
        public string calendar_integration { get; set; }
        public string country { get; set; }
        public string user_password { get; set; }
        public string last_login_time { get; set; }
        public string source { get; set; }
        public string sys_updated_on { get; set; }
        public string building { get; set; }
        public string web_service_access_only { get; set; }
        public string notification { get; set; }
        public string sys_updated_by { get; set; }
        public string sso_source { get; set; }
        public string sys_created_on { get; set; }
        public Sys_Domain_User sys_domain { get; set; }
        public string u_send_proactive_notfications { get; set; }
        public string state { get; set; }
        public string vip { get; set; }
        public string sys_created_by { get; set; }
        public string zip { get; set; }
        public string home_phone { get; set; }
        public string time_format { get; set; }
        public string last_login { get; set; }
        public string default_perspective { get; set; }
        public string active { get; set; }
        public string average_daily_fte { get; set; }
        public string time_sheet_policy { get; set; }
        public string sys_domain_path { get; set; }
        public Cost_Center_User cost_center { get; set; }
        public string phone { get; set; }
        public string name { get; set; }
        public string employee_number { get; set; }
        public string password_needs_reset { get; set; }
        public string gender { get; set; }
        public string city { get; set; }
        public string failed_attempts { get; set; }
        public string user_name { get; set; }
        public string roles { get; set; }
        public string title { get; set; }
        public string u_date_of_joining { get; set; }
        public string sys_class_name { get; set; }
        public string sys_id { get; set; }
        public string internal_integration_user { get; set; }
        public string ldap_server { get; set; }
        public string mobile_phone { get; set; }
        public string street { get; set; }
        public Company_User company { get; set; }
        public Department_User department { get; set; }
        public string first_name { get; set; }
        public string email { get; set; }
        public string introduction { get; set; }
        public string preferred_language { get; set; }
        public Manager_User manager { get; set; }
        public string locked_out { get; set; }
        public string sys_mod_count { get; set; }
        public string last_name { get; set; }
        public string photo { get; set; }
        public string middle_name { get; set; }
        public string sys_tags { get; set; }
        public string time_zone { get; set; }
        public string schedule { get; set; }
        public string date_format { get; set; }
        public Location_User location { get; set; }
    }

    public class Sys_Domain_User
    {
        public string link { get; set; }
        public string value { get; set; }
    }

    public class Cost_Center_User
    {
        public string link { get; set; }
        public string value { get; set; }
    }

    public class Company_User
    {
        public string link { get; set; }
        public string value { get; set; }
    }

    public class Department_User
    {
        public string link { get; set; }
        public string value { get; set; }
    }

    public class Manager_User
    {
        public string link { get; set; }
        public string value { get; set; }
    }

    public class Location_User
    {
        public string link { get; set; }
        public string value { get; set; }
    }

}
