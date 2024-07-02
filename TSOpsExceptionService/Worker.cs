using TSOpsExceptionService.Interfaces;
using TSOpsExceptionService.Common;
using TSOpsExceptionService.Requests;
using WorkFlowMonitorServiceReference;
using TSOpsExceptionService.Configuration.Models;
using Microsoft.Extensions.Options;

public class Worker : BackgroundService
{
    private readonly IOptions<DurationOptions> _durationOptions;
    private readonly ILogger<Worker> _logger;
    private readonly IWorkFlowExceptionService _exceptionService;
    private readonly IJobServiceClient _jobServiceClient;
    private readonly IWorkflowMonitorServiceClient _workflowMonitorServiceClient;
    private readonly IDeserialization _derialization;

    public Worker(ILogger<Worker> logger, IWorkFlowExceptionService exceptionService, IJobServiceClient jobServiceClient,
        IWorkflowMonitorServiceClient workflowMonitorServiceClient, IOptions<DurationOptions> durationOptions, IDeserialization deserialization)
    {
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
            try
            {
                var exceptions = _exceptionService.GetWorkflowExceptions();

                if (exceptions.Count > 0)
                {
                    foreach (var exception in exceptions.OrderByDescending(i => i.CreateDate))
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
                            _logger.LogError("Job retrieval was unsuccessfull for job id - {id} and job number - {jobnumber}", exception.Id, exception.JobNumber);
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
                _logger.LogError("Detailed Error - " + ex.Message);
            }

            await Task.Delay(60000 * _durationOptions.Value.TimeIntervalInMinutes, stoppingToken);
        }
    }

    private async Task ReprocessEnrouteExceptionsAsync(WorkflowExceptionRequest reprocessRequest, string xmlData)
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(xmlData) && _derialization.TryDeserializeEnrouteFromXml(xmlData, out SetEmployeeToEnRouteRequest deserializedRequest))
            {
                _logger.LogInformation("Deserialization in ReprocessEnrouteExceptionsAsync for Id - {id} is successfull", reprocessRequest.Id);
                var response = await _workflowMonitorServiceClient.ReprocessEnrouteExceptionsAsync(reprocessRequest, deserializedRequest.adUserName);

                if (response != null && response.ReturnValue)
                {
                    // Success in reprocessing
                    _logger.LogInformation("ReprocessEnrouteExceptionsAsync is successfull for Id - {id}", reprocessRequest.Id);
                }
                else
                {
                    // Fail to reprocess
                    _logger.LogInformation("ReprocessEnrouteExceptionsAsync is unsuccessfull. Record already reprocessed for Id - {id}", reprocessRequest.Id);
                }
            }
            else
            {
                _logger.LogError("An error occurred while deserializing in ReprocessEnrouteExceptionsAsync() method with Id - {Id}", reprocessRequest.Id);
                _logger.LogError("Faulted Xml - {xml}", xmlData);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError("An error occurred while processing exceptions in ReprocessEnrouteExceptionsAsync() method.");
            _logger.LogError("Detailed Error - " + ex.Message);
        }
    }

    private async Task ReprocessOnSiteExceptionsAsync(WorkflowExceptionRequest reprocessRequest, string xmlData)
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(xmlData) && _derialization.TryDeserializeOnSiteFromXml(xmlData, out SetEmployeeToOnSiteRequest deserializedRequest))
            {
                _logger.LogInformation("Deserialization in ReprocessOnSiteExceptionsAsync for Id - {id} is successfull", reprocessRequest.Id);

                var response = await _workflowMonitorServiceClient.ReprocessOnSiteExceptionsAsync(reprocessRequest, deserializedRequest.adUserName);

                if (response != null && response.ReturnValue)
                {
                    // Success in reprocessing
                    _logger.LogInformation("ReprocessOnSiteExceptionsAsync is successfull for Id - {id}", reprocessRequest.Id);
                }
                else
                {
                    // Fail to reprocess
                    _logger.LogInformation("ReprocessOnSiteExceptionsAsync is unsuccessfull. Record already reprocessed for Id - {id}", reprocessRequest.Id);
                }
            }
            else
            {
                _logger.LogError("An error occurred while deserializing in ReprocessOnSiteExceptionsAsync() method with Id - {Id}", reprocessRequest.Id);
                _logger.LogError("Faulted Xml - {xml}", xmlData);
            }
        }
        catch(Exception ex)
        {
            _logger.LogError("An error occurred while processing exceptions in ReprocessOnSiteExceptionsAsync() method.");
            _logger.LogError("Detailed Error - " + ex.Message);
        }
    }

    private async Task ReprocessOnClearAppointmentsExceptionsAsync(WorkflowExceptionRequest reprocessRequest, string xmlData)
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(xmlData) && _derialization.TryDeserializeClearFromXml(xmlData, out ClearAppointmentRequestModel deserializedRequest))
            {
                _logger.LogInformation("Deserialization in ReprocessOnClearAppointmentsExceptionsAsync for Id - {id} is successfull", reprocessRequest.Id);

                var response = await _workflowMonitorServiceClient.ReprocessClearAppointmentExceptionsAsync(reprocessRequest, deserializedRequest.adUserName);

                if (response != null && response.ReturnValue)
                {
                    // Success in reprocessing
                    _logger.LogInformation("ReprocessOnClearAppointmentsExceptionsAsync is successfull for Id - {id}", reprocessRequest.Id);
                }
                else
                {
                    // Fail to reprocess
                    _logger.LogInformation("ReprocessOnClearAppointmentsExceptionsAsync is unsuccessfull. Record already reprocessed for Id - {id}", reprocessRequest.Id);
                }
            }
            else
            {
                _logger.LogError("An error occurred while deserializing in ReprocessOnClearAppointmentsExceptionsAsync() method with Id - {Id}", reprocessRequest.Id);
                _logger.LogError("Faulted Xml - {xml}", xmlData);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError("An error occurred while processing exceptions in ReprocessOnClearAppointmentsExceptionsAsync() method.");
            _logger.LogError("Detailed Error - " + ex.Message);
        }
    } 
}