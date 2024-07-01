using WorkFlowMonitorServiceReference;

namespace ExceptionService.Interfaces
{
    public interface IDeserialization
    {
        bool TryDeserializeEnrouteFromXml(string xml, out SetEmployeeToEnRouteRequest? result);
        bool TryDeserializeOnSiteFromXml(string xml, out SetEmployeeToOnSiteRequest? result);
        bool TryDeserializeClearFromXml(string xml, out ClearAppointmentRequestModel? result);
    }
}
