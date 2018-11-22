using System.Activities;
using Cmc.Core.Xrm.ServerExtension.Core;
using Cmc.Engage.Models;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;

namespace Cmc.Engage.Lifecycle.Activities
{
    /// <summary>
    /// This step retrieves the source Expected Start of the Inbound Interest's Contact.
    /// </summary>
    public class RetrieveInboundInterestRelatedContactSourceExpectedStart : RetrieveInboundInterestRelatedContactField
    {
        protected override void Execute(IActivityExecutionContext executionContext)
        {
            Execute(executionContext, "cmc_expectedstartid", ExpectedStartId);
        }

        /// <summary>
        /// The source Expected Start of the Inbound Interest's Contact.
        /// </summary>
        [ReferenceTarget(mshied_academicperiod.EntityLogicalName)]
        [RequiredArgument]
        [Output("Source Expected Start")]
        public OutArgument<EntityReference> ExpectedStartId { get; set; }
    }
}
