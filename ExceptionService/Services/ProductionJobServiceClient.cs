﻿using ExceptionService.Interfaces;
using ExceptionService.Models;
using ExceptionServiceReference;

namespace ExceptionService.Services
{
    public class ProductionJobServiceClient : IJobServiceClient
    {
        private readonly JobServiceSoapClient _client;

        public ProductionJobServiceClient()
        {
            _client = new JobServiceSoapClient(JobServiceSoapClient.EndpointConfiguration.JobServiceSoap);
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