using ExceptionService.Configuration.Models;
using ExceptionService.Interfaces;
using ExceptionService.Requests;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using WorkFlowMonitorServiceReference;

namespace ExceptionService.Services
{
    public class WorkFlowMonitorServiceClient : IWorkflowMonitorServiceClient
    {
        private readonly ILogger<WorkFlowMonitorServiceClient> _logger;
        private readonly WorkflowMonitorClient _client;

        public WorkFlowMonitorServiceClient(IOptions<SoapEndpointOptions> endpointOptions, ILogger<WorkFlowMonitorServiceClient> logger)
        {
            _logger = logger;
            _client = new WorkflowMonitorClient(WorkflowMonitorClient.EndpointConfiguration.BasicHttpsBinding_IWorkflowMonitor, endpointOptions.Value.WorkflowMonitorServiceBaseUrl);
            _logger.LogInformation("WorkflowMonitorService Endpoint - {endpoint}", endpointOptions.Value.WorkflowMonitorServiceBaseUrl);
        }

        public async Task<StandardSoapResponseOfboolean> ReprocessEnrouteExceptionsAsync(WorkflowExceptionRequest reprocessRequest, string adUserName)
        {
            StandardSoapResponseOfboolean response = new StandardSoapResponseOfboolean();
            try
            {
                Stopwatch stopwatch = new Stopwatch();

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

                _logger.LogInformation($"Request for reprocessing Enroute Exceptions\n " +
                                    "Id - {id}\n" +
                                    "CreateDate - {date}\n" +
                                    "ErrorInformation - {errinfo}\n" +
                                    "IsBusinessError - {businesserror}\n" +
                                    "JobNumber - {jobnumber}\n" +
                                    "JobSequenceNumber - {jobseqnumber}\n" +
                                    "Type - {type}", request.Id, request.CreateDate, request.ErrorInformation,
                                    request.IsBusinessError, request.JobNumber, request.JobSequenceNumber, request.Type.ToString());

                stopwatch.Start();
                response = await _client.ReprocessEnrouteExceptionsAsync(request, adUserName);
                stopwatch.Stop();
                TimeSpan elapsedTime = stopwatch.Elapsed;
                _logger.LogInformation("Time taken to ReprocessEnrouteException is: {ElapsedMilliseconds} ms", elapsedTime.TotalMilliseconds);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error in ReprocessEnrouteExceptionsAsync() method in Services");
                _logger.LogError("Detailed Error - " + ex.Message);
                return null;
            }

            return response;
        }

        public async Task<StandardSoapResponseOfboolean> ReprocessOnSiteExceptionsAsync(WorkflowExceptionRequest reprocessRequest, string adUserName)
        {
            StandardSoapResponseOfboolean response = new StandardSoapResponseOfboolean();

            try
            {
                Stopwatch stopwatch = new Stopwatch();

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

                _logger.LogInformation($"Request for reprocessing OnSite Exceptions\n " +
                                    "Id - {id}\n" +
                                    "CreateDate - {date}\n" +
                                    "ErrorInformation - {errinfo}\n" +
                                    "IsBusinessError - {businesserror}\n" +
                                    "JobNumber - {jobnumber}\n" +
                                    "JobSequenceNumber - {jobseqnumber}\n" +
                                    "Type - {type}", request.Id, request.CreateDate, request.ErrorInformation,
                                    request.IsBusinessError, request.JobNumber, request.JobSequenceNumber, request.Type.ToString());

                stopwatch.Start();
                response = await _client.ReprocessOnSiteExceptionsAsync(request, adUserName);
                stopwatch.Stop();
                TimeSpan elapsedTime = stopwatch.Elapsed;
                _logger.LogInformation("Time taken to ReprocessOnSiteException is: {ElapsedMilliseconds} ms", elapsedTime.TotalMilliseconds);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error in ReprocessOnSiteExceptionsAsync() method in Services");
                _logger.LogError("Detailed Error - " + ex.Message);
                return null;
            }
            return response;
        }

        public async Task<StandardSoapResponseOfboolean> ReprocessClearAppointmentExceptionsAsync(WorkflowExceptionRequest reprocessRequest, string adUserName)
        {
            StandardSoapResponseOfboolean response = new StandardSoapResponseOfboolean();

            try
            {
                Stopwatch stopwatch = new Stopwatch();

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

                _logger.LogInformation($"Request for reprocessing Clear Exceptions\n " +
                                    "Id - {id}\n" +
                                    "CreateDate - {date}\n" +
                                    "ErrorInformation - {errinfo}\n" +
                                    "IsBusinessError - {businesserror}\n" +
                                    "JobNumber - {jobnumber}\n" +
                                    "JobSequenceNumber - {jobseqnumber}\n" +
                                    "Type - {type}\n" +
                                    "AdUserName - {username}", request.Id, request.CreateDate, request.ErrorInformation,
                                    request.IsBusinessError, request.JobNumber, request.JobSequenceNumber, request.Type.ToString(), request.Payload.adUserName);

                stopwatch.Start();
                response = await _client.ReprocessClearAppointmentExceptionsAsync(request, adUserName);
                stopwatch.Stop();
                TimeSpan elapsedTime = stopwatch.Elapsed;
                _logger.LogInformation("Time taken to ReprocessClearAppointmentException is: {ElapsedMilliseconds} ms", elapsedTime.TotalMilliseconds);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error in ReprocessClearAppointmentExceptionsAsync() method in Services");
                _logger.LogError("Detailed Error - " + ex.Message);
                return null;
            }

            return response;
        }
    }
}