using System.Activities;
using System.ComponentModel;
using Cmc.Core.Xrm.ServerExtension.Core;
using Cmc.Engage.Models;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;

namespace Cmc.Engage.Lifecycle.Activities
{
    /// <summary>
    /// This step retrieves the source Referring Organization of the Inbound Interest's contact.
    /// </summary>

    [DisplayName("Get source Referring Organization of Inbound Interest's Contact")]
    public class RetrieveInboundInterestRelatedContactSourceReferringOrganization : RetrieveInboundInterestRelatedContactField
    {
        protected override void Execute(IActivityExecutionContext executionContext)
        {
            Execute(executionContext, "cmc_sourcereferringorganizationid", SourceReferringOrganizationId);
        }

        /// <summary>
        /// The source Referring Organization of the Inbound Interest's contact.
        /// </summary>
        [ReferenceTarget(Account.EntityLogicalName)]
        [RequiredArgument]
        [Output("Source Referring Organization")]
        public OutArgument<EntityReference> SourceReferringOrganizationId { get; set; }
    }
}
