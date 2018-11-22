using System.Activities;
using Cmc.Core.Xrm.ServerExtension.Core;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;

namespace Cmc.Engage.Lifecycle.Activities
{
    /// <summary>
    /// This step retrieves the source Method of the Inbound Interest's contact.
    /// </summary>
    public class RetrieveInboundInterestRelatedContactSourceMethod : RetrieveInboundInterestRelatedContactField
    {
        protected override void Execute(IActivityExecutionContext executionContext)
        {
            Execute(executionContext, "cmc_sourcemethodid", SourceMethodId);
        }

        /// <summary>
        /// The source Method of the Inbound Interest's contact.
        /// </summary>
        [ReferenceTarget("cmc_sourcemethod")]
        [RequiredArgument]
        [Output("Source Method")]
        public OutArgument<EntityReference> SourceMethodId { get; set; }
    }
}
