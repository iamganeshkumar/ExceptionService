using ExceptionService.Requests;
using WorkFlowMonitorServiceReference;

namespace ExceptionService.Interfaces
{
    public interface IWorkflowMonitorServiceClient
    {
        Task<StandardSoapResponseOfboolean> ReprocessEnrouteExceptionsAsync(WorkflowExceptionRequest reprocessRequest, string adUserName);
        Task<StandardSoapResponseOfboolean> ReprocessOnSiteExceptionsAsync(WorkflowExceptionRequest reprocessRequest, string adUserName);
        Task<StandardSoapResponseOfboolean> ReprocessClearAppointmentExceptionsAsync(WorkflowExceptionRequest reprocessRequest, string adUserName);
    }
}
