using System.Net;
using ExceptionService.Interfaces;
using ExceptionService.Services;
using ExceptionService.Data;
using Serilog;
using ExceptionService.Configuration.Models;
using ExceptionService.Common;
using ExceptionService.Mock.Services;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<SoapEndpointOptions>(
    builder.Configuration.GetSection(
        key: nameof(SoapEndpointOptions)));

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.File(
        path: "Logs/Information/log-.txt",
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
        rollingInterval: RollingInterval.Day,
        restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information)
    .WriteTo.File(
        path: "Logs/Error/log-.txt",
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
        rollingInterval: RollingInterval.Day,
        restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Error)
    .CreateLogger();

builder.Logging.AddSerilog();

// Add services to the container

builder.Services.AddHostedService<Worker>();

if (Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") == Constants.Development)
{
    builder.Services.AddSingleton<IJobServiceClient, DevJobServiceClient>();
    builder.Services.AddSingleton<IWorkflowMonitorServiceClient, DevWorkflowMonitorServiceClient>();
}
else
{
    builder.Services.AddSingleton<IJobServiceClient, JobServiceClient>();
    builder.Services.AddSingleton<IWorkflowMonitorServiceClient, WorkFlowMonitorServiceClient>();
}

// Register application services
builder.Services.AddSingleton<IWorkFlowExceptionService, WorkFlowExceptionService>();
builder.Services.AddSingleton<OpsMobWwfContext, OpsMobWwfContext>();

// Register the factory
//builder.Services.AddSingleton<ServiceClientFactory>();

ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

var host = builder.Build();
host.Run();

// Ensure to flush and close the log at application exit
Log.CloseAndFlush();