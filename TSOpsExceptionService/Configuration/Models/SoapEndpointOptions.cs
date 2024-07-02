using System;
namespace ExceptionService.Configuration.Models
{
    public sealed class SoapEndpointOptions
    {
        public string JobServiceBaseUrl { get; set; }
        public string WorkflowMonitorServiceBaseUrl { get; set; }
    }
}
