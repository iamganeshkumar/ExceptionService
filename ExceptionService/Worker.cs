using ExceptionService.Common;
using ExceptionService.Data;
using ExceptionService.Interfaces;
using ExceptionServiceReference;
using Microsoft.EntityFrameworkCore;
using System;

namespace ExceptionService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IWorkFlowExceptionService _exceptionService;
        //private readonly OpsMobWwfprodContext _dbContext;

        public Worker(ILogger<Worker> logger, IWorkFlowExceptionService exceptionService)
        {
            _logger = logger;
            _exceptionService = exceptionService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Job job = new Job();

            while (!stoppingToken.IsCancellationRequested)
            {
                //if (_logger.IsEnabled(LogLevel.Information))
                //{
                //    _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                //}
                //await Task.Delay(1000, stoppingToken);

                //using OpsMobWwfprodContext context = new OpsMobWwfprodContext();

                //foreach (var item in context.WorkflowExceptions)
                //{
                //    _logger.LogInformation("Id is: {Id}", item.Id);
                //}
                var exceptions = _exceptionService.GetWorkflowExceptions();

                if (exceptions.Count > 0)
                {
                    foreach (var exception in exceptions.OrderByDescending(i => i.CreateDate))
                    {
                        if (Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") == Constants.Production)
                        {
                            ExceptionServiceReference.JobServiceSoapClient jobServiceSoapClient
                                = new ExceptionServiceReference.JobServiceSoapClient(ExceptionServiceReference.JobServiceSoapClient.EndpointConfiguration.JobServiceSoap);
                            job = await jobServiceSoapClient.GetJobAsync(exception.JobNumber.GetValueOrDefault());

                            if (job.JOBTYPE_ID == Constants.INSTALL && exception.Type == Constants.Enroute)
                            {
                                var a = exception.Id.ToString();
                                // Do Retry
                                WorkFlowMonitorServiceReference.WorkflowMonitorClient workflowMonitorClient
                                    = new WorkFlowMonitorServiceReference.WorkflowMonitorClient(WorkFlowMonitorServiceReference.WorkflowMonitorClient.EndpointConfiguration.BasicHttpsBinding_IWorkflowMonitor);
                                var abc = await workflowMonitorClient.GetEnrouteExceptionAsync(exception.Id);                                
                            }
                        }

                        //if (Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") == Constants.UAT)
                        //{
                        //    ExceptionUATServiceReference.JobServiceSoapClient jobServiceSoapClient
                        //        = new ExceptionUATServiceReference.JobServiceSoapClient(ExceptionUATServiceReference.JobServiceSoapClient.EndpointConfiguration.JobServiceSoap);
                        //    var job = await jobServiceSoapClient.GetJobAsync(exception.JobNumber.GetValueOrDefault());
                        //}
                        

                        
                        _logger.LogInformation("Error is: {error}", exception.Data);
                    }
                }

                else
                {
                    _logger.LogInformation("No Exceptions");
                }

                await Task.Delay(8000, stoppingToken);
            }
        }
    }
}
