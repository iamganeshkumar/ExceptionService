using ExceptionService.Configuration.Models;
using ExceptionService.Interfaces;
using ExceptionService.Models;
using ExceptionServiceReference;
using Microsoft.Extensions.Options;

namespace ExceptionService.Services
{
    public class ProductionJobServiceClient : IJobServiceClient
    {
        private readonly JobServiceSoapClient _client;

        public ProductionJobServiceClient
            (IOptions<SoapEndpointOptions> endpointOptions)
        {
            _client = new JobServiceSoapClient(JobServiceSoapClient.EndpointConfiguration.JobServiceSoap, endpointOptions.Value.ServiceBaseUrl);
        }

        public async Task<JobModel> GetJobAsync(long jobNumber)
        {
            var job = await _client.GetJobAsync(jobNumber);
            return new JobModel
            {
                JobTypeId = job.JOBTYPE_ID
            };
        }
    }
}
