using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ServiceNowClassLibrary
{
    public partial class ServiceNowActivity
    {
        private static readonly HttpClient httpClient;
        static ServiceNowActivity()
        {
            httpClient = new HttpClient();
        }
    }
}
