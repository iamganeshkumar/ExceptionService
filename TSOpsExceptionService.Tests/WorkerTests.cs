using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using TSOpsExceptionService.Common;
using TSOpsExceptionService.Configuration.Models;
using TSOpsExceptionService.Interfaces;
using TSOpsExceptionService.Requests;
using WorkFlowMonitorServiceReference;
using ExceptionServiceReference;
using TSOpsExceptionService.Models;

namespace TSOpsExceptionService.Tests
{
    public class WorkerTests
    {
        private readonly Mock<IWorkFlowExceptionService> _mockExceptionService;
        private readonly Mock<IJobServiceClient> _mockJobServiceClient;
        private readonly Mock<IWorkflowMonitorServiceClient> _mockWorkflowMonitorServiceClient;
        private readonly Mock<IDeserialization> _mockDeserialization;
        private readonly Mock<IOptions<DurationOptions>> _mockDurationOptions;
        private readonly Mock<IOptions<Retry>> _mockRetryOptions;
        private readonly Mock<ILogger<Worker>> _mockLogger;
        private readonly Worker _worker;

        public WorkerTests()
        {
            _mockExceptionService = new Mock<IWorkFlowExceptionService>();
            _mockJobServiceClient = new Mock<IJobServiceClient>();
            _mockWorkflowMonitorServiceClient = new Mock<IWorkflowMonitorServiceClient>();
            _mockDeserialization = new Mock<IDeserialization>();
            _mockDurationOptions = new Mock<IOptions<DurationOptions>>();
            _mockRetryOptions = new Mock<IOptions<Retry>>();
            _mockLogger = new Mock<ILogger<Worker>>();

            _mockDurationOptions.Setup(o => o.Value).Returns(new DurationOptions { TimeIntervalInMinutes = 1 });
            _mockRetryOptions.Setup(o => o.Value).Returns(new Retry { Attempts = 3 });

            _worker = new Worker(
                _mockLogger.Object,
                _mockExceptionService.Object,
                _mockJobServiceClient.Object,
                _mockWorkflowMonitorServiceClient.Object,
                _mockDurationOptions.Object,
                _mockDeserialization.Object,
                _mockRetryOptions.Object
            );
        }

        [Fact]
        public async Task ProcessWorkflowExceptions_HandlesExceptions()
        {
            // Arrange
            var exceptions = new List<WorkflowException>
            {
                new WorkflowException
                {
                    Id = Guid.NewGuid(),
                    JobNumber = 123,
                    JobSeqNumber = 1,
                    Type = "Enroute",
                    Data = "<xml></xml>"
                }
            };

            _mockExceptionService.Setup(s => s.GetWorkflowExceptions()).Returns(exceptions);
            _mockJobServiceClient.Setup(s => s.GetJobAsync(It.IsAny<long>())).ReturnsAsync(new Job { JOBTYPE_ID = Constants.INSTALL });
            _mockDeserialization.Setup(d => d.TryDeserializeEnrouteFromXml(It.IsAny<string>(), out It.Ref<SetEmployeeToEnRouteRequest>.IsAny))
                .Returns((string xml, out SetEmployeeToEnRouteRequest req) =>
                {
                    req = new SetEmployeeToEnRouteRequest { adUserName = "testUser" };
                    return true;
                });
            _mockWorkflowMonitorServiceClient.Setup(s => s.ReprocessEnrouteExceptionsAsync(It.IsAny<WorkflowExceptionRequest>(), It.IsAny<string>()))
                .ReturnsAsync(new StandardSoapResponseOfboolean { ReturnValue = true });

            // Act
            await _worker.ProcessWorkflowExceptions();

            // Assert

            _mockLogger.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("ReprocessEnrouteExceptionsAsync is successfull")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
        }

        [Fact]
        public async Task ProcessWorkflowExceptions_HandlesReprocessFailures()
        {
            // Arrange
            var exceptions = new List<WorkflowException>
            {
                new WorkflowException
                {
                    Id = Guid.NewGuid(),
                    JobNumber = 123,
                    JobSeqNumber = 1,
                    Type = "Enroute",
                    Data = "<xml></xml>"
                }
            };

            _mockExceptionService.Setup(s => s.GetWorkflowExceptions()).Returns(exceptions);
            _mockJobServiceClient.Setup(s => s.GetJobAsync(It.IsAny<long>())).ReturnsAsync(new Job { JOBTYPE_ID = Constants.INSTALL });
            _mockDeserialization.Setup(d => d.TryDeserializeEnrouteFromXml(It.IsAny<string>(), out It.Ref<SetEmployeeToEnRouteRequest>.IsAny))
                .Returns((string xml, out SetEmployeeToEnRouteRequest req) =>
                {
                    req = new SetEmployeeToEnRouteRequest { adUserName = "testUser" };
                    return true;
                });
            _mockWorkflowMonitorServiceClient.Setup(s => s.ReprocessEnrouteExceptionsAsync(It.IsAny<WorkflowExceptionRequest>(), It.IsAny<string>()))
                .ReturnsAsync((StandardSoapResponseOfboolean)null);

            // Act
            await _worker.ProcessWorkflowExceptions();

            // Assert
            _mockLogger.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Retrying RetryReprocessEnrouteExceptionsAsync")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.AtLeastOnce);

            _mockLogger.Verify(l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("ReprocessEnrouteExceptionsAsync is unsuccessfull")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
        }
    }
}
