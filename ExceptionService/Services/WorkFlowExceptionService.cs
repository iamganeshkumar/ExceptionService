using ExceptionService.Configuration.Models;
using ExceptionService.Data;
using ExceptionService.Interfaces;
using ExceptionService.Models;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using WorkFlowMonitorServiceReference;

namespace ExceptionService.Services
{
    public class WorkFlowExceptionService : IWorkFlowExceptionService
    {
        IOptions<WorkFlowMonitorTableRecordsOptions> _records;
        ILogger<WorkFlowExceptionService> _logger;
        static bool fistIteration = true;
        private readonly OpsMobWwfContext _context;
        List<WorkflowException> exceptions = new List<WorkflowException>();
        WorkflowException lastException = new WorkflowException();

        public WorkFlowExceptionService(OpsMobWwfContext context, ILogger<WorkFlowExceptionService> logger, IOptions<WorkFlowMonitorTableRecordsOptions> records)
        {
            _context = context;
            _logger = logger;
            _records = records;
        }
        public IList<WorkflowException> GetWorkflowExceptions()
        {
            try
            {
                Stopwatch stopwatch = new Stopwatch();

                if (fistIteration)
                {
                    _logger.LogInformation("First iteration is true");
                    _logger.LogInformation("Getting records from past {days} days", _records.Value.Days);

                    if (_records.Value.ProcessAllRecords)
                    {
                        stopwatch.Start();
                        exceptions = _context.WorkflowExceptions.ToList();
                        stopwatch.Stop();
                        TimeSpan elapsedTime = stopwatch.Elapsed;
                        _logger.LogInformation("Time taken to retieve workflowexceptions from database is: {ElapsedMilliseconds} ms", elapsedTime.TotalMilliseconds);
                    }
                    else
                    {
                        stopwatch.Start();
                        exceptions = _context.WorkflowExceptions.Where(e => e.CreateDate > DateTime.Now.AddDays(-(_records.Value.Days))
                        && (e.Type == nameof(ExceptionType.Enroute) || e.Type == nameof(ExceptionType.Clear) || e.Type == nameof(ExceptionType.OnSite))).ToList();
                        stopwatch.Stop();
                        TimeSpan elapsedTime = stopwatch.Elapsed;
                        _logger.LogInformation("Time taken to retieve workflowexceptions from database is: {ElapsedMilliseconds} ms", elapsedTime.TotalMilliseconds);
                    }
                    _logger.LogInformation("Retrieved {count} records", exceptions.Count);
                    SaveLastRecord();
                    fistIteration = false;
                }

                else
                {
                    var startWithLastException = _context.LastWorkFlowExceptions.OrderByDescending(i => i.CreateDate).FirstOrDefault();

                    if (startWithLastException != null)
                    {
                        _logger.LogInformation("Now getting records greater than {date}", startWithLastException.CreateDate?.ToString("yyyy-MM-dd:HH:mm"));
                        stopwatch.Start();
                        exceptions = _context.WorkflowExceptions.Where(i => i.CreateDate > startWithLastException.CreateDate).ToList();
                        stopwatch.Stop();
                        TimeSpan elapsedTime = stopwatch.Elapsed;
                        _logger.LogInformation("Time taken to retieve workflowexceptions from database is: {ElapsedMilliseconds} ms", elapsedTime.TotalMilliseconds);
                        _logger.LogInformation("Retrieved {count} records greater than {date} date", exceptions.Count, startWithLastException.CreateDate?.ToString("yyyy-MM-dd:HH:mm"));

                        SaveLastRecord();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error in GetWorkflowException() method");
                _logger.LogError("Detailed Error - " + ex.Message);
            }

            return exceptions;
        }

        private void SaveLastRecord()
        {
            try
            {
                if (exceptions.Count > 0)
                {
                    _logger.LogInformation("Saving last retrieved record");

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
                        _logger.LogInformation("Record {id} saved in LastWorkFlowException table", lastException.Id);
                    }
                    else
                    {
                        _logger.LogInformation("Record {id} already exists in LastWorkFlowException table", lastException.Id);
                    }
                }
            }
            catch(Exception ex) 
            {
                _logger.LogError("Error in SaveLastRecord() method");
                _logger.LogError("Detailed Error - " + ex.Message);
            }
        }

        private bool DoesNotExistInDataBase(Guid Id)
        {
            return _context.LastWorkFlowExceptions.FirstOrDefault(i => i.Id == Id) == null;
        }
    }
}
