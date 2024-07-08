using System.Net;
using TSOpsExceptionService.Interfaces;
using TSOpsExceptionService.Services;
using TSOpsExceptionService.Data;
using Serilog;
using TSOpsExceptionService.Configuration.Models;
using TSOpsExceptionService.Mock.Services;
using Serilog.Events;
using Microsoft.EntityFrameworkCore;

var builder = Host.CreateApplicationBuilder(args);

// Set the base path to the directory where the executable is located
builder.Configuration.SetBasePath(AppContext.BaseDirectory);

// Add the appsettings.json file
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

// Read the environment name from the configuration
var environmentName = builder.Configuration["EnvironmentSettings:Environment"];

// Add the environment-specific configuration file
builder.Configuration.AddJsonFile($"appsettings.{environmentName}.json", optional: false, reloadOnChange: true);

// Configure services
builder.Services.Configure<SoapEndpointOptions>(
    builder.Configuration.GetSection(
        key: nameof(SoapEndpointOptions)));

builder.Services.Configure<WorkFlowMonitorTableRecordsOptions>(
    builder.Configuration.GetSection(
        key: nameof(WorkFlowMonitorTableRecordsOptions)));

builder.Services.Configure<DurationOptions>(
    builder.Configuration.GetSection(
        key: nameof(DurationOptions)));

// Ensure log directories exist
var logDirectory = Path.Combine(AppContext.BaseDirectory, "Logs");
Directory.CreateDirectory(Path.Combine(logDirectory, "Information"));
Directory.CreateDirectory(Path.Combine(logDirectory, "Error"));


// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning) // Set minimum level to Warning for Microsoft logs
    .MinimumLevel.Debug()
    .Enrich.FromLogContext()
    .WriteTo.Console() // Output logs to console
    .WriteTo.Logger(lc => lc
        .Filter.ByIncludingOnly(le => le.Level == LogEventLevel.Information)
        .WriteTo.File(
            path: Path.Combine(logDirectory, "Information", "log-.txt"),
            outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
            rollingInterval: RollingInterval.Day,
            shared: true))
    .WriteTo.Logger(lc => lc
        .Filter.ByIncludingOnly(le => le.Level >= LogEventLevel.Error)
        .WriteTo.File(
            path: Path.Combine(logDirectory, "Error", "log-.txt"),
            outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
            rollingInterval: RollingInterval.Day,
            shared: true))
    .CreateLogger();

builder.Logging.ClearProviders();
builder.Logging.AddSerilog();

// Add services to the container
builder.Services.AddWindowsService();
builder.Services.AddHostedService<Worker>();

if (environmentName == "Development")
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
builder.Services.AddSingleton<IDeserialization, Deserialization>();

builder.Services.AddDbContext<OpsMobWwfContext>((serviceProvider, optionsBuilder) =>
{
    var configuration = serviceProvider.GetRequiredService<IConfiguration>();
    var logger = serviceProvider.GetRequiredService<ILogger<OpsMobWwfContext>>();
    optionsBuilder.UseSqlServer(
        configuration.GetConnectionString("OpsMobWwfConnectionString"),
        sqlOptions => sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 10, // Number of retry attempts
            maxRetryDelay: TimeSpan.FromSeconds(30), // Delay between retries
            errorNumbersToAdd: null // Optional: specify additional SQL error numbers to retry on
        ));
    logger.LogInformation("Connection String is " + configuration.GetConnectionString("OpsMobWwfConnectionString"));
});

ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

var host = builder.Build();
host.Run();

// Ensure to flush and close the log at application exit
Log.CloseAndFlush();
