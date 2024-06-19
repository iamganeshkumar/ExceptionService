using ExceptionService;
using ExceptionService.Data;
using ExceptionService.Interfaces;
using ExceptionService.Services;
using Microsoft.EntityFrameworkCore;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();
builder.Services.AddSingleton<IWorkFlowExceptionService, WorkFlowExceptionService>();
builder.Services.AddSingleton<OpsMobWwfprodContext, OpsMobWwfprodContext>();
//var configSettings = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

//builder.Services.AddDbContext<OpsMobWwfprodContext>(options =>
//options.UseSqlServer(builder.Configuration.GetConnectionString("OpsMobWwfprodConnectionString")));


var host = builder.Build();
host.Run();
