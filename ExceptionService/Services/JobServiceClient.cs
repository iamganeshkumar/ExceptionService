using ExceptionService.Configuration.Models;
using ExceptionService.Interfaces;
using ExceptionServiceReference;
using Microsoft.Extensions.Options;

namespace ExceptionService.Services
{
    public class JobServiceClient : IJobServiceClient
    {
        private readonly JobServiceSoapClient _client;

        public JobServiceClient
            (IOptions<SoapEndpointOptions> endpointOptions)
        {
            _client = new JobServiceSoapClient(JobServiceSoapClient.EndpointConfiguration.JobServiceSoap, endpointOptions.Value.JobServiceBaseUrl);
        }

        public async Task<Job> GetJobAsync(long jobNumber)
        {
            var job = await _client.GetJobAsync(jobNumber);
            return job;
        }
    }
}
