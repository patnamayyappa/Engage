using System.Activities;
using Cmc.Core.Xrm.ServerExtension.Core;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;

namespace Cmc.Engage.Lifecycle.Activities
{
    /// <summary>
    /// This step retrieves the source campaign of the Inbound Interest's Contact.
    /// </summary>
    public class RetrieveInboundInterestRelatedContactSourceCampaign : RetrieveInboundInterestRelatedContactField
    {
        protected override void Execute(IActivityExecutionContext executionContext)
        {
            Execute(executionContext, "cmc_sourcecampaignid", SourceCampaignId);
        }

        /// <summary>
        /// The source campaign of the Inbound Interest's contact.
        /// </summary>
        [ReferenceTarget("campaign")]
        [RequiredArgument]
        [Output("Source Campaign")]
        public OutArgument<EntityReference> SourceCampaignId { get; set; }
    }
}
