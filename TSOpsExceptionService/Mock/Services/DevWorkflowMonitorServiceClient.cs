using TSOpsExceptionService.Interfaces;
using TSOpsExceptionService.Mock.Response;
using TSOpsExceptionService.Requests;
using WorkFlowMonitorServiceReference;

namespace TSOpsExceptionService.Mock.Services
{
    public class DevWorkflowMonitorServiceClient : IWorkflowMonitorServiceClient
    {
        public Task<StandardSoapResponseOfboolean> ReprocessEnrouteExceptionsAsync(WorkflowExceptionRequest reprocessRequest, string adUserName)
        {
            // Mock response
            try
            {
                var response = new MockStandardSoapResponse
                {
                    ReturnValue = true // Set mock response value
                };
                
                return Task.FromResult(new StandardSoapResponseOfboolean { ReturnValue = response.ReturnValue });
            }
            catch (Exception ex) 
            {
                return Task.FromResult<StandardSoapResponseOfboolean>(null);
            }
        }

        public Task<StandardSoapResponseOfboolean> ReprocessOnSiteExceptionsAsync(WorkflowExceptionRequest reprocessRequest, string adUserName)
        {
            // Mock response
            try
            {
                var response = new MockStandardSoapResponse
                {
                    ReturnValue = true // Set mock response value
                };
                return Task.FromResult(new StandardSoapResponseOfboolean { ReturnValue = response.ReturnValue });
            }
            catch (Exception ex)
            {
                return Task.FromResult<StandardSoapResponseOfboolean>(null);
            }
        }

        public Task<StandardSoapResponseOfboolean> ReprocessClearAppointmentExceptionsAsync(WorkflowExceptionRequest reprocessRequest, string adUserName)
        {
            // Mock response
            try
            {
                var response = new MockStandardSoapResponse
                {
                    ReturnValue = true // Set mock response value
                };

                return Task.FromResult(new StandardSoapResponseOfboolean { ReturnValue = response.ReturnValue });
            }
            catch (Exception ex)
            {
                return Task.FromResult<StandardSoapResponseOfboolean>(null);
            }
        }
    }
}
