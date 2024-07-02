using TSOpsExceptionServiceReference;

public interface IJobServiceSoapClient
{
    Task<Job> GetJobAsync(long jobNumber);
}
