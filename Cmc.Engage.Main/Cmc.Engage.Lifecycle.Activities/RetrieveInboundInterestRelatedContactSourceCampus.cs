using System.Activities;
using Cmc.Core.Xrm.ServerExtension.Core;
using Cmc.Engage.Models;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;

namespace Cmc.Engage.Lifecycle.Activities
{
    /// <summary>
    /// This step retrieves the source Campus of the Inbound Interest's Contact
    /// </summary>
    public class RetrieveInboundInterestRelatedContactSourceCampus : RetrieveInboundInterestRelatedContactField
    {
        protected override void Execute(IActivityExecutionContext executionContext)
        {
            Execute(executionContext, "cmc_sourcecampusid", SourceCampusId);
        }

        /// <summary>
        /// The source campus of the Inbound Interest's Contact.
        /// </summary>
        [ReferenceTarget(Account.EntityLogicalName)]
        [RequiredArgument]
        [Output("Source Campus")]
        public OutArgument<EntityReference> SourceCampusId { get; set; }
    }
}
