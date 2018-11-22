using System.Activities;
using Cmc.Core.Xrm.ServerExtension.Core;
using Cmc.Engage.Models;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;

namespace Cmc.Engage.Lifecycle.Activities
{
    /// <summary>
    /// This step retrieves the current Campus of an Inbound Interest's Contact.
    /// </summary>
    public class RetrieveInboundInterestRelatedContactCurrentCampus : RetrieveInboundInterestRelatedContactField
    {
        protected override void Execute(IActivityExecutionContext executionContext)
        {
            Execute(executionContext, "parentcustomerid", CurrentCampusId);
        }

        /// <summary>
        /// The current campus of an Inbound Interest's Contact.
        /// </summary>
        [ReferenceTarget(Account.EntityLogicalName)]
        [RequiredArgument]
        [Output("Current Campus")]
        public OutArgument<EntityReference> CurrentCampusId { get; set; }
    }
}
