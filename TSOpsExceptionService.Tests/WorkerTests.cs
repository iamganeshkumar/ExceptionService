using TSOpsExceptionService.Common;
using TSOpsExceptionService.Configuration.Models;
using TSOpsExceptionService.Interfaces;
using TSOpsExceptionService.Models;
using TSOpsExceptionService.Requests;
using ExceptionServiceReference;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using WorkFlowMonitorServiceReference;

namespace TSOpsExceptionService.Tests
{
    public class WorkerTests
    {
        [Fact]
        public async Task ExecuteAsync_NoExceptions_LogsNoExceptions()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<Worker>>();
            var exceptionServiceMock = new Mock<IWorkFlowExceptionService>();
            var jobServiceClientMock = new Mock<IJobServiceClient>();
            var workflowMonitorServiceClientMock = new Mock<IWorkflowMonitorServiceClient>();
            var durationOptionsMock = new Mock<IOptions<DurationOptions>>();
            var deserializationMock = new Mock<IDeserialization>();

            exceptionServiceMock.Setup(es => es.GetWorkflowExceptions()).Returns(new List<WorkflowException>());
            durationOptionsMock.Setup(o => o.Value).Returns(new DurationOptions { TimeIntervalInMinutes = 1 });

            var worker = new Worker(loggerMock.Object, exceptionServiceMock.Object, jobServiceClientMock.Object,
                workflowMonitorServiceClientMock.Object, durationOptionsMock.Object, deserializationMock.Object);
            var stoppingToken = new CancellationTokenSource();

            // Act
            await worker.StartAsync(stoppingToken.Token);
            stoppingToken.Cancel(); // Stop the worker

            // Assert
            loggerMock.Verify(log => log.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("No Exceptions")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_WithExceptions_ProcessesExceptions()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<Worker>>();
            var exceptionServiceMock = new Mock<IWorkFlowExceptionService>();
            var jobServiceClientMock = new Mock<IJobServiceClient>();
            var workflowMonitorServiceClientMock = new Mock<IWorkflowMonitorServiceClient>();
            var durationOptionsMock = new Mock<IOptions<DurationOptions>>();
            var deserializationMock = new Mock<IDeserialization>();
            SetEmployeeToEnRouteRequest setEmployeeToEnRouteRequest = new SetEmployeeToEnRouteRequest()
            { };

            var exceptions = new List<WorkflowException>
        {
            new WorkflowException
            {
                Id = Guid.NewGuid(),
                CreateDate = DateTime.Now,
                ErrorInfo = "Test Error",
                IsBusinessError = false,
                JobNumber = 123,
                JobSeqNumber = 1,
                Type = "Enroute",
                Data = "abc"
            }
        };

            exceptionServiceMock.Setup(es => es.GetWorkflowExceptions()).Returns(exceptions);
            durationOptionsMock.Setup(o => o.Value).Returns(new DurationOptions { TimeIntervalInMinutes = 1 });
            jobServiceClientMock.Setup(js => js.GetJobAsync(It.IsAny<long>())).ReturnsAsync(new Job { JOBTYPE_ID = Constants.INSTALL });
            deserializationMock.Setup(d => d.TryDeserializeEnrouteFromXml(It.IsAny<string>(), out setEmployeeToEnRouteRequest)).Returns(true);

            workflowMonitorServiceClientMock.Setup(wm => wm.ReprocessEnrouteExceptionsAsync(It.IsAny<WorkflowExceptionRequest>(), It.IsAny<string>()))
                .ReturnsAsync(new StandardSoapResponseOfboolean { ReturnValue = true });

            var worker = new Worker(loggerMock.Object, exceptionServiceMock.Object, jobServiceClientMock.Object,
                workflowMonitorServiceClientMock.Object, durationOptionsMock.Object, deserializationMock.Object);
            var stoppingToken = new CancellationTokenSource();

            // Act
            await worker.StartAsync(stoppingToken.Token);
            stoppingToken.Cancel(); // Stop the worker

            // Assert
            workflowMonitorServiceClientMock.Verify(wm => wm.ReprocessEnrouteExceptionsAsync(It.IsAny<WorkflowExceptionRequest>(), It.IsAny<string>()), Times.Once);
            loggerMock.Verify(log => log.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Successfully retrieved job for job number")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_WithExceptionAndJobNull_LogsJobRetrievalUnsuccessful()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<Worker>>();
            var exceptionServiceMock = new Mock<IWorkFlowExceptionService>();
            var jobServiceClientMock = new Mock<IJobServiceClient>();
            var workflowMonitorServiceClientMock = new Mock<IWorkflowMonitorServiceClient>();
            var durationOptionsMock = new Mock<IOptions<DurationOptions>>();
            var deserializationMock = new Mock<IDeserialization>();

            var exceptions = new List<WorkflowException>
        {
            new WorkflowException
            {
                Id = Guid.NewGuid(),
                CreateDate = DateTime.Now,
                ErrorInfo = "Test Error",
                IsBusinessError = false,
                JobNumber = 123,
                JobSeqNumber = 1,
                Type = "Enroute"
            }
        };

            exceptionServiceMock.Setup(es => es.GetWorkflowExceptions()).Returns(exceptions);
            durationOptionsMock.Setup(o => o.Value).Returns(new DurationOptions { TimeIntervalInMinutes = 1 });
            jobServiceClientMock.Setup(js => js.GetJobAsync(It.IsAny<long>())).ReturnsAsync((Job)null);

            var worker = new Worker(loggerMock.Object, exceptionServiceMock.Object, jobServiceClientMock.Object,
                workflowMonitorServiceClientMock.Object, durationOptionsMock.Object, deserializationMock.Object);
            var stoppingToken = new CancellationTokenSource();

            // Act
            await worker.StartAsync(stoppingToken.Token);
            stoppingToken.Cancel(); // Stop the worker

            // Assert
            loggerMock.Verify(log => log.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Job retrieval was unsuccessfull for job id")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), Times.Once);
        }
    }
}
