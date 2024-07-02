using TSOpsExceptionService.Interfaces;
using TSOpsExceptionServiceReference;

namespace TSOpsExceptionService.Mock.Services
{
    public class DevJobServiceClient : IJobServiceClient
    {
        public Task<Job> GetJobAsync(long jobNumber)
        {
            // Mock data
            var job = new Job
            {
                JOBTYPE_ID = "INST" // Set mock job type ID
            };

            return Task.FromResult(job);
        }
    }
}
