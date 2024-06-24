namespace ExceptionService.Interfaces
{
    public interface IXmlDeserializer
    {
        bool TryDeserializeFromXml(string xml, out DeserializedRequest? result);
    }
}
