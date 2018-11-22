using Microsoft.Xrm.Sdk;
using IExecutionContext = Cmc.Core.Xrm.ServerExtension.Core.IExecutionContext;

namespace Cmc.Engage.Common
{
    public interface IBingMapService
    {
        LatitudeLongitude GetCoordinatesFromAddress(string address);
        double? GetDistanceInMiles(double sourceLatitude, double sourceLongitude, double destinationLatitude, double destinationLongitude, IExecutionContext context);
    }
}
