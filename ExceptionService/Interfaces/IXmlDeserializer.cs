namespace ExceptionService.Interfaces
{
    public interface IXmlDeserializer
    {
        bool TryDeserializeEnrouteFromXml(string xml, out SetEmployeeToEnRouteRequest? result);
        bool TryDeserializeOnSiteFromXml(string xml, out SetEmployeeToOnSiteRequest? result);
        bool TryDeserializeClearFromXml(string xml, out ClearAppointmentRequest? result);
    }
}
