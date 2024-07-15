using TSOpsExceptionService.Interfaces;
using TSOpsExceptionService.Common;
using TSOpsExceptionService.Requests;
using WorkFlowMonitorServiceReference;
using TSOpsExceptionService.Configuration.Models;
using Microsoft.Extensions.Options;

public class Worker : BackgroundService
{
    private readonly IOptions<Retry> _retry;
    private readonly IOptions<DurationOptions> _durationOptions;
    private readonly ILogger<Worker> _logger;
    private readonly IWorkFlowExceptionService _exceptionService;
    private readonly IJobServiceClient _jobServiceClient;
    private readonly IWorkflowMonitorServiceClient _workflowMonitorServiceClient;
    private readonly IDeserialization _derialization;

    public Worker(ILogger<Worker> logger, IWorkFlowExceptionService exceptionService, IJobServiceClient jobServiceClient,
        IWorkflowMonitorServiceClient workflowMonitorServiceClient, IOptions<DurationOptions> durationOptions, IDeserialization deserialization, IOptions<Retry> retry)
    {
        _retry = retry;
        _durationOptions = durationOptions;
        _logger = logger;
        _exceptionService = exceptionService;
        _jobServiceClient = jobServiceClient;
        _workflowMonitorServiceClient = workflowMonitorServiceClient;
        _derialization = deserialization;
    }

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Service Started at {datetime}", DateTime.Now.ToString("yyyy-MM-dd:HH:mm"));
        return base.StartAsync(cancellationToken);
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Service Stopped at {datetime}", DateTime.Now.ToString("yyyy-MM-dd:HH:mm"));
        return base.StopAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await ProcessWorkflowExceptions();
            await Task.Delay(60000 * _durationOptions.Value.TimeIntervalInMinutes, stoppingToken);
        }
    }

    public async Task ProcessWorkflowExceptions()
    {
        try
        {
            var exceptions = _exceptionService.GetWorkflowExceptions();

            if (exceptions.Count > 0)
            {
                foreach (var exception in exceptions)
                {
                    var job = await _jobServiceClient.GetJobAsync(exception.JobNumber.GetValueOrDefault());

                    if (job != null && job.JOBTYPE_ID == Constants.INSTALL)
                    {
                        _logger.LogInformation("Successfully retrieved job for job number - {id}", exception.JobNumber);

                        var request = new WorkflowExceptionRequest
                        {
                            Id = exception.Id,
                            CreateDate = exception.CreateDate ?? DateTime.MinValue,
                            ErrorInformation = exception.ErrorInfo ?? string.Empty,
                            IsBusinessError = exception.IsBusinessError ?? false,
                            JobNumber = exception.JobNumber,
                            JobSequenceNumber = exception.JobSeqNumber,
                            Type = Helper.MapServiceExceptionTypeToExceptionType(exception.Type)
                        };

                        if (request.Type == ExceptionType.Enroute)
                        {
                            await ReprocessEnrouteExceptionsAsync(request, exception.Data);
                        }
                        else if (request.Type == ExceptionType.Clear)
                        {
                            await ReprocessOnClearAppointmentsExceptionsAsync(request, exception.Data);
                        }
                        else if (request.Type == ExceptionType.OnSite)
                        {
                            await ReprocessOnSiteExceptionsAsync(request, exception.Data);
                        }
                    }
                    else if (job == null)
                    {
                        _logger.LogError("Job retrieval was unsuccessfull for Id - {id} and job number - {jobnumber}", exception.Id, exception.JobNumber);
                    }
                }
            }
            else
            {
                _logger.LogInformation("No Exceptions");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError("An error occurred while processing exceptions in ExecuteAsync() method.");
            _logger.LogError("Detailed Error - " + ex);
        }
    }

    internal async Task ReprocessEnrouteExceptionsAsync(WorkflowExceptionRequest reprocessRequest, string xmlData)
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(xmlData) && _derialization.TryDeserializeEnrouteFromXml(xmlData, out SetEmployeeToEnRouteRequest deserializedRequest))
            {
                _logger.LogInformation("Deserialization in ReprocessEnrouteExceptionsAsync for Id - {id} with jobnumber {jobno} and seqno {seqno} is successfull",
                    reprocessRequest.Id, reprocessRequest.JobNumber, reprocessRequest.JobSequenceNumber);

                var response = await _workflowMonitorServiceClient.ReprocessEnrouteExceptionsAsync(reprocessRequest, deserializedRequest.adUserName);

                if (response == null)
                {
                    await RetryReprocessEnrouteExceptionsAsync(reprocessRequest, deserializedRequest.adUserName);
                }

                else
                {
                    if (response != null && response.ReturnValue)
                    {
                        // Success in reprocessing
                        _exceptionService.SaveReprocessedRecord(reprocessRequest, true);
                        _logger.LogInformation("ReprocessEnrouteExceptionsAsync is successfull for Id - {id} with jobnumber {jobno} and seqno {seqno}",
                            reprocessRequest.Id, reprocessRequest.JobNumber, reprocessRequest.JobSequenceNumber);
                    }
                    else
                    {
                        // Fail to reprocess but response was 200
                        _exceptionService.SaveReprocessedRecord(reprocessRequest, true);
                        _logger.LogInformation("ReprocessEnrouteExceptionsAsync is unsuccessfull. Record already reprocessed for Id - {id} with jobnumber {jobno} and seqno {seqno}",
                            reprocessRequest.Id, reprocessRequest.JobNumber, reprocessRequest.JobSequenceNumber);
                    }
                }
            }
            else
            {
                _logger.LogError("An error occurred while deserializing in ReprocessEnrouteExceptionsAsync() method for Id - {Id} with jobnumber {jobno} and seqno {seqno}",
                    reprocessRequest.Id, reprocessRequest.JobNumber, reprocessRequest.JobSequenceNumber);
                _logger.LogError("Faulted Xml - {xml}", xmlData);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError("An error occurred while processing exceptions in ReprocessEnrouteExceptionsAsync() method for Id - {id} with jobnumber {jobno} and seqno {seqno}",
                reprocessRequest.Id, reprocessRequest.JobNumber, reprocessRequest.JobSequenceNumber);
            _logger.LogError("Detailed Error - " + ex);
        }
    }

    internal async Task ReprocessOnSiteExceptionsAsync(WorkflowExceptionRequest reprocessRequest, string xmlData)
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(xmlData) && _derialization.TryDeserializeOnSiteFromXml(xmlData, out SetEmployeeToOnSiteRequest deserializedRequest))
            {
                _logger.LogInformation("Deserialization in ReprocessOnSiteExceptionsAsync is successfull for Id - {id} with jobnumber {jobno} and seqno {seqno}",
                    reprocessRequest.Id, reprocessRequest.JobNumber, reprocessRequest.JobSequenceNumber);

                var response = await _workflowMonitorServiceClient.ReprocessOnSiteExceptionsAsync(reprocessRequest, deserializedRequest.adUserName);

                if (response == null)
                {
                    await RetryReprocessOnSiteExceptionsAsync(reprocessRequest, deserializedRequest.adUserName);
                }

                else
                {
                    if (response != null && response.ReturnValue)
                    {
                        // Success in reprocessing
                        _exceptionService.SaveReprocessedRecord(reprocessRequest, true);
                        _logger.LogInformation("ReprocessOnSiteExceptionsAsync is successfull for Id - {id} with jobnumber {jobno} and seqno {seqno}",
                            reprocessRequest.Id, reprocessRequest.JobNumber, reprocessRequest.JobSequenceNumber);
                    }
                    else
                    {
                        // Fail to reprocess but response was 200
                        _exceptionService.SaveReprocessedRecord(reprocessRequest, true);
                        _logger.LogInformation("ReprocessOnSiteExceptionsAsync is unsuccessfull. Record already reprocessed for Id - {id} with jobnumber {jobno} and seqno {seqno}",
                            reprocessRequest.Id, reprocessRequest.JobNumber, reprocessRequest.JobSequenceNumber);
                    }
                }
            }
            else
            {
                _logger.LogError("An error occurred while deserializing in ReprocessOnSiteExceptionsAsync() method for Id - {Id} with jobnumber {jobno} and seqno {seqno}",
                    reprocessRequest.Id, reprocessRequest.JobNumber, reprocessRequest.JobSequenceNumber);
                _logger.LogError("Faulted Xml - {xml}", xmlData);
            }
        }
        catch(Exception ex)
        {
            _logger.LogError("An error occurred while processing exceptions in ReprocessOnSiteExceptionsAsync() method.");
            _logger.LogError("Detailed Error - " + ex);
        }
    }

    internal async Task ReprocessOnClearAppointmentsExceptionsAsync(WorkflowExceptionRequest reprocessRequest, string xmlData)
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(xmlData) && _derialization.TryDeserializeClearFromXml(xmlData, out ClearAppointmentRequestModel deserializedRequest))
            {
                _logger.LogInformation("Deserialization is successfull in ReprocessOnClearAppointmentsExceptionsAsync for Id - {id} with jobnumber {jobno} and seqno {seqno}",
                    reprocessRequest.Id, reprocessRequest.JobNumber, reprocessRequest.JobSequenceNumber);

                var response = await _workflowMonitorServiceClient.ReprocessClearAppointmentExceptionsAsync(reprocessRequest, deserializedRequest.adUserName);

                if (response == null)
                {
                    await RetryReprocessClearAppointmentExceptionsAsync(reprocessRequest, deserializedRequest.adUserName);
                }

                else
                {
                    if (response != null && response.ReturnValue)
                    {
                        // Success in reprocessing
                        _exceptionService.SaveReprocessedRecord(reprocessRequest, true);
                        _logger.LogInformation("ReprocessOnClearAppointmentsExceptionsAsync is successfull for Id - {id} with jobnumber {jobno} and seqno {seqno}",
                            reprocessRequest.Id, reprocessRequest.JobNumber, reprocessRequest.JobSequenceNumber);
                    }
                    else
                    {
                        // Fail to reprocess but response was 200
                        _exceptionService.SaveReprocessedRecord(reprocessRequest, true);
                        _logger.LogInformation("ReprocessOnClearAppointmentsExceptionsAsync is unsuccessfull. Record already reprocessed for Id - {id} with jobnumber {jobno} and seqno {seqno}",
                            reprocessRequest.Id, reprocessRequest.JobNumber, reprocessRequest.JobSequenceNumber);
                    }
                }
            }
            else
            {
                _logger.LogError("An error occurred while deserializing in ReprocessOnClearAppointmentsExceptionsAsync() method for Id - {Id} with jobnumber {jobno} and seqno {seqno}",
                    reprocessRequest.Id, reprocessRequest.JobNumber, reprocessRequest.JobSequenceNumber);
                _logger.LogError("Faulted Xml - {xml}", xmlData);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError("An error occurred while processing exceptions in ReprocessOnClearAppointmentsExceptionsAsync() method.");
            _logger.LogError("Detailed Error - " + ex);
        }
    }

    private async Task RetryReprocessEnrouteExceptionsAsync(WorkflowExceptionRequest reprocessRequest, string adUserName)
    {
        bool isReprocessed = false;
        int retry = _retry.Value.Attempts + 1;

        for (int i = 1; i < retry; i++)
        {
            _logger.LogInformation("Retrying RetryReprocessEnrouteExceptionsAsync for Id - {id} with jobnumber {jobnumber} and seqno {seqno}",
                reprocessRequest.Id, reprocessRequest.JobNumber, reprocessRequest.JobSequenceNumber);

            var response = await _workflowMonitorServiceClient.ReprocessEnrouteExceptionsAsync(reprocessRequest, adUserName);

            if (response != null)
            {
                _logger.LogInformation("ReprocessEnrouteExceptionsAsync is successfull for Id - {id} with jobnumber {jobno} and seqno {seqno} in {count} attempts",
                    reprocessRequest.Id, reprocessRequest.JobNumber, reprocessRequest.JobSequenceNumber, i + 1);
                isReprocessed = true;
                break;
            }
        }
        _exceptionService.SaveReprocessedRecord(reprocessRequest, isReprocessed);

        if (!isReprocessed)
            _logger.LogError("ReprocessEnrouteExceptionsAsync is unsuccessfull for Id - {id} with jobnumber {jobno} and seqno {seqno}. Response was null",
                reprocessRequest.Id, reprocessRequest.JobNumber, reprocessRequest.JobSequenceNumber);
    }

    private async Task RetryReprocessOnSiteExceptionsAsync(WorkflowExceptionRequest reprocessRequest, string adUserName)
    {
        bool isReprocessed = false;
        int retry = _retry.Value.Attempts + 1;

        for (int i = 1; i < retry; i++)
        {
            _logger.LogInformation("Retrying ReprocessOnSiteExceptionAsync for Id - {id} with jobnumber {jobnumber} and seqno {seqno}",
                reprocessRequest.Id, reprocessRequest.JobNumber, reprocessRequest.JobSequenceNumber);

            var response = await _workflowMonitorServiceClient.ReprocessOnSiteExceptionsAsync(reprocessRequest, adUserName);

            if (response != null)
            {
                _logger.LogInformation("ReprocessOnSiteExceptionsAsync is successfull for Id - {id} with jobnumber {jobno} and seqno {seqno} in {count} attempts",
                    reprocessRequest.Id, reprocessRequest.JobSequenceNumber, reprocessRequest.JobNumber, i + 1);
                isReprocessed = true;
                break;
            }
        }
        _exceptionService.SaveReprocessedRecord(reprocessRequest, isReprocessed);

        if (!isReprocessed)
            _logger.LogError("ReprocessOnSiteExceptionsAsync is unsuccessfull for Id - {id} with jobnumber {jobno} and seqno {seqno}. Response was null",
                reprocessRequest.Id, reprocessRequest.JobNumber, reprocessRequest.JobSequenceNumber);
    }

    private async Task RetryReprocessClearAppointmentExceptionsAsync(WorkflowExceptionRequest reprocessRequest, string adUserName)
    {
        bool isReprocessed = false;
        int retry = _retry.Value.Attempts + 1;

        for (int i = 1; i < retry; i++)
        {
            _logger.LogInformation("Retrying ReprocessOnClearAppointmentsExceptionsAsync for Id - {id} with jobnumber {jobnumber} and seqno {seqno}",
                reprocessRequest.Id, reprocessRequest.JobNumber, reprocessRequest.JobSequenceNumber);

            var response = await _workflowMonitorServiceClient.ReprocessClearAppointmentExceptionsAsync(reprocessRequest, adUserName);

            if (response != null)
            {
                _logger.LogInformation("ReprocessOnClearAppointmentsExceptionsAsync is successfull for Id - {id} with jobnumber {jobno} and seqno {seqno} in {count} attempts",
                    reprocessRequest.Id, reprocessRequest.JobSequenceNumber, reprocessRequest.JobNumber, i + 1);
                isReprocessed = true;
                break;
            }
        }
        _exceptionService.SaveReprocessedRecord(reprocessRequest, isReprocessed);

        if (!isReprocessed)
            _logger.LogError("ReprocessOnClearAppointmentsExceptionsAsync is unsuccessfull for Id - {id} with jobnumber {jobno} and seqno {seqno}. Response was null",
                reprocessRequest.Id, reprocessRequest.JobNumber, reprocessRequest.JobSequenceNumber);
    }
}
