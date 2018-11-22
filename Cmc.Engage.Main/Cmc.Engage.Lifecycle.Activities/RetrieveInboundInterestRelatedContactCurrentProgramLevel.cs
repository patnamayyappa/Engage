using System.Activities;
using Cmc.Core.Xrm.ServerExtension.Core;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;

namespace Cmc.Engage.Lifecycle.Activities
{
    /// <summary>
    /// This step retrieves the current Program Level of the Inbound Interest's Contact.
    /// </summary>
    public class RetrieveInboundInterestRelatedContactCurrentProgramLevel : RetrieveInboundInterestRelatedContactField
    {
        protected override void Execute(IActivityExecutionContext executionContext)
        {
            Execute(executionContext, "cmc_prgmlevelid", CurrentProgramLevelId);
        }

        /// <summary>
        /// The current Program Level of the Inbound Interest's Contact.
        /// </summary>
        [ReferenceTarget("mshied_programlevel")]
        [Output("Current Program Level")]
        public OutArgument<EntityReference> CurrentProgramLevelId { get; set; }
    }
}
