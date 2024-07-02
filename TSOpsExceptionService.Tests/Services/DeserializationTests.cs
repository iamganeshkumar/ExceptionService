using ExceptionService.Services;
using Microsoft.Extensions.Logging;
using Moq;
using WorkFlowMonitorServiceReference;

namespace ExceptionService.Tests.Services
{
    public class DeserializationTests
    {
        [Fact]
        public void TryDeserializeEnrouteFromXml_ValidXml_ReturnsTrue()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<Deserialization>>();
            var deserialization = new Deserialization(loggerMock.Object);
            string validXml = "<SetEmployeeToEnRouteRequest><adUserName>testuser</adUserName></SetEmployeeToEnRouteRequest>";

            // Act
            var result = deserialization.TryDeserializeEnrouteFromXml(validXml, out SetEmployeeToEnRouteRequest? deserializedRequest);

            // Assert
            Assert.True(result);
            Assert.NotNull(deserializedRequest);
            Assert.Equal("testuser", deserializedRequest.adUserName);
        }

        [Fact]
        public void TryDeserializeEnrouteFromXml_InvalidXml_ReturnsFalse()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<Deserialization>>();
            var deserialization = new Deserialization(loggerMock.Object);
            string invalidXml = "<InvalidXml>";

            // Act
            var result = deserialization.TryDeserializeEnrouteFromXml(invalidXml, out SetEmployeeToEnRouteRequest? deserializedRequest);

            // Assert
            Assert.False(result);
            Assert.Null(deserializedRequest);
            loggerMock.Verify(
                log => log.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error deserializing Enroute XML")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), Times.Once);
        }

        [Fact]
        public void TryDeserializeOnSiteFromXml_ValidXml_ReturnsTrue()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<Deserialization>>();
            var deserialization = new Deserialization(loggerMock.Object);
            string validXml = "<SetEmployeeToOnSiteRequest><adUserName>testuser</adUserName></SetEmployeeToOnSiteRequest>";

            // Act
            var result = deserialization.TryDeserializeOnSiteFromXml(validXml, out SetEmployeeToOnSiteRequest? deserializedRequest);

            // Assert
            Assert.True(result);
            Assert.NotNull(deserializedRequest);
            Assert.Equal("testuser", deserializedRequest.adUserName);
        }

        [Fact]
        public void TryDeserializeOnSiteFromXml_InvalidXml_ReturnsFalse()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<Deserialization>>();
            var deserialization = new Deserialization(loggerMock.Object);
            string invalidXml = "<InvalidXml>";

            // Act
            var result = deserialization.TryDeserializeOnSiteFromXml(invalidXml, out SetEmployeeToOnSiteRequest? deserializedRequest);

            // Assert
            Assert.False(result);
            Assert.Null(deserializedRequest);
            loggerMock.Verify(
                log => log.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error deserializing OnSite XML")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), Times.Once);
        }

        [Fact]
        public void TryDeserializeClearFromXml_ValidXml_ReturnsTrue()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<Deserialization>>();
            var deserialization = new Deserialization(loggerMock.Object);
            string validXml = "<ClearAppointmentRequestModel><adUserName>testuser</adUserName></ClearAppointmentRequestModel>";

            // Act
            var result = deserialization.TryDeserializeClearFromXml(validXml, out ClearAppointmentRequestModel? deserializedRequest);

            // Assert
            Assert.True(result);
            Assert.NotNull(deserializedRequest);
            Assert.Equal("testuser", deserializedRequest.adUserName);
        }

        [Fact]
        public void TryDeserializeClearFromXml_InvalidXml_ReturnsFalse()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<Deserialization>>();
            var deserialization = new Deserialization(loggerMock.Object);
            string invalidXml = "<InvalidXml>";

            // Act
            var result = deserialization.TryDeserializeClearFromXml(invalidXml, out ClearAppointmentRequestModel? deserializedRequest);

            // Assert
            Assert.False(result);
            Assert.Null(deserializedRequest);
            loggerMock.Verify(
                log => log.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error deserializing Clear XML")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), Times.Once);
        }
    }
}
