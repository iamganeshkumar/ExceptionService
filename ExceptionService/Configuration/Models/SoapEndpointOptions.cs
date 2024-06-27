using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExceptionService.Configuration.Models
{
    public sealed class SoapEndpointOptions
    {
        public string JobServiceBaseUrl { get; set; }
        public string WorkflowMonitorServiceBaseUrl { get; set; }
    }
}
