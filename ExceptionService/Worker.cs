using ExceptionService.Interfaces;
using ExceptionService.Common;
using ExceptionService.Requests;
using System.Xml.Serialization;
using WorkFlowMonitorServiceReference;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IWorkFlowExceptionService _exceptionService;
    private readonly IJobServiceClient _jobServiceClient;
    private readonly IWorkflowMonitorServiceClient _workflowMonitorServiceClient;

    public Worker(ILogger<Worker> logger, IWorkFlowExceptionService exceptionService, IJobServiceClient jobServiceClient, IWorkflowMonitorServiceClient workflowMonitorServiceClient)
    {
        _logger = logger;
        _exceptionService = exceptionService;
        _jobServiceClient = jobServiceClient;
        _workflowMonitorServiceClient = workflowMonitorServiceClient;
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

                        if (job.JOBTYPE_ID == Constants.INSTALL)
                        {
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
                    }
                }
                else
                {
                    _logger.LogInformation("No Exceptions");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing exceptions.");
            }

            await Task.Delay(8000, stoppingToken);
        }
    }

    private async Task ReprocessEnrouteExceptionsAsync(WorkflowExceptionRequest reprocessRequest, string xmlData)
    {
        if (!string.IsNullOrWhiteSpace(xmlData) && TryDeserializeEnrouteFromXml(xmlData, out SetEmployeeToEnRouteRequest deserializedRequest))
        {
            var response = await _workflowMonitorServiceClient.ReprocessEnrouteExceptionsAsync(reprocessRequest, deserializedRequest.adUserName);

            if (response != null && response.ReturnValue)
            {
                // Success in reprocessing
            }
            else
            {
                // Fail to reprocess
            }
        }
    }

    private async Task ReprocessOnSiteExceptionsAsync(WorkflowExceptionRequest reprocessRequest, string xmlData)
    {
        if (!string.IsNullOrWhiteSpace(xmlData) && TryDeserializeOnSiteFromXml(xmlData, out SetEmployeeToOnSiteRequest deserializedRequest))
        {
            var response = await _workflowMonitorServiceClient.ReprocessOnSiteExceptionsAsync(reprocessRequest, deserializedRequest.adUserName);

            if (response != null && response.ReturnValue)
            {
                // Success in reprocessing
            }
            else
            {
                // Fail to reprocess
            }
        }
    }

    private async Task ReprocessOnClearAppointmentsExceptionsAsync(WorkflowExceptionRequest reprocessRequest, string xmlData)
    {
        if (!string.IsNullOrWhiteSpace(xmlData) && TryDeserializeClearFromXml(xmlData, out ClearAppointmentRequestModel deserializedRequest))
        {
            var response = await _workflowMonitorServiceClient.ReprocessClearAppointmentExceptionsAsync(reprocessRequest, deserializedRequest.adUserName);

            if (response != null && response.ReturnValue)
            {
                // Success in reprocessing
            }
            else
            {
                // Fail to reprocess
            }
        }
    }

    public static bool TryDeserializeEnrouteFromXml(string xml, out SetEmployeeToEnRouteRequest? result)
    {
        var xmlSerializer = new XmlSerializer(typeof(SetEmployeeToEnRouteRequest));
        try
        {
            using (var stringReader = new StringReader(xml))
            {
                result = xmlSerializer.Deserialize(stringReader) as SetEmployeeToEnRouteRequest;
                return result != null;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deserializing XML: {ex.Message}");
            result = null;
            return false;
        }
    }

    public static bool TryDeserializeOnSiteFromXml(string xml, out SetEmployeeToOnSiteRequest? result)
    {
        var xmlSerializer = new XmlSerializer(typeof(SetEmployeeToOnSiteRequest));
        try
        {
            using (var stringReader = new StringReader(xml))
            {
                result = xmlSerializer.Deserialize(stringReader) as SetEmployeeToOnSiteRequest;
                return result != null;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deserializing XML: {ex.Message}");
            result = null;
            return false;
        }
    }

    public static bool TryDeserializeClearFromXml(string xml, out ClearAppointmentRequestModel? result)
    {
        var xmlSerializer = new XmlSerializer(typeof(ClearAppointmentRequestModel));
        try
        {
            using (var stringReader = new StringReader(xml))
            {
                result = xmlSerializer.Deserialize(stringReader) as ClearAppointmentRequestModel;
                return result != null;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deserializing XML: {ex.Message}");
            result = null;
            return false;
        }
    }
}
