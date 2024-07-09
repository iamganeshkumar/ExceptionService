using TSOpsExceptionService.Interfaces;
using System.Xml.Serialization;
using WorkFlowMonitorServiceReference;

namespace TSOpsExceptionService.Services
{
    public class Deserialization : IDeserialization
    {
        private readonly ILogger<Deserialization> _logger;

        public Deserialization(ILogger<Deserialization> logger)
        {
            _logger = logger;
        }

        public bool TryDeserializeEnrouteFromXml(string xml, out SetEmployeeToEnRouteRequest? result)
        {
            var xmlSerializer = new XmlSerializer(typeof(SetEmployeeToEnRouteRequest));
            try
            {
                using (var stringReader = new StringReader(xml))
                {
                    result = xmlSerializer.Deserialize(stringReader) as SetEmployeeToEnRouteRequest;
                    return result != null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deserializing Enroute XML: {ex}");
                result = null;
                return false;
            }
        }

        public bool TryDeserializeOnSiteFromXml(string xml, out SetEmployeeToOnSiteRequest? result)
        {
            var xmlSerializer = new XmlSerializer(typeof(SetEmployeeToOnSiteRequest));
            try
            {
                using (var stringReader = new StringReader(xml))
                {
                    result = xmlSerializer.Deserialize(stringReader) as SetEmployeeToOnSiteRequest;
                    return result != null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deserializing OnSite XML: {ex}");
                result = null;
                return false;
            }
        }

        public bool TryDeserializeClearFromXml(string xml, out ClearAppointmentRequestModel? result)
        {
            var xmlSerializer = new XmlSerializer(typeof(ClearAppointmentRequestModel));
            try
            {
                using (var stringReader = new StringReader(xml))
                {
                    result = xmlSerializer.Deserialize(stringReader) as ClearAppointmentRequestModel;
                    return result != null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deserializing Clear XML: {ex}");
                result = null;
                return false;
            }
        }
    }
}
