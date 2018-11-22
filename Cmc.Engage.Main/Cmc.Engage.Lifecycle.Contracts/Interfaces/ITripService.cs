using Cmc.Core.Xrm.ServerExtension.Core;

namespace Cmc.Engage.Lifecycle
{
    /// <summary>
    /// 
    /// </summary>
    public interface ITripService
    {
        /// <summary>
        /// Create and Update the Trip
        /// </summary>
        /// <param name="context"></param>
        void CreateUpdateTripService(IExecutionContext context);

        /// <summary>
        /// Completes or Cancel the Trip
        /// </summary>
        /// <param name="context"></param>
        void CompleteOrCancelTrip(IExecutionContext context);
    }
}
