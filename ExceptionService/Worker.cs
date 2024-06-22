using ExceptionService.Interfaces;
using ExceptionService.Common;
using ExceptionService.Factory;
using ExceptionService.Requests;
using System.Xml.Serialization;
using ExceptionService.Enums;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IWorkFlowExceptionService _exceptionService;
    private readonly ServiceClientFactory _serviceClientFactory;

    public Worker(ILogger<Worker> logger, IWorkFlowExceptionService exceptionService, ServiceClientFactory serviceClientFactory)
    {
        _logger = logger;
        _exceptionService = exceptionService;
        _serviceClientFactory = serviceClientFactory;
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
                        var jobServiceClient = _serviceClientFactory.CreateJobServiceClient();
                        var job = await jobServiceClient.GetJobAsync(exception.JobNumber.GetValueOrDefault());

                        if (job.JobTypeId == Constants.INSTALL)
                        {
                            var workflowMonitorClient = _serviceClientFactory.CreateWorkflowMonitorClient();
                            var request = new WorkflowExceptionRequest
                            {
                                Id = exception.Id,
                                CreateDate = exception.CreateDate ?? DateTime.MinValue,
                                ErrorInformation = exception.ErrorInfo ?? string.Empty,
                                IsBusinessError = exception.IsBusinessError ?? false,
                                JobNumber = exception.JobNumber,
                                JobSequenceNumber = exception.JobSeqNumber,
                                Type = workflowMonitorClient.MapServiceExceptionTypeToCommonExceptionType(exception.Type)
                            };

                            if (request.Type == CommonExceptionType.Enroute)
                            {
                                await ReprocessEnrouteExceptionsAsync(request, workflowMonitorClient, exception.Data);
                            }
                            else if (request.Type == CommonExceptionType.Clear)
                            {
                                await ReprocessOnClearAppointmentsExceptionsAsync(request, workflowMonitorClient, exception.Data);
                            }
                            else if (request.Type == CommonExceptionType.OnSite)
                            {
                                await ReprocessOnSiteExceptionsAsync(request, workflowMonitorClient, exception.Data);
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

    private async Task ReprocessEnrouteExceptionsAsync(WorkflowExceptionRequest reprocessRequest, IWorkflowMonitorServiceClient workflowMonitorClient, string xmlData)
    {
        if (!string.IsNullOrWhiteSpace(xmlData) && TryDeserializeEnrouteFromXml(xmlData, out SetEmployeeToEnRouteRequest deserializedRequest))
        {
            var response = await workflowMonitorClient.ReprocessEnrouteExceptionsAsync(reprocessRequest, deserializedRequest.AdUserName);

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

    private async Task ReprocessOnSiteExceptionsAsync(WorkflowExceptionRequest reprocessRequest, IWorkflowMonitorServiceClient workflowMonitorClient, string xmlData)
    {
        if (!string.IsNullOrWhiteSpace(xmlData) && TryDeserializeOnSiteFromXml(xmlData, out SetEmployeeToOnSiteRequest deserializedRequest))
        {
            var response = await workflowMonitorClient.ReprocessOnSiteExceptionsAsync(reprocessRequest, deserializedRequest.AdUserName);

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

    private async Task ReprocessOnClearAppointmentsExceptionsAsync(WorkflowExceptionRequest reprocessRequest, IWorkflowMonitorServiceClient workflowMonitorClient, string xmlData)
    {
        if (!string.IsNullOrWhiteSpace(xmlData) && TryDeserializeClearFromXml(xmlData, out ClearAppointmentRequest deserializedRequest))
        {
            var response = await workflowMonitorClient.ReprocessClearAppointmentExceptionsAsync(reprocessRequest, deserializedRequest.AdUserName);

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

    public static bool TryDeserializeClearFromXml(string xml, out ClearAppointmentRequest? result)
    {
        var xmlSerializer = new XmlSerializer(typeof(ClearAppointmentRequest));
        try
        {
            using (var stringReader = new StringReader(xml))
            {
                result = xmlSerializer.Deserialize(stringReader) as ClearAppointmentRequest;
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
