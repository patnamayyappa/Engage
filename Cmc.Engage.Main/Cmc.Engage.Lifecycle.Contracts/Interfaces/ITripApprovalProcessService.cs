using Cmc.Core.Xrm.ServerExtension.Core;

namespace Cmc.Engage.Lifecycle
{
    /// <summary>
    /// Contract for TripApprovalProcess
    /// </summary>
    public interface ITripApprovalProcessService
    {
        /// <summary>
        /// Call the work flow on move next stage for Business Process Flow
        /// </summary>
        /// <param name="context"></param>
        void TripApprovalProcessExecute(IExecutionContext context);
    }
}
