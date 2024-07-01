using ExceptionService.Configuration.Models;
using ExceptionService.Services;
using ExceptionServiceReference;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace ExceptionService.Tests.Services
{
    public class JobServiceClientTests
    {
        [Fact]
        public async Task GetJobAsync_ValidJobNumber_ReturnsJob()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<JobServiceClient>>();
            var endpointOptionsMock = new Mock<IOptions<SoapEndpointOptions>>();
            var soapClientMock = new Mock<IJobServiceSoapClient>();

            var job = new Job { JOB_NO = 123 };

            soapClientMock.Setup(client => client.GetJobAsync(It.IsAny<long>()))
                          .ReturnsAsync(job);

            endpointOptionsMock.Setup(o => o.Value)
                               .Returns(new SoapEndpointOptions { JobServiceBaseUrl = "http://test" });

            var jobServiceClient = new JobServiceClient(endpointOptionsMock.Object, loggerMock.Object, soapClientMock.Object);

            // Act
            var result = await jobServiceClient.GetJobAsync(123);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(123, result.JOB_NO);
            soapClientMock.Verify(client => client.GetJobAsync(It.IsAny<long>()), Times.Once);
            loggerMock.Verify(log => log.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Time taken to GetJob type is")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), Times.Once);
        }

        [Fact]
        public async Task GetJobAsync_ExceptionThrown_LogsErrorAndReturnsNull()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<JobServiceClient>>();
            var endpointOptionsMock = new Mock<IOptions<SoapEndpointOptions>>();
            var soapClientMock = new Mock<IJobServiceSoapClient>();

            soapClientMock.Setup(client => client.GetJobAsync(It.IsAny<long>()))
                          .ThrowsAsync(new Exception("Test exception"));

            endpointOptionsMock.Setup(o => o.Value)
                               .Returns(new SoapEndpointOptions { JobServiceBaseUrl = "http://test" });

            var jobServiceClient = new JobServiceClient(endpointOptionsMock.Object, loggerMock.Object, soapClientMock.Object);

            // Act
            var result = await jobServiceClient.GetJobAsync(123);

            // Assert
            Assert.Null(result);
            soapClientMock.Verify(client => client.GetJobAsync(It.IsAny<long>()), Times.Once);
            loggerMock.Verify(log => log.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error in GetJobAsync() method in Services")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), Times.Once);
            loggerMock.Verify(log => log.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Detailed Error - Test exception")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), Times.Once);
        }
    }
}
