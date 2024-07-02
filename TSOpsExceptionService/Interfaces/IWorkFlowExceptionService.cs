using TSOpsExceptionService.Models;
namespace TSOpsExceptionService.Interfaces
{
    public interface IWorkFlowExceptionService
    {
        public IList<WorkflowException> GetWorkflowExceptions();
    }
}
