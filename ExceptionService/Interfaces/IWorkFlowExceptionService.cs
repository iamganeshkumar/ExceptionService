using ExceptionService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExceptionService.Interfaces
{
    public interface IWorkFlowExceptionService
    {
        public IList<WorkflowException> GetWorkflowExceptions();
    }
}
