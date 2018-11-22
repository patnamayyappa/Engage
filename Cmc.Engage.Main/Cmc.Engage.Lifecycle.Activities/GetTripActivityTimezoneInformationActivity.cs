using Autofac;
using Cmc.Core.Xrm.ServerExtension.Core;
using Cmc.Engage.Models;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;
using System.Activities;

namespace Cmc.Engage.Lifecycle.Activities
{
    /// <summary>
    /// to get the trip activity related timezone information.
    /// </summary>
    public class GetTripActivityTimezoneInformationActivity : ActivityBase
    {
        protected override void Execute(IActivityExecutionContext executionContext)
        {
            var tripActivityService = executionContext.IocScope.Resolve<ITripActivityService>();
            var tracer = executionContext.LoggerFactory.GetLogger(this.GetType());
            tracer.Trace("Reading In Arguments.");
            var tripActivityId = TripActivityId.Get(executionContext.ActivityContext);
            tracer.Trace("TripActivityId is " + tripActivityId.Id);
            var userId = executionContext?.ActivityContext?.GetExtension<IWorkflowContext>()?.UserId;
            tracer.Trace("User Id is " + userId);
            var timezone = tripActivityService.GetTripActivityTimezoneInformationActivity(tripActivityId, userId);
            TimezoneInformation.Set(executionContext.ActivityContext, timezone);
        }

        /// <summary>
        /// The Trip Activity that is associated with volunteers/staff members.
        /// </summary>
        [ReferenceTarget(cmc_tripactivity.EntityLogicalName)]
        [RequiredArgument]
        [Input("TripActivity")]
        public InArgument<EntityReference> TripActivityId { get; set; }

        /// <summary>
        /// Timezone information
        /// </summary>
        [Output("Timezone")]
        public OutArgument<string> TimezoneInformation { get; set; }

    }
}
