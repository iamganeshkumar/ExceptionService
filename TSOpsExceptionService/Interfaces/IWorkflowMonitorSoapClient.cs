using WorkFlowMonitorServiceReference;

namespace TSOpsExceptionService.Interfaces
{
    public interface IWorkflowMonitorSoapClient
    {
        Task<StandardSoapResponseOfboolean> ReprocessEnrouteExceptionsAsync(WorkflowExceptionModelOfSetEmployeeToEnRouteRequestbwABAbVO request, string adUserName);
        Task<StandardSoapResponseOfboolean> ReprocessOnSiteExceptionsAsync(WorkflowExceptionModelOfSetEmployeeToOnSiteRequestbwABAbVO request, string adUserName);
        Task<StandardSoapResponseOfboolean> ReprocessClearAppointmentExceptionsAsync(WorkflowExceptionModelOfClearAppointmentRequestModelT2o2hOfe request, string adUserName);
    }
}
