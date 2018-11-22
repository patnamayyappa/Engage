using Cmc.Core.Xrm.ServerExtension.Core;

namespace Cmc.Engage.Marketing
{
    /// <summary>
    /// contract for event service
    /// </summary>
    public interface IEventService
    {
        /// <summary>
        /// To update trip activity details for associated event.
        /// </summary>
        /// <param name="context"></param>
        void UpdateTripActivityEventDetails(IExecutionContext context);
        /// <summary>
        /// On Update of msevtmgt_EventStartDate,msevtmgt_EventEndDate,msevtmgt_EventTimeZone of event this method will update cmc_startdatetime,cmc_enddatetime of event.
        /// </summary>
        /// <param name="context"></param>
        void UpdateEventDetails(IExecutionContext context);
    }
}
