using ExceptionService.Interfaces;
using ExceptionService.Mock.Models;
using ExceptionService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExceptionService.Mock.Services
{
    public class DevJobServiceClient : IJobServiceClient
    {
        public Task<JobModel> GetJobAsync(long jobNumber)
        {
            // Mock data
            var job = new MockJob
            {
                JOBTYPE_ID = "INST" // Set mock job type ID
            };

            var jobModel = new JobModel
            {
                JobTypeId = job.JOBTYPE_ID
                // Map other necessary properties here
            };

            return Task.FromResult(jobModel);
        }
    }
}
