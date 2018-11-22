using System.Activities;
using Cmc.Core.Xrm.ServerExtension.Core;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;

namespace Cmc.Engage.Lifecycle.Activities
{
    /// <summary>
    /// This step retrieves the current Program of the Inbound Interest's Contact.
    /// </summary>
    public class RetrieveInboundInterestRelatedContactCurrentProgram : RetrieveInboundInterestRelatedContactField
    {
        protected override void Execute(IActivityExecutionContext executionContext)
        {
            Execute(executionContext, "mshied_programid", CurrentProgramId);
        }

        /// <summary>
        /// The current Program of the Inbound Interest's Contact.
        /// </summary>
        [ReferenceTarget("mshied_program")]
        [RequiredArgument]
        [Output("Current Program")]
        public OutArgument<EntityReference> CurrentProgramId { get; set; }
    }
}
