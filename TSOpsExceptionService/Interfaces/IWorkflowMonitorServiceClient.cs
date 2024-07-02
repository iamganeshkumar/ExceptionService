using TSOpsExceptionService.Requests;
using WorkFlowMonitorServiceReference;

namespace TSOpsExceptionService.Interfaces
{
    public interface IWorkflowMonitorServiceClient
    {
        Task<StandardSoapResponseOfboolean> ReprocessEnrouteExceptionsAsync(WorkflowExceptionRequest reprocessRequest, string adUserName);
        Task<StandardSoapResponseOfboolean> ReprocessOnSiteExceptionsAsync(WorkflowExceptionRequest reprocessRequest, string adUserName);
        Task<StandardSoapResponseOfboolean> ReprocessClearAppointmentExceptionsAsync(WorkflowExceptionRequest reprocessRequest, string adUserName);
    }
}
