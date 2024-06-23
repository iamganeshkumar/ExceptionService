using ExceptionService.Enums;
using ExceptionService.Interfaces;
using ExceptionService.Mock.Response;
using ExceptionService.Models;
using ExceptionService.Requests;

namespace ExceptionService.Mock.Services
{
    public class DevWorkflowMonitorServiceClient : IWorkflowMonitorServiceClient
    {
        public Task<StandardSoapResponse> ReprocessEnrouteExceptionsAsync(WorkflowExceptionRequest reprocessRequest, string adUserName)
        {
            // Mock response
            var response = new MockStandardSoapResponse
            {
                ReturnValue = true // Set mock response value
            };

            return Task.FromResult(new StandardSoapResponse { ReturnValue = response.ReturnValue });
        }

        public Task<StandardSoapResponse> ReprocessOnSiteExceptionsAsync(WorkflowExceptionRequest reprocessRequest, string adUserName)
        {
            // Mock response
            var response = new MockStandardSoapResponse
            {
                ReturnValue = true // Set mock response value
            };

            return Task.FromResult(new StandardSoapResponse { ReturnValue = response.ReturnValue });
        }

        public Task<StandardSoapResponse> ReprocessClearAppointmentExceptionsAsync(WorkflowExceptionRequest reprocessRequest, string adUserName)
        {
            // Mock response
            var response = new MockStandardSoapResponse
            {
                ReturnValue = true // Set mock response value
            };

            return Task.FromResult(new StandardSoapResponse { ReturnValue = response.ReturnValue });
        }

        public CommonExceptionType MapServiceExceptionTypeToCommonExceptionType(object serviceExceptionType)
        {
            // Map mock exception types
            return serviceExceptionType switch
            {
                "Enroute" => CommonExceptionType.Enroute,
                "OnSite" => CommonExceptionType.OnSite,
                "Clear" => CommonExceptionType.Clear,
                _ => throw new ArgumentOutOfRangeException(nameof(serviceExceptionType), $"Not expected exception type value: {serviceExceptionType}")
            };
        }
    }

}
