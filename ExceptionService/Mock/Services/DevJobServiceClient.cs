using ExceptionService.Interfaces;
using ExceptionService.Mock.Models;
using ExceptionServiceReference;

namespace ExceptionService.Mock.Services
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
