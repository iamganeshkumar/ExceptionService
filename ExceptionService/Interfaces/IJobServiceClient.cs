using ExceptionService.Models;

namespace ExceptionService.Interfaces
{
    public interface IJobServiceClient
    {
        Task<JobModel> GetJobAsync(long jobNumber);
    }
}