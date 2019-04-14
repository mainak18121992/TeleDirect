using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceNowClassLibrary.Models
{
    /// <summary>
    /// Model to capture SNOW Constants varying with customer.
    /// </summary>
    [Serializable]
    public class ServiceNowConstantsModel
    {
        public string snowInstanceID { get; set; }
        public string apiUserName { get; set; }
        public string apiPassword { get; set; }
        public string customerSupportQueueID { get; set; }

    }
}
