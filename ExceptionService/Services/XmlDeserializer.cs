using ExceptionService.Interfaces;
using System.Xml.Serialization;

namespace ExceptionService.Services
{
    public class XmlDeserializer : IXmlDeserializer
    {
        public bool TryDeserializeFromXml(string xml, out DeserializedRequest? result)
        {
            var xmlSerializer = new XmlSerializer(typeof(DeserializedRequest));
            try
            {
                using (var stringReader = new StringReader(xml))
                {
                    result = xmlSerializer.Deserialize(stringReader) as DeserializedRequest;
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
