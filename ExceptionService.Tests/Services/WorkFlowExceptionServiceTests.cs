using ExceptionService.Configuration.Models;
using ExceptionService.Data;
using ExceptionService.Models;
using ExceptionService.Services;
using ExceptionService.Tests.Logger;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace ExceptionService.Tests.Services
{
    public class WorkFlowExceptionServiceTests
    {
        private OpsMobWwfContext CreateInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<OpsMobWwfContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new OpsMobWwfContext(options);
        }

        private void ResetStaticFields()
        {
            // Reset the static field `fistIteration` to true before each test
            typeof(WorkFlowExceptionService)
                .GetField("fistIteration", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
                .SetValue(null, true);
        }

        [Fact]
        public void GetWorkflowExceptions_FirstIterationWithAllRecords_LogsRetrievingAllRecords()
        {
            // Arrange
            ResetStaticFields(); // Ensure the static field is reset
            var logger = new InMemoryLogger<WorkFlowExceptionService>();
            var optionsMock = new Mock<IOptions<WorkFlowMonitorTableRecordsOptions>>();
            optionsMock.Setup(o => o.Value).Returns(new WorkFlowMonitorTableRecordsOptions { ProcessAllRecords = true, Days = 30 });

            var context = CreateInMemoryContext();
            context.WorkflowExceptions.AddRange(
                new WorkflowException { Id = Guid.NewGuid(), CreateDate = DateTime.Now.AddDays(-1), Type = "Enroute" },
                new WorkflowException { Id = Guid.NewGuid(), CreateDate = DateTime.Now.AddDays(-2), Type = "Clear" },
                new WorkflowException { Id = Guid.NewGuid(), CreateDate = DateTime.Now.AddDays(-3), Type = "OnSite" }
            );
            context.SaveChanges();

            var service = new WorkFlowExceptionService(context, logger, optionsMock.Object);

            // Act
            var result = service.GetWorkflowExceptions();

            // Assert
            Console.WriteLine("Captured Logs:");
            foreach (var log in logger.Logs)
            {
                Console.WriteLine($"LogLevel: {log.LogLevel}, Message: {log.Message}");
            }

            Assert.Contains(logger.Logs, log => log.LogLevel == LogLevel.Information && log.Message.Contains("Getting All Records from Database"));
        }

        [Fact]
        public void GetWorkflowExceptions_FirstIterationWithLimitedRecords_LogsRetrievingLimitedRecords()
        {
            // Arrange
            ResetStaticFields(); // Ensure the static field is reset
            var logger = new InMemoryLogger<WorkFlowExceptionService>();
            var optionsMock = new Mock<IOptions<WorkFlowMonitorTableRecordsOptions>>();
            optionsMock.Setup(o => o.Value).Returns(new WorkFlowMonitorTableRecordsOptions { ProcessAllRecords = false, Days = 30 });

            var context = CreateInMemoryContext();
            context.WorkflowExceptions.AddRange(
                new WorkflowException { Id = Guid.NewGuid(), CreateDate = DateTime.Now.AddDays(-1), Type = "Enroute" },
                new WorkflowException { Id = Guid.NewGuid(), CreateDate = DateTime.Now.AddDays(-2), Type = "Clear" },
                new WorkflowException { Id = Guid.NewGuid(), CreateDate = DateTime.Now.AddDays(-3), Type = "OnSite" },
                new WorkflowException { Id = Guid.NewGuid(), CreateDate = DateTime.Now.AddDays(-40), Type = "Enroute" }
            );
            context.SaveChanges();

            var service = new WorkFlowExceptionService(context, logger, optionsMock.Object);

            // Act
            var result = service.GetWorkflowExceptions();

            // Assert
            Assert.Equal(3, result.Count);
            Assert.Contains(logger.Logs, log => log.LogLevel == LogLevel.Information && log.Message.Contains("Getting records from past 30 days"));
        }

        [Fact]
        public void GetWorkflowExceptions_NonFirstIteration_LogsRetrievingNewRecords()
        {
            // Arrange
            ResetStaticFields(); // Ensure the static field is reset
            var logger = new InMemoryLogger<WorkFlowExceptionService>();
            var optionsMock = new Mock<IOptions<WorkFlowMonitorTableRecordsOptions>>();
            optionsMock.Setup(o => o.Value).Returns(new WorkFlowMonitorTableRecordsOptions { ProcessAllRecords = false, Days = 30 });

            var context = CreateInMemoryContext();
            var lastWorkFlowException = new LastWorkFlowException { Id = Guid.NewGuid(), CreateDate = DateTime.Now.AddDays(-2) };
            context.LastWorkFlowExceptions.Add(lastWorkFlowException);
            context.WorkflowExceptions.AddRange(
                new WorkflowException { Id = Guid.NewGuid(), CreateDate = DateTime.Now.AddDays(-1), Type = "Enroute" },
                new WorkflowException { Id = Guid.NewGuid(), CreateDate = DateTime.Now.AddMinutes(-1), Type = "Clear" }
            );
            context.SaveChanges();

            var service = new WorkFlowExceptionService(context, logger, optionsMock.Object);

            // Manually set the first iteration to false
            typeof(WorkFlowExceptionService).GetField("fistIteration", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static).SetValue(null, false);

            // Act
            var result = service.GetWorkflowExceptions();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Contains(logger.Logs, log => log.LogLevel == LogLevel.Information && log.Message.Contains("Now getting records greater than"));
        }

        [Fact]
        public void SaveLastRecord_NewRecord_SavesRecord()
        {
            // Arrange
            ResetStaticFields(); // Ensure the static field is reset
            var logger = new InMemoryLogger<WorkFlowExceptionService>();
            var optionsMock = new Mock<IOptions<WorkFlowMonitorTableRecordsOptions>>();

            var context = CreateInMemoryContext();
            var workflowExceptions = new List<WorkflowException>
        {
            new WorkflowException { Id = Guid.NewGuid(), CreateDate = DateTime.Now, Type = "Enroute" }
        };
            context.WorkflowExceptions.AddRange(workflowExceptions);
            context.SaveChanges();

            var service = new WorkFlowExceptionService(context, logger, optionsMock.Object);
            typeof(WorkFlowExceptionService).GetField("exceptions", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(service, workflowExceptions);

            // Act
            service.GetType().GetMethod("SaveLastRecord", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).Invoke(service, null);

            // Assert
            Assert.Single(context.LastWorkFlowExceptions);
            Assert.Contains(logger.Logs, log => log.LogLevel == LogLevel.Information && log.Message.Contains("Record") && log.Message.Contains("saved in LastWorkFlowException table"));
        }

        [Fact]
        public void SaveLastRecord_RecordAlreadyExists_DoesNotSaveRecord()
        {
            // Arrange
            ResetStaticFields(); // Ensure the static field is reset
            var logger = new InMemoryLogger<WorkFlowExceptionService>();
            var optionsMock = new Mock<IOptions<WorkFlowMonitorTableRecordsOptions>>();

            var context = CreateInMemoryContext();
            var workflowExceptions = new List<WorkflowException>
        {
            new WorkflowException { Id = Guid.NewGuid(), CreateDate = DateTime.Now, Type = "Enroute" }
        };
            var lastWorkFlowException = new LastWorkFlowException { Id = workflowExceptions.First().Id };
            context.WorkflowExceptions.AddRange(workflowExceptions);
            context.LastWorkFlowExceptions.Add(lastWorkFlowException);
            context.SaveChanges();

            var service = new WorkFlowExceptionService(context, logger, optionsMock.Object);
            typeof(WorkFlowExceptionService).GetField("exceptions", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(service, workflowExceptions);

            // Act
            service.GetType().GetMethod("SaveLastRecord", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).Invoke(service, null);

            // Assert
            Assert.Single(context.LastWorkFlowExceptions);
            Assert.Contains(logger.Logs, log => log.LogLevel == LogLevel.Information && log.Message.Contains("already exists in LastWorkFlowException table"));
        }
    }
}
