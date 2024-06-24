using ExceptionServiceReference;

namespace ExceptionService.Interfaces
{
    public interface IJobServiceClient
    {
        Task<Job> GetJobAsync(long jobNumber);
    }
}