using TSOpsExceptionServiceReference;

namespace TSOpsExceptionService.Interfaces
{
    public interface IJobServiceClient
    {
        Task<Job> GetJobAsync(long jobNumber);
    }
}