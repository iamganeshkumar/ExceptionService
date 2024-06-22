using ExceptionService.Interfaces;
using System.Xml.Serialization;

namespace ExceptionService.Services
{
    public class XmlDeserializer : IXmlDeserializer
    {
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
            catch
            {
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
            catch
            {
                result = null;
                return false;
            }
        }

        public bool TryDeserializeClearFromXml(string xml, out ClearAppointmentRequest? result)
        {
            var xmlSerializer = new XmlSerializer(typeof(ClearAppointmentRequest));
            try
            {
                using (var stringReader = new StringReader(xml))
                {
                    result = xmlSerializer.Deserialize(stringReader) as ClearAppointmentRequest;
                    return result != null;
                }
            }
            catch
            {
                result = null;
                return false;
            }
        }
    }
}
