using System;
namespace TSOpsExceptionService.Configuration.Models
{
    public sealed class SoapEndpointOptions
    {
        public string JobServiceBaseUrl { get; set; }
        public string WorkflowMonitorServiceBaseUrl { get; set; }
    }
}
