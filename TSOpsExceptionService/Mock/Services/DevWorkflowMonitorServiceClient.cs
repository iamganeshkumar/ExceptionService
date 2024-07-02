using ExceptionService.Interfaces;
using ExceptionService.Mock.Response;
using ExceptionService.Requests;
using WorkFlowMonitorServiceReference;

namespace ExceptionService.Mock.Services
{
    public class DevWorkflowMonitorServiceClient : IWorkflowMonitorServiceClient
    {
        public Task<StandardSoapResponseOfboolean> ReprocessEnrouteExceptionsAsync(WorkflowExceptionRequest reprocessRequest, string adUserName)
        {
            // Mock response
            var response = new MockStandardSoapResponse
            {
                ReturnValue = true // Set mock response value
            };

            return Task.FromResult(new StandardSoapResponseOfboolean { ReturnValue = response.ReturnValue });
        }

        public Task<StandardSoapResponseOfboolean> ReprocessOnSiteExceptionsAsync(WorkflowExceptionRequest reprocessRequest, string adUserName)
        {
            // Mock response
            var response = new MockStandardSoapResponse
            {
                ReturnValue = true // Set mock response value
            };

            return Task.FromResult(new StandardSoapResponseOfboolean { ReturnValue = response.ReturnValue });
        }

        public Task<StandardSoapResponseOfboolean> ReprocessClearAppointmentExceptionsAsync(WorkflowExceptionRequest reprocessRequest, string adUserName)
        {
            // Mock response
            var response = new MockStandardSoapResponse
            {
                ReturnValue = true // Set mock response value
            };

            return Task.FromResult(new StandardSoapResponseOfboolean { ReturnValue = response.ReturnValue });
        }
    }
}
