using TSOpsExceptionService.Configuration.Models;
using TSOpsExceptionService.Interfaces;
using ExceptionServiceReference;
using Microsoft.Extensions.Options;
using System.Diagnostics;

namespace TSOpsExceptionService.Services
{
    public class JobServiceClient : IJobServiceClient
    {
        private readonly IJobServiceSoapClient _client;
        private readonly ILogger _logger;

        public JobServiceClient
            (IOptions<SoapEndpointOptions> endpointOptions, ILogger<JobServiceClient> logger, IJobServiceSoapClient client = null)
        {
            _logger = logger;
            _client = client ?? new JobServiceSoapClientWrapper(new JobServiceSoapClient(JobServiceSoapClient.EndpointConfiguration.JobServiceSoap, endpointOptions.Value.JobServiceBaseUrl));
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
                _logger.LogInformation("Time taken to GetJob type for jobnumber {jobno} is: {ElapsedMilliseconds} ms", jobNumber,  elapsedTime.TotalMilliseconds);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error in GetJobAsync() method in Services while processing jobnumber {jobno}", jobNumber);
                _logger.LogError("Detailed Error - " + ex);
                return null;
            }

            return job;
        }
    }
}
