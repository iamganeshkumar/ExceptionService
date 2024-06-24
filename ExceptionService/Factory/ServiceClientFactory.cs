using ExceptionService.Common;
using ExceptionService.Interfaces;
using ExceptionService.Mock.Services;
using ExceptionService.Services;

namespace ExceptionService.Factory
{
    public class ServiceClientFactory
    {
        private readonly string _environment;

        public ServiceClientFactory()
        {
            _environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production";
        }

        //public IJobServiceClient CreateJobServiceClient()
        //{
        //    return _environment switch
        //    {
        //        Constants.Production => new ProductionJobServiceClient(),
        //        Constants.UAT => new UATJobServiceClient(),
        //        Constants.Development => new DevJobServiceClient(),
        //        _ => new ProductionJobServiceClient(),
        //    };
        //}

        public IWorkflowMonitorServiceClient CreateWorkflowMonitorClient()
        {
            return _environment switch
            {
                Constants.Production => new ProductionWorkFlowMonitorServiceClient(),
                Constants.UAT => new UATWorkflowMonitorServiceClient(),
                Constants.Development => new DevWorkflowMonitorServiceClient(),
                _ => new ProductionWorkFlowMonitorServiceClient()
            };
        }
    }
}
