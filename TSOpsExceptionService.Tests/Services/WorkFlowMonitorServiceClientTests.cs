using TSOpsExceptionService.Configuration.Models;
using TSOpsExceptionService.Interfaces;
using TSOpsExceptionService.Requests;
using TSOpsExceptionService.Services;
using TSOpsExceptionService.Tests.Logger;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using WorkFlowMonitorServiceReference;

namespace TSOpsExceptionService.Tests.Services
{
    public class WorkFlowMonitorServiceClientTests
    {
        private Mock<IWorkflowMonitorSoapClient> CreateMockClient()
        {
            return new Mock<IWorkflowMonitorSoapClient>();
        }

        private Mock<IOptions<SoapEndpointOptions>> CreateMockOptions(string url)
        {
            var optionsMock = new Mock<IOptions<SoapEndpointOptions>>();
            optionsMock.Setup(o => o.Value).Returns(new SoapEndpointOptions { WorkflowMonitorServiceBaseUrl = url });
            return optionsMock;
        }

        private InMemoryLogger<WorkFlowMonitorServiceClient> CreateLogger()
        {
            return new InMemoryLogger<WorkFlowMonitorServiceClient>();
        }

        [Fact]
        public async Task ReprocessEnrouteExceptionsAsync_Success_LogsAndReturnsResponse()
        {
            // Arrange
            var logger = CreateLogger();
            var mockClient = CreateMockClient();
            var optionsMock = CreateMockOptions("http://localhost");

            var expectedResponse = new StandardSoapResponseOfboolean { ReturnValue = true };
            mockClient.Setup(client => client.ReprocessEnrouteExceptionsAsync(It.IsAny<WorkflowExceptionModelOfSetEmployeeToEnRouteRequestbwABAbVO>(), It.IsAny<string>()))
                      .ReturnsAsync(expectedResponse);

            var service = new WorkFlowMonitorServiceClient(optionsMock.Object, logger, mockClient.Object);

            var reprocessRequest = new WorkflowExceptionRequest
            {
                Id = Guid.NewGuid(),
                CreateDate = DateTime.Now,
                ErrorInformation = "Error Info",
                IsBusinessError = true,
                JobNumber = 123,
                JobSequenceNumber = 1,
                Type = ExceptionType.Enroute
            };

            // Act
            var response = await service.ReprocessEnrouteExceptionsAsync(reprocessRequest, "testUser");

            // Assert
            Assert.Equal(expectedResponse, response);
            Assert.Contains(logger.Logs, log => log.LogLevel == LogLevel.Information && log.Message.Contains("Request for reprocessing Enroute Exceptions"));
            Assert.Contains(logger.Logs, log => log.LogLevel == LogLevel.Information && log.Message.Contains("Time taken to ReprocessEnrouteException is"));
        }

        [Fact]
        public async Task ReprocessEnrouteExceptionsAsync_ExceptionThrown_LogsErrorAndReturnsNull()
        {
            // Arrange
            var logger = CreateLogger();
            var mockClient = CreateMockClient();
            var optionsMock = CreateMockOptions("http://localhost");

            mockClient.Setup(client => client.ReprocessEnrouteExceptionsAsync(It.IsAny<WorkflowExceptionModelOfSetEmployeeToEnRouteRequestbwABAbVO>(), It.IsAny<string>()))
                      .ThrowsAsync(new Exception("Test Exception"));

            var service = new WorkFlowMonitorServiceClient(optionsMock.Object, logger, mockClient.Object);

            var reprocessRequest = new WorkflowExceptionRequest
            {
                Id = Guid.NewGuid(),
                CreateDate = DateTime.Now,
                ErrorInformation = "Error Info",
                IsBusinessError = true,
                JobNumber = 123,
                JobSequenceNumber = 1,
                Type = ExceptionType.Enroute
            };

            // Act
            var response = await service.ReprocessEnrouteExceptionsAsync(reprocessRequest, "testUser");

            // Assert
            Assert.Null(response);
            Assert.Contains(logger.Logs, log => log.LogLevel == LogLevel.Error && log.Message.Contains("Error in ReprocessEnrouteExceptionsAsync() method in Services"));
            Assert.Contains(logger.Logs, log => log.LogLevel == LogLevel.Error && log.Message.Contains("Detailed Error - Test Exception"));
        }

        [Fact]
        public async Task ReprocessOnSiteExceptionsAsync_Success_LogsAndReturnsResponse()
        {
            // Arrange
            var logger = CreateLogger();
            var mockClient = CreateMockClient();
            var optionsMock = CreateMockOptions("http://localhost");

            var expectedResponse = new StandardSoapResponseOfboolean { ReturnValue = true };
            mockClient.Setup(client => client.ReprocessOnSiteExceptionsAsync(It.IsAny<WorkflowExceptionModelOfSetEmployeeToOnSiteRequestbwABAbVO>(), It.IsAny<string>()))
                      .ReturnsAsync(expectedResponse);

            var service = new WorkFlowMonitorServiceClient(optionsMock.Object, logger, mockClient.Object);

            var reprocessRequest = new WorkflowExceptionRequest
            {
                Id = Guid.NewGuid(),
                CreateDate = DateTime.Now,
                ErrorInformation = "Error Info",
                IsBusinessError = true,
                JobNumber = 123,
                JobSequenceNumber = 1,
                Type = ExceptionType.OnSite
            };

            // Act
            var response = await service.ReprocessOnSiteExceptionsAsync(reprocessRequest, "testUser");

            // Assert
            Assert.Equal(expectedResponse, response);
            Assert.Contains(logger.Logs, log => log.LogLevel == LogLevel.Information && log.Message.Contains("Request for reprocessing OnSite Exceptions"));
            Assert.Contains(logger.Logs, log => log.LogLevel == LogLevel.Information && log.Message.Contains("Time taken to ReprocessOnSiteException is"));
        }

        [Fact]
        public async Task ReprocessOnSiteExceptionsAsync_ExceptionThrown_LogsErrorAndReturnsNull()
        {
            // Arrange
            var logger = CreateLogger();
            var mockClient = CreateMockClient();
            var optionsMock = CreateMockOptions("http://localhost");

            mockClient.Setup(client => client.ReprocessOnSiteExceptionsAsync(It.IsAny<WorkflowExceptionModelOfSetEmployeeToOnSiteRequestbwABAbVO>(), It.IsAny<string>()))
                      .ThrowsAsync(new Exception("Test Exception"));

            var service = new WorkFlowMonitorServiceClient(optionsMock.Object, logger, mockClient.Object);

            var reprocessRequest = new WorkflowExceptionRequest
            {
                Id = Guid.NewGuid(),
                CreateDate = DateTime.Now,
                ErrorInformation = "Error Info",
                IsBusinessError = true,
                JobNumber = 123,
                JobSequenceNumber = 1,
                Type = ExceptionType.OnSite
            };

            // Act
            var response = await service.ReprocessOnSiteExceptionsAsync(reprocessRequest, "testUser");

            // Assert
            Assert.Null(response);
            Assert.Contains(logger.Logs, log => log.LogLevel == LogLevel.Error && log.Message.Contains("Error in ReprocessOnSiteExceptionsAsync() method in Services"));
            Assert.Contains(logger.Logs, log => log.LogLevel == LogLevel.Error && log.Message.Contains("Detailed Error - Test Exception"));
        }

        [Fact]
        public async Task ReprocessClearAppointmentExceptionsAsync_Success_LogsAndReturnsResponse()
        {
            // Arrange
            var logger = CreateLogger();
            var mockClient = CreateMockClient();
            var optionsMock = CreateMockOptions("http://localhost");

            var expectedResponse = new StandardSoapResponseOfboolean { ReturnValue = true };
            mockClient.Setup(client => client.ReprocessClearAppointmentExceptionsAsync(It.IsAny<WorkflowExceptionModelOfClearAppointmentRequestModelT2o2hOfe>(), It.IsAny<string>()))
                      .ReturnsAsync(expectedResponse);

            var service = new WorkFlowMonitorServiceClient(optionsMock.Object, logger, mockClient.Object);

            var reprocessRequest = new WorkflowExceptionRequest
            {
                Id = Guid.NewGuid(),
                CreateDate = DateTime.Now,
                ErrorInformation = "Error Info",
                IsBusinessError = true,
                JobNumber = 123,
                JobSequenceNumber = 1,
                Type = ExceptionType.Clear
            };

            // Act
            var response = await service.ReprocessClearAppointmentExceptionsAsync(reprocessRequest, "testUser");

            // Assert
            Assert.Equal(expectedResponse, response);
            Assert.Contains(logger.Logs, log => log.LogLevel == LogLevel.Information && log.Message.Contains("Request for reprocessing Clear Exceptions"));
            Assert.Contains(logger.Logs, log => log.LogLevel == LogLevel.Information && log.Message.Contains("Time taken to ReprocessClearAppointmentException is"));
        }

        [Fact]
        public async Task ReprocessClearAppointmentExceptionsAsync_ExceptionThrown_LogsErrorAndReturnsNull()
        {
            // Arrange
            var logger = CreateLogger();
            var mockClient = CreateMockClient();
            var optionsMock = CreateMockOptions("http://localhost");

            mockClient.Setup(client => client.ReprocessClearAppointmentExceptionsAsync(It.IsAny<WorkflowExceptionModelOfClearAppointmentRequestModelT2o2hOfe>(), It.IsAny<string>()))
                      .ThrowsAsync(new Exception("Test Exception"));

            var service = new WorkFlowMonitorServiceClient(optionsMock.Object, logger, mockClient.Object);

            var reprocessRequest = new WorkflowExceptionRequest
            {
                Id = Guid.NewGuid(),
                CreateDate = DateTime.Now,
                ErrorInformation = "Error Info",
                IsBusinessError = true,
                JobNumber = 123,
                JobSequenceNumber = 1,
                Type = ExceptionType.Clear
            };

            // Act
            var response = await service.ReprocessClearAppointmentExceptionsAsync(reprocessRequest, "testUser");

            // Assert
            Assert.Null(response);
            Assert.Contains(logger.Logs, log => log.LogLevel == LogLevel.Error && log.Message.Contains("Error in ReprocessClearAppointmentExceptionsAsync() method in Services"));
            Assert.Contains(logger.Logs, log => log.LogLevel == LogLevel.Error && log.Message.Contains("Detailed Error - Test Exception"));
        }
    }
}
