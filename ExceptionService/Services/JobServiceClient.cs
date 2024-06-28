using ExceptionService.Configuration.Models;
using ExceptionService.Interfaces;
using ExceptionServiceReference;
using Microsoft.Extensions.Options;
using System.Diagnostics;

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
            Job job = new Job();

            try
            {
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                job = await _client.GetJobAsync(jobNumber);
                stopwatch.Stop();
                TimeSpan elapsedTime = stopwatch.Elapsed;
                _logger.LogInformation("Time taken to GetJob type is: {ElapsedMilliseconds} ms", elapsedTime.TotalMilliseconds);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error in GetJobAsync() method in Services");
                _logger.LogError("Detailed Error - " + ex.Message);
                return null;
            }

            return job;
        }
    }
}
