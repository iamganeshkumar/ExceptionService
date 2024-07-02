using WorkFlowMonitorServiceReference;

namespace TSOpsExceptionService.Requests
{
    public class WorkflowExceptionRequest
    {
        public Guid Id { get; set; }
        public DateTime CreateDate { get; set; }
        public string ErrorInformation { get; set; }
        public bool IsBusinessError { get; set; }
        public long? JobNumber { get; set; }
        public int? JobSequenceNumber { get; set; }
        public ExceptionType Type { get; set; }
    }
}
