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
            Stopwatch stopwatch = new Stopwatch();
            try
            {
                var typeOrder = new Dictionary<string, int>
                        {
                            { nameof(ExceptionType.Enroute), 1 },
                            { nameof(ExceptionType.OnSite), 2 },
                            { nameof(ExceptionType.Clear), 3 }
                        };

                if (_records.Value.ProcessAllRecords)
                {
                    var lastExceptionRecord = _context.LastWorkFlowExceptions.OrderByDescending(i => i.CreateDate).FirstOrDefault();

                    if (lastExceptionRecord != null)
                    {
                        _logger.LogInformation("Getting records greater than {date}", lastExceptionRecord.CreateDate?.ToString("yyyy-MM-dd:HH:mm"));

                        stopwatch.Start();

                        exceptions = _context.WorkflowExceptions
                        .Where(e => (e.Type == nameof(ExceptionType.Enroute)
                        || e.Type == nameof(ExceptionType.Clear)
                        || e.Type == nameof(ExceptionType.OnSite))
                        && e.CreateDate > lastExceptionRecord.CreateDate
                        && !_context.ReprocessedExceptions.Any(re => re.JobNumber == e.JobNumber && re.JobSequenceNo == e.JobSeqNumber))
                        .ToList()
                        .OrderBy(e => typeOrder[e.Type])
                        .ToList();

                        stopwatch.Stop();
                        TimeSpan elapsedTime = stopwatch.Elapsed;
                        _logger.LogInformation("Time taken to retieve workflowexceptions from database is: {ElapsedMilliseconds} ms", elapsedTime.TotalMilliseconds);
                    }
                    else
                    {
                        _logger.LogInformation("Getting All Records from Database which are not reprocessed...");

                        stopwatch.Start();

                        exceptions = _context.WorkflowExceptions
                        .Where(e => (e.Type == nameof(ExceptionType.Enroute)
                        || e.Type == nameof(ExceptionType.Clear)
                        || e.Type == nameof(ExceptionType.OnSite))
                        && !_context.ReprocessedExceptions.Any(re => re.JobNumber == e.JobNumber && re.JobSequenceNo == e.JobSeqNumber))
                        .ToList()
                        .OrderBy(e => typeOrder[e.Type])
                        .ToList();

                        stopwatch.Stop();
                        TimeSpan elapsedTime = stopwatch.Elapsed;
                        _logger.LogInformation("Time taken to retieve workflowexceptions from database is: {ElapsedMilliseconds} ms", elapsedTime.TotalMilliseconds);
                    }
                }
                else
                {
                    if (fistIteration)
                    {
                        _logger.LogInformation("Getting records from past {days} days which are not reprocessed...", _records.Value.Days);
                        stopwatch.Start();

                        exceptions = _context.WorkflowExceptions
                            .Where(e => e.CreateDate > DateTime.Now.AddDays(-(_records.Value.Days))
                            && (e.Type == nameof(ExceptionType.Enroute)
                            || e.Type == nameof(ExceptionType.Clear)
                            || e.Type == nameof(ExceptionType.OnSite))
                            && !_context.ReprocessedExceptions.Any(re => re.JobNumber == e.JobNumber && re.JobSequenceNo == e.JobSeqNumber))
                            .ToList()
                            .OrderBy(e => typeOrder[e.Type])
                            .ToList();

                        stopwatch.Stop();
                        TimeSpan elapsedTime = stopwatch.Elapsed;
                        _logger.LogInformation("Time taken to retieve workflowexceptions from database is: {ElapsedMilliseconds} ms", elapsedTime.TotalMilliseconds);
                        fistIteration = false;
                    }
                    else
                    {
                        var lastExceptionRecord = _context.LastWorkFlowExceptions.OrderByDescending(i => i.CreateDate).FirstOrDefault();

                        if (lastExceptionRecord != null)
                        {
                            _logger.LogInformation("Getting records greater than {date}", lastExceptionRecord.CreateDate?.ToString("yyyy-MM-dd:HH:mm"));

                            stopwatch.Start();

                            exceptions = _context.WorkflowExceptions
                                .Where(e => (e.Type == nameof(ExceptionType.Enroute)
                                || e.Type == nameof(ExceptionType.Clear)
                                || e.Type == nameof(ExceptionType.OnSite))
                                && e.CreateDate > lastExceptionRecord.CreateDate
                                && !_context.ReprocessedExceptions.Any(re => re.JobNumber == e.JobNumber && re.JobSequenceNo == e.JobSeqNumber))
                                .ToList()
                                .OrderBy(e => typeOrder[e.Type])
                                .ToList();

                            stopwatch.Stop();
                            TimeSpan elapsedTime = stopwatch.Elapsed;
                            _logger.LogInformation("Time taken to retieve workflowexceptions from database is: {ElapsedMilliseconds} ms", elapsedTime.TotalMilliseconds);
                        }
                    }
                }


                _logger.LogInformation("Retrieved {count} records", exceptions.Count);
                SaveLastRecord();
            }
            catch (Exception ex)
            {
                if (stopwatch != null) { stopwatch.Stop(); }
                _logger.LogError("Error in GetWorkflowException() method");
                _logger.LogError("Detailed Error - " + ex);
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
                _logger.LogError("Error in SaveLastRecord() method occurred while trying to save last record for Id {id} with jobnumber {jobno}", lastException.Id, lastException.JobNumber);
                _logger.LogError("Detailed Error - " + ex);
            }
        }

        public void SaveReprocessedRecord(WorkflowExceptionRequest workflowExceptionRequest, bool isReprocessed)
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
                            JobNumber = workflowExceptionRequest?.JobNumber,
                            JobSequenceNo = workflowExceptionRequest?.JobSequenceNumber,
                            Type = workflowExceptionRequest?.Type.ToString(),
                            ReprocessedDateTime = DateTime.Now,
                            IsReprocessed = isReprocessed
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
                _logger.LogError("Error in SaveReprocessedRecordAsync() method occurred while trying to save reprocess record for Id {id} with jobno {jobno}", workflowExceptionRequest.Id, workflowExceptionRequest.JobNumber);
                _logger.LogError("Detailed Error - " + ex);
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
