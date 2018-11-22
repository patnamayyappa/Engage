using System.Activities;
using System.ComponentModel;
using Cmc.Core.Xrm.ServerExtension.Core;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;

namespace Cmc.Engage.Lifecycle.Activities
{
    /// <summary>
    /// This step retrieves the source Sub-category of the Inbound Interest's contact.
    /// </summary>

    [DisplayName("Get source Sub-category of Inbound Interest's Contact")]
    public class RetrieveInboundInterestRelatedContactSourceSubCategory : RetrieveInboundInterestRelatedContactField
    {
        protected override void Execute(IActivityExecutionContext executionContext)
        {
            Execute(executionContext, "cmc_sourcesubcategoryid", SourceSubcategoryId);
        }

        /// <summary>
        /// The source Sub-category of the Inbound Interest's contact.
        /// </summary>
        [ReferenceTarget("cmc_sourcesubcategory")]
        [RequiredArgument]
        [Output("Source Sub-Category")]
        public OutArgument<EntityReference> SourceSubcategoryId { get; set; }
    }
}
