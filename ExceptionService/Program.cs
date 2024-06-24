using System.Net;
using ExceptionService.Interfaces;
using ExceptionService.Services;
using ExceptionService.Data;
using ExceptionService.Factory;
using ExceptionService.Common;
using Serilog;
using ExceptionService.Configuration.Models;

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

builder.Services.AddSingleton<IJobServiceClient, ProductionJobServiceClient>();
builder.Services.AddSingleton<IWorkflowMonitorServiceClient, ProductionWorkFlowMonitorServiceClient>();

//if (Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") == Constants.Production)
//{
//    builder.Services.AddSingleton<IJobServiceClient, ProductionJobServiceClient>();
//    builder.Services.AddSingleton<IWorkflowMonitorServiceClient, ProductionWorkFlowMonitorServiceClient>();
//}

//if (Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") == Constants.UAT)
//{
//    builder.Services.AddSingleton<IJobServiceClient, UATJobServiceClient>();
//    builder.Services.AddSingleton<IWorkflowMonitorServiceClient, UATWorkflowMonitorServiceClient>();
//}

// Register application services
builder.Services.AddSingleton<IWorkFlowExceptionService, WorkFlowExceptionService>();
builder.Services.AddSingleton<OpsMobWwfprodContext, OpsMobWwfprodContext>();
builder.Services.AddSingleton<IXmlDeserializer, XmlDeserializer>();

// Register the factory
builder.Services.AddSingleton<ServiceClientFactory>();

ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

var host = builder.Build();
host.Run();

// Ensure to flush and close the log at application exit
Log.CloseAndFlush();