using System.Net;
using ExceptionService.Interfaces;
using ExceptionService.Services;
using ExceptionService.Data;
using ExceptionService.Factory;
using ExceptionService.Common;

var builder = Host.CreateApplicationBuilder(args);

// Add services to the container
builder.Services.AddHostedService<Worker>();

if (Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") == Constants.Production)
{
    builder.Services.AddSingleton<IJobServiceClient, ProductionJobServiceClient>();
    builder.Services.AddSingleton<IWorkflowMonitorServiceClient, ProductionWorkFlowMonitorServiceClient>();
}

if (Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") == Constants.UAT)
{
    builder.Services.AddSingleton<IJobServiceClient, UATJobServiceClient>();
    builder.Services.AddSingleton<IWorkflowMonitorServiceClient, UATWorkflowMonitorServiceClient>();
}

// Register application services
builder.Services.AddSingleton<IWorkFlowExceptionService, WorkFlowExceptionService>();
builder.Services.AddSingleton<OpsMobWwfprodContext, OpsMobWwfprodContext>();
builder.Services.AddSingleton<IXmlDeserializer, XmlDeserializer>();

// Register the factory
builder.Services.AddSingleton<ServiceClientFactory>();

ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

var host = builder.Build();
host.Run();
