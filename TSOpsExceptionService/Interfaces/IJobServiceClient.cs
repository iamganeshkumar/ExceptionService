using ExceptionServiceReference;

namespace TSOpsExceptionService.Interfaces
{
    public interface IJobServiceClient
    {
        Task<Job> GetJobAsync(long jobNumber);
    }
}