using TSOpsExceptionService.Models;
using TSOpsExceptionService.Requests;

namespace TSOpsExceptionService.Interfaces
{
    public interface IWorkFlowExceptionService
    {
        public IList<WorkflowException> GetWorkflowExceptions();
        public void SaveReprocessedRecord(WorkflowExceptionRequest request);
    }
}
