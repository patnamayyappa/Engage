using System.Activities;
using Cmc.Core.Xrm.ServerExtension.Core;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;

namespace Cmc.Engage.Lifecycle.Activities
{
    /// <summary>
    /// This step retrieves the School Status of the Inbound Interest's Contact.
    /// </summary>
    public class RetrieveInboundInterestRelatedContactSchoolStatus : RetrieveInboundInterestRelatedContactField
    {
        protected override void Execute(IActivityExecutionContext executionContext)
        {
            Execute(executionContext, "mshied_studentstatusid", SchoolStatusId);
        }

        /// <summary>
        ///  The School Status of the Inbound Interest's Contact.
        /// </summary>
        [ReferenceTarget("mshied_studentstatus")]
        [Output("School Status")]
        public OutArgument<EntityReference> SchoolStatusId { get; set; }
    }
}
