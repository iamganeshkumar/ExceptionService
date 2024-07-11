using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using TSOpsExceptionService.Configuration.Models;
using TSOpsExceptionService.Models;
using TSOpsExceptionService.Services;

namespace TSOpsExceptionService.Tests.Services
{
    public class WorkFlowExceptionServiceTests : IClassFixture<InMemoryDbContextFixture>
    {
        private readonly InMemoryDbContextFixture _fixture;
        private readonly InMemoryLogger<WorkFlowExceptionService> _logger;
        private readonly Mock<IOptions<WorkFlowMonitorTableRecordsOptions>> _optionsMock;

        public WorkFlowExceptionServiceTests(InMemoryDbContextFixture fixture)
        {
            _fixture = fixture;
            _logger = new InMemoryLogger<WorkFlowExceptionService>();
            _optionsMock = new Mock<IOptions<WorkFlowMonitorTableRecordsOptions>>();
            _optionsMock.Setup(o => o.Value).Returns(new WorkFlowMonitorTableRecordsOptions { ProcessAllRecords = true, Days = 30 });
        }

        private void ResetStaticFields()
        {
            typeof(WorkFlowExceptionService)
                .GetField("fistIteration", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
                .SetValue(null, true);
        }

        private void ClearDatabase()
        {
            _fixture.Context.WorkflowExceptions.RemoveRange(_fixture.Context.WorkflowExceptions);
            _fixture.Context.ReprocessedExceptions.RemoveRange(_fixture.Context.ReprocessedExceptions);
            _fixture.Context.LastWorkFlowExceptions.RemoveRange(_fixture.Context.LastWorkFlowExceptions);
            _fixture.Context.SaveChanges();
        }

        [Fact]
        public void GetWorkflowExceptions_FirstIterationWithAllRecords_LogsRetrievingAllRecords()
        {
            // Arrange
            ResetStaticFields();
            ClearDatabase();

            _fixture.Context.WorkflowExceptions.AddRange(
                new WorkflowException { Id = Guid.NewGuid(), CreateDate = DateTime.Now.AddDays(-1), Type = "Enroute" },
                new WorkflowException { Id = Guid.NewGuid(), CreateDate = DateTime.Now.AddDays(-2), Type = "Clear" },
                new WorkflowException { Id = Guid.NewGuid(), CreateDate = DateTime.Now.AddDays(-3), Type = "OnSite" }
            );
            _fixture.Context.SaveChanges();

            var service = new WorkFlowExceptionService(_fixture.Context, _logger, _optionsMock.Object);

            // Act
            var result = service.GetWorkflowExceptions();

            // Assert
            Assert.Equal(3, result.Count);
            Assert.Contains(_logger.Logs, log => log.LogLevel == LogLevel.Information && log.Message.Contains("Getting All Records from Database"));
        }

        [Fact]
        public void GetWorkflowExceptions_FirstIterationWithLimitedRecords_LogsRetrievingLimitedRecords()
        {
            // Arrange
            ResetStaticFields();
            ClearDatabase();
            _optionsMock.Setup(o => o.Value).Returns(new WorkFlowMonitorTableRecordsOptions { ProcessAllRecords = false, Days = 30 });

            _fixture.Context.WorkflowExceptions.AddRange(
                new WorkflowException { Id = Guid.NewGuid(), CreateDate = DateTime.Now.AddDays(-1), Type = "Enroute" },
                new WorkflowException { Id = Guid.NewGuid(), CreateDate = DateTime.Now.AddDays(-2), Type = "Clear" },
                new WorkflowException { Id = Guid.NewGuid(), CreateDate = DateTime.Now.AddDays(-3), Type = "OnSite" },
                new WorkflowException { Id = Guid.NewGuid(), CreateDate = DateTime.Now.AddDays(-40), Type = "Enroute" }
            );
            _fixture.Context.SaveChanges();

            var service = new WorkFlowExceptionService(_fixture.Context, _logger, _optionsMock.Object);

            // Act
            var result = service.GetWorkflowExceptions();

            // Assert
            Assert.Equal(3, result.Count); // Only records from the last 30 days should be retrieved
            Assert.Contains(_logger.Logs, log => log.LogLevel == LogLevel.Information && log.Message.Contains("Getting records from past 30 days"));
        }

        [Fact]
        public void GetWorkflowExceptions_NonFirstIteration_LogsRetrievingNewRecords()
        {
            // Arrange
            ResetStaticFields();
            ClearDatabase();
            _optionsMock.Setup(o => o.Value).Returns(new WorkFlowMonitorTableRecordsOptions { ProcessAllRecords = false, Days = 30 });

            var lastWorkFlowException = new LastWorkFlowException { Id = Guid.NewGuid(), CreateDate = DateTime.Now.AddDays(-2) };
            _fixture.Context.LastWorkFlowExceptions.Add(lastWorkFlowException);
            _fixture.Context.WorkflowExceptions.AddRange(
                new WorkflowException { Id = Guid.NewGuid(), CreateDate = DateTime.Now.AddDays(-1), Type = "Enroute" },
                new WorkflowException { Id = Guid.NewGuid(), CreateDate = DateTime.Now.AddMinutes(-1), Type = "Clear" }
            );
            _fixture.Context.SaveChanges();

            var service = new WorkFlowExceptionService(_fixture.Context, _logger, _optionsMock.Object);

            // Manually set the first iteration to false
            typeof(WorkFlowExceptionService).GetField("fistIteration", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static).SetValue(null, false);

            // Act
            var result = service.GetWorkflowExceptions();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Contains(_logger.Logs, log => log.LogLevel == LogLevel.Information && log.Message.Contains("Now getting records greater than"));
        }

        [Fact]
        public void SaveLastRecord_NewRecord_SavesRecord()
        {
            // Arrange
            ResetStaticFields();
            ClearDatabase();

            var workflowExceptions = new List<WorkflowException>
            {
                new WorkflowException { Id = Guid.NewGuid(), CreateDate = DateTime.Now, Type = "Enroute" }
            };
            _fixture.Context.WorkflowExceptions.AddRange(workflowExceptions);
            _fixture.Context.SaveChanges();

            var service = new WorkFlowExceptionService(_fixture.Context, _logger, _optionsMock.Object);
            typeof(WorkFlowExceptionService).GetField("exceptions", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(service, workflowExceptions);

            // Act
            service.GetType().GetMethod("SaveLastRecord", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).Invoke(service, null);

            // Assert
            Assert.Single(_fixture.Context.LastWorkFlowExceptions);
            Assert.Contains(_logger.Logs, log => log.LogLevel == LogLevel.Information && log.Message.Contains("Record") && log.Message.Contains("saved in LastWorkFlowException table"));
        }

        [Fact]
        public void SaveLastRecord_RecordAlreadyExists_DoesNotSaveRecord()
        {
            // Arrange
            ResetStaticFields();
            ClearDatabase();

            var workflowExceptions = new List<WorkflowException>
            {
                new WorkflowException { Id = Guid.NewGuid(), CreateDate = DateTime.Now, Type = "Enroute" }
            };
            var lastWorkFlowException = new LastWorkFlowException { Id = workflowExceptions.First().Id };
            _fixture.Context.WorkflowExceptions.AddRange(workflowExceptions);
            _fixture.Context.LastWorkFlowExceptions.Add(lastWorkFlowException);
            _fixture.Context.SaveChanges();

            var service = new WorkFlowExceptionService(_fixture.Context, _logger, _optionsMock.Object);
            typeof(WorkFlowExceptionService).GetField("exceptions", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(service, workflowExceptions);

            // Act
            service.GetType().GetMethod("SaveLastRecord", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).Invoke(service, null);

            // Assert
            Assert.Single(_fixture.Context.LastWorkFlowExceptions);
            Assert.Contains(_logger.Logs, log => log.LogLevel == LogLevel.Information && log.Message.Contains("already exists in LastWorkFlowException table"));
        }
    }
}
