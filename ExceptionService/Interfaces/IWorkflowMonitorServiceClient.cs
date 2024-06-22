using ExceptionService.Enums;
using ExceptionService.Models;
using ExceptionService.Requests;

namespace ExceptionService.Interfaces
{
    public interface IWorkflowMonitorServiceClient
    {
        Task<StandardSoapResponse> ReprocessEnrouteExceptionsAsync(WorkflowExceptionRequest reprocessRequest, string adUserName);
        Task<StandardSoapResponse> ReprocessOnSiteExceptionsAsync(WorkflowExceptionRequest reprocessRequest, string adUserName);
        Task<StandardSoapResponse> ReprocessClearAppointmentExceptionsAsync(WorkflowExceptionRequest reprocessRequest, string adUserName);
        CommonExceptionType MapServiceExceptionTypeToCommonExceptionType(object serviceExceptionType);
    }
}
