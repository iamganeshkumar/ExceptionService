using ExceptionService.Models;
namespace ExceptionService.Interfaces
{
    public interface IWorkFlowExceptionService
    {
        public IList<WorkflowException> GetWorkflowExceptions();
    }
}
