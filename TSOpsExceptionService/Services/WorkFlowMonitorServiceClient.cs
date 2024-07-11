using TSOpsExceptionService.Configuration.Models;
using TSOpsExceptionService.Interfaces;
using TSOpsExceptionService.Requests;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using WorkFlowMonitorServiceReference;

namespace TSOpsExceptionService.Services
{
    public class WorkFlowMonitorServiceClient : IWorkflowMonitorServiceClient
    {
        private readonly ILogger<WorkFlowMonitorServiceClient> _logger;
        private readonly IWorkflowMonitorSoapClient _client;

        public WorkFlowMonitorServiceClient(IOptions<SoapEndpointOptions> endpointOptions, ILogger<WorkFlowMonitorServiceClient> logger, IWorkflowMonitorSoapClient client = null)
        {
            _logger = logger;
            _client = client ?? new WorkflowMonitorSoapClientWrapper(new WorkflowMonitorClient(WorkflowMonitorClient.EndpointConfiguration.BasicHttpsBinding_IWorkflowMonitor, endpointOptions.Value.WorkflowMonitorServiceBaseUrl));
            _logger.LogInformation("WorkflowMonitorService Endpoint - {endpoint}", endpointOptions.Value.WorkflowMonitorServiceBaseUrl);
        }

        public async Task<StandardSoapResponseOfboolean> ReprocessEnrouteExceptionsAsync(WorkflowExceptionRequest reprocessRequest, string adUserName)
        {
            StandardSoapResponseOfboolean response = new StandardSoapResponseOfboolean();
            WorkflowExceptionModelOfSetEmployeeToEnRouteRequestbwABAbVO request = new WorkflowExceptionModelOfSetEmployeeToEnRouteRequestbwABAbVO();
            try
            {
                Stopwatch stopwatch = new Stopwatch();

                request = new WorkflowExceptionModelOfSetEmployeeToEnRouteRequestbwABAbVO
                {
                    Id = reprocessRequest.Id,
                    CreateDate = reprocessRequest.CreateDate,
                    ErrorInformation = reprocessRequest.ErrorInformation,
                    IsBusinessError = reprocessRequest.IsBusinessError,
                    JobNumber = reprocessRequest.JobNumber,
                    JobSequenceNumber = reprocessRequest.JobSequenceNumber,
                    Type = reprocessRequest.Type,
                    Payload = new SetEmployeeToEnRouteRequest
                    {
                        adUserName = adUserName,
                        jobEmp_SeqNo = reprocessRequest.JobSequenceNumber ?? 0,
                        job_No = reprocessRequest.JobNumber ?? 0,
                        utcStatusDateTime = reprocessRequest.CreateDate
                    }
                };

                _logger.LogInformation($"Request for reprocessing Enroute Exceptions\n " +
                                    "Id - {id} \n" +
                                    "CreateDate - {date} \n " +
                                    "ErrorInformation - {errinfo} \n" +
                                    "IsBusinessError - {businesserror} \n" +
                                    "JobNumber - {jobnumber} \n" +
                                    "JobSequenceNumber - {jobseqnumber} \n" +
                                    "Type - {type} \n" +
                                    "PayLoad = {payload}", request.Id, request.CreateDate, request.ErrorInformation,
                                    request.IsBusinessError, request.JobNumber, request.JobSequenceNumber, request.Type.ToString(),
                                    adUserName + " " + reprocessRequest.JobSequenceNumber + " " + reprocessRequest.JobNumber + " " + reprocessRequest.CreateDate.ToString());

                stopwatch.Start();
                response = await _client.ReprocessEnrouteExceptionsAsync(request, adUserName);
                stopwatch.Stop();
                TimeSpan elapsedTime = stopwatch.Elapsed;
                _logger.LogInformation("Time taken to ReprocessEnrouteException for jobnumber {jobno} is: {ElapsedMilliseconds} ms", request.JobNumber, elapsedTime.TotalMilliseconds);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error in ReprocessEnrouteExceptionsAsync() method in Services occurred while trying to reprocess enroute exception for Id {id} with jobnumber {jobno}", request.Id, request.JobNumber);
                _logger.LogError("Detailed Error - " + ex);
                return null;
            }

            return response;
        }

        public async Task<StandardSoapResponseOfboolean> ReprocessOnSiteExceptionsAsync(WorkflowExceptionRequest reprocessRequest, string adUserName)
        {
            StandardSoapResponseOfboolean response = new StandardSoapResponseOfboolean();
            WorkflowExceptionModelOfSetEmployeeToOnSiteRequestbwABAbVO request = new WorkflowExceptionModelOfSetEmployeeToOnSiteRequestbwABAbVO();

            try
            {
                Stopwatch stopwatch = new Stopwatch();

                request = new WorkflowExceptionModelOfSetEmployeeToOnSiteRequestbwABAbVO
                {
                    Id = reprocessRequest.Id,
                    CreateDate = reprocessRequest.CreateDate,
                    ErrorInformation = reprocessRequest.ErrorInformation,
                    IsBusinessError = reprocessRequest.IsBusinessError,
                    JobNumber = reprocessRequest.JobNumber,
                    JobSequenceNumber = reprocessRequest.JobSequenceNumber,
                    Type = reprocessRequest.Type,
                    Payload = new SetEmployeeToOnSiteRequest
                    {
                        adUserName = adUserName,
                        jobEmp_SeqNo = reprocessRequest.JobSequenceNumber ?? 0,
                        job_No = reprocessRequest.JobNumber ?? 0,
                        utcStatusDateTime = reprocessRequest.CreateDate
                    }
                };

                _logger.LogInformation($"Request for reprocessing OnSite Exceptions\n " +
                                    "Id - {id}\n" +
                                    "CreateDate - {date}\n" +
                                    "ErrorInformation - {errinfo}\n" +
                                    "IsBusinessError - {businesserror}\n" +
                                    "JobNumber - {jobnumber}\n" +
                                    "JobSequenceNumber - {jobseqnumber}\n" +
                                    "Type - {type} \n" +
                                    "PayLoad = {payload}", request.Id, request.CreateDate, request.ErrorInformation,
                                    request.IsBusinessError, request.JobNumber, request.JobSequenceNumber, request.Type.ToString(),
                                    adUserName + " " + reprocessRequest.JobSequenceNumber + " " + reprocessRequest.JobNumber + " " + reprocessRequest.CreateDate.ToString());

                stopwatch.Start();
                response = await _client.ReprocessOnSiteExceptionsAsync(request, adUserName);
                stopwatch.Stop();
                TimeSpan elapsedTime = stopwatch.Elapsed;
                _logger.LogInformation("Time taken to ReprocessOnSiteException for jobnumber {jobno} is: {ElapsedMilliseconds} ms", request.JobNumber, elapsedTime.TotalMilliseconds);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error in ReprocessOnSiteExceptionsAsync() method in Services occurred while trying to reprocess onsite exception for Id {id} with jobnumber {jobno}", request.Id, request.JobNumber);
                _logger.LogError("Detailed Error - " + ex);
                return null;
            }
            return response;
        }

        public async Task<StandardSoapResponseOfboolean> ReprocessClearAppointmentExceptionsAsync(WorkflowExceptionRequest reprocessRequest, string adUserName)
        {
            StandardSoapResponseOfboolean response = new StandardSoapResponseOfboolean();
            WorkflowExceptionModelOfClearAppointmentRequestModelT2o2hOfe request = new WorkflowExceptionModelOfClearAppointmentRequestModelT2o2hOfe();

            try
            {
                Stopwatch stopwatch = new Stopwatch();

                request = new WorkflowExceptionModelOfClearAppointmentRequestModelT2o2hOfe
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
                _logger.LogInformation("Time taken to ReprocessClearAppointmentException for jobnumber {jobno} is: {ElapsedMilliseconds} ms", request.JobNumber, elapsedTime.TotalMilliseconds);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error in ReprocessClearAppointmentExceptionsAsync() method in Services occurred while trying to reprocess clear exception for Id {id} with jobnumber {jobno}", request.Id, request.JobNumber);
                _logger.LogError("Detailed Error - " + ex);
                return null;
            }

            return response;
        }
    }
}