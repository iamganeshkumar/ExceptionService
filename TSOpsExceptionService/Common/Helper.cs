using WorkFlowMonitorServiceReference;

namespace TSOpsExceptionService.Common
{
    public static class Helper
    {
        public static ExceptionType MapServiceExceptionTypeToExceptionType(string serviceExceptionType)
        {
            return serviceExceptionType switch
            {
                nameof(ExceptionType.Enroute) => ExceptionType.Enroute,
                nameof(ExceptionType.OnSite) => ExceptionType.OnSite,
                nameof(ExceptionType.Clear) => ExceptionType.Clear,
                _ => throw new ArgumentOutOfRangeException(nameof(serviceExceptionType), $"Not expected exception type value: {serviceExceptionType}")
            };
        }
    }
}
