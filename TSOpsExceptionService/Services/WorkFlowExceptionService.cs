using TSOpsExceptionService.Configuration.Models;
using TSOpsExceptionService.Data;
using TSOpsExceptionService.Interfaces;
using TSOpsExceptionService.Models;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using WorkFlowMonitorServiceReference;
using TSOpsExceptionService.Requests;

namespace TSOpsExceptionService.Services
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

                    if (_records.Value.ProcessAllRecords)
                    {
                        _logger.LogInformation("Getting All Records from Database");
                        stopwatch.Start();

                        exceptions = _context.WorkflowExceptions.Where(e => (e.Type == nameof(ExceptionType.Enroute) 
                        || e.Type == nameof(ExceptionType.Clear)
                        || e.Type == nameof(ExceptionType.OnSite))
                        && !_context.ReprocessedExceptions.Any(re => re.JobNumber == e.JobNumber && re.JobSequenceNo == e.JobSeqNumber)).ToList();

                        stopwatch.Stop();
                        TimeSpan elapsedTime = stopwatch.Elapsed;
                        _logger.LogInformation("Time taken to retieve workflowexceptions from database is: {ElapsedMilliseconds} ms", elapsedTime.TotalMilliseconds);
                    }
                    else
                    {
                        _logger.LogInformation("Getting records from past {days} days", _records.Value.Days);
                        stopwatch.Start();

                        exceptions = _context.WorkflowExceptions.Where(e => e.CreateDate > DateTime.Now.AddDays(-(_records.Value.Days))
                        && (e.Type == nameof(ExceptionType.Enroute)
                        || e.Type == nameof(ExceptionType.Clear)
                        || e.Type == nameof(ExceptionType.OnSite))
                        && !_context.ReprocessedExceptions.Any(re => re.JobNumber == e.JobNumber && re.JobSequenceNo == e.JobSeqNumber)).ToList();

                        //exceptions = _context.WorkflowExceptions.Where(e => e.CreateDate > DateTime.Now.AddDays(-(_records.Value.Days))
                        //&& (e.Type == nameof(ExceptionType.Enroute) || e.Type == nameof(ExceptionType.Clear) || e.Type == nameof(ExceptionType.OnSite))).ToList();
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
                    _logger.LogInformation("Saving last retrieved record...");

                    lastException = exceptions.OrderByDescending(i => i.CreateDate).First();

                    if (DoesNotExistInLastWorkFlowExceptionsTable(lastException.Id))
                    {
                        LastWorkFlowException lastWorkFlowException = new LastWorkFlowException()
                        {
                            Id = lastException.Id,
                            CreateDate = lastException.CreateDate
                        };

                        _context.LastWorkFlowExceptions.Add(lastWorkFlowException);
                        _context.SaveChanges();
                        _logger.LogInformation("Record {id} with jobnumber {jobno} saved in LastWorkFlowException table", lastException.Id, lastException.JobNumber);
                    }
                    else
                    {
                        _logger.LogInformation("Record {id} with jobnumber {jobno} already exists in LastWorkFlowException table", lastException.Id, lastException.JobNumber);
                    }
                }
            }
            catch(Exception ex) 
            {
                _logger.LogError("Error in SaveLastRecord() method");
                _logger.LogError("Detailed Error - " + ex.Message);
            }
        }

        public void SaveReprocessedRecord(WorkflowExceptionRequest workflowExceptionRequest)
        {
            try
            {
                if (workflowExceptionRequest != null)
                {
                    _logger.LogInformation("Saving reprocessed record...");

                    if (DoesNotExistInReprocessedExceptionsTable(workflowExceptionRequest.Id))
                    {
                        ReprocessedException reprocessedException = new ReprocessedException()
                        {
                            Id = workflowExceptionRequest.Id,
                            JobNumber = workflowExceptionRequest.JobNumber,
                            JobSequenceNo = workflowExceptionRequest.JobSequenceNumber,
                            ReprocessedDateTime = DateTime.Now
                        };

                        _context.ReprocessedExceptions.Add(reprocessedException);
                        _context.SaveChanges();
                        _logger.LogInformation("Record {id} with jobnumber {jobnumber} saved in ReprocessedExceptions table", reprocessedException.Id, reprocessedException.JobNumber);
                    }
                    else
                    {
                        _logger.LogInformation("Record {id} with jobnumber {jobno} already exists in ReprocessedExceptions table", workflowExceptionRequest.Id, workflowExceptionRequest.JobNumber);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error in SaveReprocessedRecordAsync() method");
                _logger.LogError("Detailed Error - " + ex.Message);
            }
        }

        private bool DoesNotExistInLastWorkFlowExceptionsTable(Guid Id)
        {
            return _context.LastWorkFlowExceptions.FirstOrDefault(i => i.Id == Id) == null;
        }

        private bool DoesNotExistInReprocessedExceptionsTable(Guid id)
        {
            return _context.ReprocessedExceptions.FirstOrDefault(i => i.Id == id) == null;
        }
    }
}
