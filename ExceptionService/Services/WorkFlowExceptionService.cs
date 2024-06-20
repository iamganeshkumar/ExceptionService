using ExceptionService.Data;
using ExceptionService.Interfaces;
using ExceptionService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExceptionService.Services
{
    public class WorkFlowExceptionService : IWorkFlowExceptionService
    {
        static bool fistIteration = true;
        private readonly OpsMobWwfprodContext _context;
        List<WorkflowException> exceptions = new List<WorkflowException>();
        WorkflowException lastException = new WorkflowException();

        public WorkFlowExceptionService(OpsMobWwfprodContext context)
        {
            _context = context; 
        }
        public IList<WorkflowException> GetWorkflowExceptions()
        {
            if (fistIteration)
            {
                exceptions = _context.WorkflowExceptions.Where(i => i.CreateDate > DateTime.Now.AddDays(-2)).ToList();
                //SaveLastRecord();
                fistIteration = false;
            }

            else
            {
                var startWithLastException = _context.LastWorkFlowExceptions.OrderByDescending(i => i.CreateDate).FirstOrDefault();

                if (startWithLastException != null)
                {
                    exceptions = _context.WorkflowExceptions.Where(i => i.CreateDate > startWithLastException.CreateDate).ToList();
                    SaveLastRecord();
                }
            }

            return exceptions;
        }

        private void SaveLastRecord()
        {
            if (exceptions.Count > 0)
            {
                lastException = exceptions.OrderByDescending(i => i.CreateDate).First();

                if (DoesNotExistInDataBase(lastException.Id))
                {
                    LastWorkFlowException lastWorkFlowException = new LastWorkFlowException()
                    {
                        Id = lastException.Id,
                        CreateDate = lastException.CreateDate
                    };

                    _context.LastWorkFlowExceptions.Add(lastWorkFlowException);
                    _context.SaveChanges();
                }
            }
        }

        private bool DoesNotExistInDataBase(Guid Id)
        {
            return _context.LastWorkFlowExceptions.FirstOrDefault(i => i.Id == Id) == null;
        }
    }
}
