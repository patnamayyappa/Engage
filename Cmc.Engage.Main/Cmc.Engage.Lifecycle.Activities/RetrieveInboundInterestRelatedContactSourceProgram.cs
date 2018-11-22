using System.Activities;
using System.ComponentModel;
using Cmc.Core.Xrm.ServerExtension.Core;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;

namespace Cmc.Engage.Lifecycle.Activities
{
    /// <summary>
    /// This step retrieves the source Program of the Inbound Interest's contact.
    /// </summary>

    [DisplayName("Get source Program of Inbound Interest's Contact")]
    public class RetrieveInboundInterestRelatedContactSourceProgram : RetrieveInboundInterestRelatedContactField
    {
        protected override void Execute(IActivityExecutionContext executionContext)
        {
            Execute(executionContext, "cmc_sourceprogramid", SourceProgramId);
        }

        /// <summary>
        /// The source Program of the Inbound Interest's contact.
        /// </summary>
        [ReferenceTarget("mshied_program")]
        [RequiredArgument]
        [Output("Source Program")]
        public OutArgument<EntityReference> SourceProgramId { get; set; }
    }
}
