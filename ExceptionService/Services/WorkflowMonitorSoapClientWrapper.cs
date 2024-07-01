using ExceptionService.Interfaces;
using WorkFlowMonitorServiceReference;

namespace ExceptionService.Services
{
    public class WorkflowMonitorSoapClientWrapper : IWorkflowMonitorSoapClient
    {
        private readonly WorkflowMonitorClient _client;

        public WorkflowMonitorSoapClientWrapper(WorkflowMonitorClient client)
        {
            _client = client;
        }

        public Task<StandardSoapResponseOfboolean> ReprocessEnrouteExceptionsAsync(WorkflowExceptionModelOfSetEmployeeToEnRouteRequestbwABAbVO request, string adUserName)
        {
            return _client.ReprocessEnrouteExceptionsAsync(request, adUserName);
        }

        public Task<StandardSoapResponseOfboolean> ReprocessOnSiteExceptionsAsync(WorkflowExceptionModelOfSetEmployeeToOnSiteRequestbwABAbVO request, string adUserName)
        {
            return _client.ReprocessOnSiteExceptionsAsync(request, adUserName);
        }

        public Task<StandardSoapResponseOfboolean> ReprocessClearAppointmentExceptionsAsync(WorkflowExceptionModelOfClearAppointmentRequestModelT2o2hOfe request, string adUserName)
        {
            return _client.ReprocessClearAppointmentExceptionsAsync(request, adUserName);
        }
    }
}
