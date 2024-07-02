using System.Net;
using ExceptionService.Interfaces;
using ExceptionService.Services;
using ExceptionService.Data;
using Serilog;
using ExceptionService.Configuration.Models;
using ExceptionService.Common;
using ExceptionService.Mock.Services;
using Serilog.Events;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<SoapEndpointOptions>(
    builder.Configuration.GetSection(
        key: nameof(SoapEndpointOptions)));

builder.Services.Configure<WorkFlowMonitorTableRecordsOptions>(
    builder.Configuration.GetSection(
        key: nameof(WorkFlowMonitorTableRecordsOptions)));

builder.Services.Configure<DurationOptions>(
    builder.Configuration.GetSection(
        key: nameof(DurationOptions)));

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning) // Set minimum level to Warning for Microsoft logs
    .MinimumLevel.Debug()
    .Enrich.FromLogContext()
    .WriteTo.Logger(lc => lc
        .Filter.ByIncludingOnly(le => le.Level == LogEventLevel.Information)
        .WriteTo.File(
            path: "Logs/Information/log-.txt",
            outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
            rollingInterval: RollingInterval.Day,
            shared: true))
    .WriteTo.Logger(lc => lc
        .Filter.ByIncludingOnly(le => le.Level >= LogEventLevel.Error)
        .WriteTo.File(
            path: "Logs/Error/log-.txt",
            outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
            rollingInterval: RollingInterval.Day,
            shared: true))
    .CreateLogger();

builder.Logging.AddSerilog();
builder.Services.AddWindowsService();
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
builder.Services.AddSingleton<IDeserialization, Deserialization>();

ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

var host = builder.Build();
host.Run();

// Ensure to flush and close the log at application exit
Log.CloseAndFlush();