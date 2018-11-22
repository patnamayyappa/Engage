using Cmc.Core.Xrm.ServerExtension.Core;

namespace Cmc.Engage.Common
{
    public interface IMapService
    {
        LatitudeLongitude GetCoordinatesFromAddress(string address);
        double? GetDistanceInMiles(double sourceLatitude, double sourceLongitude, double destinationLatitude,
            double destinationLongitude, IExecutionContext context);
    }
}
