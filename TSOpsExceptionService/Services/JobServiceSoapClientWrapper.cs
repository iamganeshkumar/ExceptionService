using TSOpsExceptionServiceReference;

public class JobServiceSoapClientWrapper : IJobServiceSoapClient
{
    private readonly JobServiceSoapClient _client;

    public JobServiceSoapClientWrapper(JobServiceSoapClient client)
    {
        _client = client;
    }

    public Task<Job> GetJobAsync(long jobNumber)
    {
        return _client.GetJobAsync(jobNumber);
    }
}
