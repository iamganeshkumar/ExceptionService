using ExceptionService.Configuration.Models;
using ExceptionService.Interfaces;
using ExceptionServiceReference;
using Microsoft.Extensions.Options;

namespace ExceptionService.Services
{
    public class JobServiceClient : IJobServiceClient
    {
        private readonly JobServiceSoapClient _client;
        private readonly ILogger _logger;

        public JobServiceClient
            (IOptions<SoapEndpointOptions> endpointOptions, ILogger<JobServiceClient> logger)
        {
            _logger = logger;
            _client = new JobServiceSoapClient(JobServiceSoapClient.EndpointConfiguration.JobServiceSoap, endpointOptions.Value.JobServiceBaseUrl);
            _logger.LogInformation("Job Service Endpoint - {endpoint}", endpointOptions.Value.JobServiceBaseUrl);
        }

        public async Task<Job> GetJobAsync(long jobNumber)
        {
            var job = await _client.GetJobAsync(jobNumber);
            return job;
        }
    }
}
