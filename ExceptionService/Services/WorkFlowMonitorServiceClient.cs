using ExceptionService.Interfaces;
using ExceptionService.Requests;
using WorkFlowMonitorServiceReference;

namespace ExceptionService.Services
{
    public class WorkFlowMonitorServiceClient : IWorkflowMonitorServiceClient
    {
        private readonly WorkflowMonitorClient _client;

        public WorkFlowMonitorServiceClient()
        {
            _client = new WorkflowMonitorClient(WorkflowMonitorClient.EndpointConfiguration.BasicHttpsBinding_IWorkflowMonitor);
        }

        public async Task<StandardSoapResponseOfboolean> ReprocessEnrouteExceptionsAsync(WorkflowExceptionRequest reprocessRequest, string adUserName)
        {
            var request = new WorkflowExceptionModelOfSetEmployeeToEnRouteRequestbwABAbVO
            {
                Id = reprocessRequest.Id,
                CreateDate = reprocessRequest.CreateDate,
                ErrorInformation = reprocessRequest.ErrorInformation,
                IsBusinessError = reprocessRequest.IsBusinessError,
                JobNumber = reprocessRequest.JobNumber,
                JobSequenceNumber = reprocessRequest.JobSequenceNumber,
                Type = reprocessRequest.Type
            };

            var response = await _client.ReprocessEnrouteExceptionsAsync(request, adUserName);
            return response;
        }

        public async Task<StandardSoapResponseOfboolean> ReprocessOnSiteExceptionsAsync(WorkflowExceptionRequest reprocessRequest, string adUserName)
        {
            var request = new WorkflowExceptionModelOfSetEmployeeToOnSiteRequestbwABAbVO
            {
                Id = reprocessRequest.Id,
                CreateDate = reprocessRequest.CreateDate,
                ErrorInformation = reprocessRequest.ErrorInformation,
                IsBusinessError = reprocessRequest.IsBusinessError,
                JobNumber = reprocessRequest.JobNumber,
                JobSequenceNumber = reprocessRequest.JobSequenceNumber,
                Type = reprocessRequest.Type
            };

            var response = await _client.ReprocessOnSiteExceptionsAsync(request, adUserName);
            return response;
        }

        public async Task<StandardSoapResponseOfboolean> ReprocessClearAppointmentExceptionsAsync(WorkflowExceptionRequest reprocessRequest, string adUserName)
        {
            var request = new WorkflowExceptionModelOfClearAppointmentRequestModelT2o2hOfe
            {
                Id = reprocessRequest.Id,
                CreateDate = reprocessRequest.CreateDate,
                ErrorInformation = reprocessRequest.ErrorInformation,
                IsBusinessError = reprocessRequest.IsBusinessError,
                JobNumber = reprocessRequest.JobNumber,
                JobSequenceNumber = reprocessRequest.JobSequenceNumber,
                Type = reprocessRequest.Type,
                Payload = new ClearAppointmentRequestModel
                {
                    adUserName = adUserName
                }
            };

            var response = await _client.ReprocessClearAppointmentExceptionsAsync(request, adUserName);
            return response;
        }
    }
}