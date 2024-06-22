using ExceptionService.Enums;
using ExceptionService.Interfaces;
using ExceptionService.Models;
using ExceptionService.Requests;
using WorkFlowMonitorServiceReference;

namespace ExceptionService.Services
{
    public class ProductionWorkFlowMonitorServiceClient : IWorkflowMonitorServiceClient
    {
        private readonly WorkflowMonitorClient _client;

        public ProductionWorkFlowMonitorServiceClient()
        {
            _client = new WorkflowMonitorClient(WorkflowMonitorClient.EndpointConfiguration.BasicHttpsBinding_IWorkflowMonitor);
        }

        public async Task<StandardSoapResponse> ReprocessEnrouteExceptionsAsync(WorkflowExceptionRequest reprocessRequest, string adUserName)
        {
            var request = new WorkflowExceptionModelOfSetEmployeeToEnRouteRequestbwABAbVO
            {
                Id = reprocessRequest.Id,
                CreateDate = reprocessRequest.CreateDate,
                ErrorInformation = reprocessRequest.ErrorInformation,
                IsBusinessError = reprocessRequest.IsBusinessError,
                JobNumber = reprocessRequest.JobNumber,
                JobSequenceNumber = reprocessRequest.JobSequenceNumber,
                Type = MapCommonExceptionTypeToServiceExceptionType(reprocessRequest.Type)
            };

            var response = await _client.ReprocessEnrouteExceptionsAsync(request, adUserName);
            return new StandardSoapResponse { ReturnValue = response.ReturnValue };
        }

        public async Task<StandardSoapResponse> ReprocessOnSiteExceptionsAsync(WorkflowExceptionRequest reprocessRequest, string adUserName)
        {
            var request = new WorkflowExceptionModelOfSetEmployeeToOnSiteRequestbwABAbVO
            {
                Id = reprocessRequest.Id,
                CreateDate = reprocessRequest.CreateDate,
                ErrorInformation = reprocessRequest.ErrorInformation,
                IsBusinessError = reprocessRequest.IsBusinessError,
                JobNumber = reprocessRequest.JobNumber,
                JobSequenceNumber = reprocessRequest.JobSequenceNumber,
                Type = MapCommonExceptionTypeToServiceExceptionType(reprocessRequest.Type)
            };

            var response = await _client.ReprocessOnSiteExceptionsAsync(request, adUserName);
            return new StandardSoapResponse { ReturnValue = response.ReturnValue };
        }

        public async Task<StandardSoapResponse> ReprocessClearAppointmentExceptionsAsync(WorkflowExceptionRequest reprocessRequest, string adUserName)
        {
            var request = new WorkflowExceptionModelOfClearAppointmentRequestModelT2o2hOfe
            {
                Id = reprocessRequest.Id,
                CreateDate = reprocessRequest.CreateDate,
                ErrorInformation = reprocessRequest.ErrorInformation,
                IsBusinessError = reprocessRequest.IsBusinessError,
                JobNumber = reprocessRequest.JobNumber,
                JobSequenceNumber = reprocessRequest.JobSequenceNumber,
                Type = MapCommonExceptionTypeToServiceExceptionType(reprocessRequest.Type),
                Payload = new ClearAppointmentRequestModel
                {
                    adUserName = adUserName
                }
            };

            var response = await _client.ReprocessClearAppointmentExceptionsAsync(request, adUserName);
            return new StandardSoapResponse { ReturnValue = response.ReturnValue };
        }
        private ExceptionType MapCommonExceptionTypeToServiceExceptionType(CommonExceptionType type)
        {
            return type switch
            {
                CommonExceptionType.Enroute => ExceptionType.Enroute,
                CommonExceptionType.OnSite => ExceptionType.OnSite,
                CommonExceptionType.Clear => ExceptionType.Clear,
                _ => throw new ArgumentOutOfRangeException(nameof(type), $"Not expected exception type value: {type}")
            };
        }

        public CommonExceptionType MapServiceExceptionTypeToCommonExceptionType(object serviceExceptionType)
        {
            return serviceExceptionType switch
            {
                ExceptionType.Enroute => CommonExceptionType.Enroute,
                ExceptionType.OnSite => CommonExceptionType.OnSite,
                ExceptionType.Clear => CommonExceptionType.Clear,
                _ => throw new ArgumentOutOfRangeException(nameof(serviceExceptionType), $"Not expected exception type value: {serviceExceptionType}")
            };
        }
    }
}