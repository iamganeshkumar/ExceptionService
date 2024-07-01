using ExceptionServiceReference;

public interface IJobServiceSoapClient
{
    Task<Job> GetJobAsync(long jobNumber);
}
