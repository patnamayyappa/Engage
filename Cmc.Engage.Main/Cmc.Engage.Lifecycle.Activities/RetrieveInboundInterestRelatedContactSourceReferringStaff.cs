using System.Activities;
using System.ComponentModel;
using Cmc.Core.Xrm.ServerExtension.Core;
using Cmc.Engage.Models;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;

namespace Cmc.Engage.Lifecycle.Activities
{
    /// <summary>
    /// This step retrieves the source Referring Staff of the Inbound Interest's contact.
    /// </summary>

    [DisplayName("Get source Referring Staff of Inbound Interest's Contact")]
    public class RetrieveInboundInterestRelatedContactSourceReferringStaff : RetrieveInboundInterestRelatedContactField
    {
        protected override void Execute(IActivityExecutionContext executionContext)
        {
            Execute(executionContext, "cmc_sourcereferringstaffid", SourceReferringStaffId);
        }

        /// <summary>
        /// The source Referring Staff of the Inbound Interest's contact.
        /// </summary>
        [ReferenceTarget(SystemUser.EntityLogicalName)]
        [RequiredArgument]
        [Output("Source Referring Staff")]
        public OutArgument<EntityReference> SourceReferringStaffId { get; set; }
    }
}
