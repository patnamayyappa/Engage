using System.Activities;
using System.ComponentModel;
using Cmc.Core.Xrm.ServerExtension.Core;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;

namespace Cmc.Engage.Lifecycle.Activities
{
    /// <summary>
    /// This step retrieves the source Category of the Inbound Interest's Contact.
    /// </summary>

    [DisplayName("Get source Category of Inbound Interest's Contact")]
    public class RetrieveInboundInterestRelatedContactSourceCategory : RetrieveInboundInterestRelatedContactField
    {
        protected override void Execute(IActivityExecutionContext executionContext)
        {
            Execute(executionContext, "cmc_sourcecategoryid", SourceCategoryId);
        }

        /// <summary>
        /// The source category of the Inbound Interest's Contact.
        /// </summary>
        [ReferenceTarget("cmc_sourcecategory")]
        [RequiredArgument]
        [Output("Source Category")]
        public OutArgument<EntityReference> SourceCategoryId { get; set; }
    }
}
