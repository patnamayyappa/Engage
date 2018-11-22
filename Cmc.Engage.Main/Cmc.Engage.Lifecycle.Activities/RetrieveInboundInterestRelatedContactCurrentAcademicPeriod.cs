using System.Activities;
using Cmc.Core.Xrm.ServerExtension.Core;
using Cmc.Engage.Models;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;

namespace Cmc.Engage.Lifecycle.Activities
{
    /// <summary>
    /// This step retrieves the current Academic Period  of an Inbound Interest's Contact.
    /// </summary>
    public class RetrieveInboundInterestRelatedContactCurrentAcademicPeriod : RetrieveInboundInterestRelatedContactField
    {
        protected override void Execute(IActivityExecutionContext executionContext)
        {
            Execute(executionContext, "mshied_currentacademicperiodid", CurrentAcademicPeriodId);
        }

        /// <summary>
        /// The current Academic Period of the Inbound Interest's Contact.
        /// </summary>
        [ReferenceTarget(mshied_academicperiod.EntityLogicalName)]
        [Output("Current Academic Period")]
        public OutArgument<EntityReference> CurrentAcademicPeriodId { get; set; }
    }
}
