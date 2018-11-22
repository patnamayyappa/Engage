using Autofac;
using Cmc.Core.Xrm.ServerExtension.Core;
using Cmc.Engage.Models;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;
using System.Activities;

namespace Cmc.Engage.Lifecycle.Activities
{
    /// <summary>
    /// This step returns an Open Lifecycle of a given Contact and its input filter parameters.
    /// </summary>
    public class RetrieveContactOpenLifecycle : ActivityBase
    {
        protected override void Execute(IActivityExecutionContext executionContext)
        {
            var activityContext = executionContext.ActivityContext;

            var filters = new RetrieveContactOpenLifecycleFilters()
            {
                CampusId = CampusId.Get(activityContext),
                CompleteAppReceivedContainsData = CompleteAppReceivedContainsData.Get(activityContext),
                ContactId = ContactId.Get(activityContext),
                DepositReceivedContainsData = DepositReceivedContainsData.Get(activityContext),
                ExpectedStartDateId = ExpectedStartDateId.Get(activityContext),
                LifecycleType = LifecycleType.Get(activityContext),
                ProgramId = ProgramId.Get(activityContext),
                ProgramLevelId = ProgramLevelId.Get(activityContext),
                SourceCampaignId = SourceCampaignId.Get(activityContext),
                SourceCampusId = SourceCampusId.Get(activityContext),
                SourceCategoryId = SourceCategoryId.Get(activityContext),
                SourceMethodId = SourceMethodId.Get(activityContext),
                SourceProgramId = SourceProgramId.Get(activityContext),
                SourceProgramLevelId = SourceProgramLevelId.Get(activityContext),
                SourceReferringContactId = SourceReferringContactId.Get(activityContext),
                SourceReferringOrganizationId = SourceReferringOrganizationId.Get(activityContext),
                SourceReferringStaffId = SourceReferringStaffId.Get(activityContext),
                SourceSubCategoryId = SourceSubCategoryId.Get(activityContext)
            };

            var lifecycleService = executionContext.IocScope.Resolve<ILifecycleService>();
            LifecycleId.Set(activityContext, lifecycleService.RetrieveContactOpenLifecycle(filters));
        }

        /// <summary>
        /// The ID of the Contact.
        /// </summary>
        [ReferenceTarget(Contact.EntityLogicalName)]
        [RequiredArgument]
        [Input("Contact")]
        public InArgument<EntityReference> ContactId { get; set; }

        /// <summary>
        /// The ID of the Campus that is associated with the contact.
        /// </summary>
        [ReferenceTarget(Account.EntityLogicalName)]
        [Input("Campus")]
        public InArgument<EntityReference> CampusId { get; set; }

        /// <summary>
        /// Check for Complete App Received Data that is associated with the contact.
        /// </summary>
        [Input("Complete App Received Contains Data")]
        public InArgument<bool> CompleteAppReceivedContainsData { get; set; }

        /// <summary>
        /// Check for Deposit Received Data that is associated with the Contact.
        /// </summary>
        [Input("Deposit Received Contains Data")]
        public InArgument<bool> DepositReceivedContainsData { get; set; }

        /// <summary>
        /// The ID of the Expected Start Date that is associated with the contact.
        /// </summary>
        [ReferenceTarget(mshied_academicperiod.EntityLogicalName)]
        [Input("Expected Start Date")]
        public InArgument<EntityReference> ExpectedStartDateId { get; set; }

        /// <summary>
        /// The Lifecycle Type that is associated with the contact.
        /// </summary>
        [Input("Lifecycle Type")]
        public InArgument<string> LifecycleType { get; set; }

        /// <summary>
        /// The ID of Program that is associated with the contact.
        /// </summary>
        [ReferenceTarget("mshied_program")]
        [Input("Program")]
        public InArgument<EntityReference> ProgramId { get; set; }

        /// <summary>
        /// The ID of Program Level that is associated with the contact.
        /// </summary>
        [ReferenceTarget("mshied_programlevel")]
        [Input("Program Level")]
        public InArgument<EntityReference> ProgramLevelId { get; set; }

        /// <summary>
        /// The ID of Source Campus that is associated with the contact.
        /// </summary>
        [ReferenceTarget(Account.EntityLogicalName)]
        [Input("Source Campus")]
        public InArgument<EntityReference> SourceCampusId { get; set; }

        /// <summary>
        /// The ID of Source Category that is associated with the contact.
        /// </summary>
        [ReferenceTarget("cmc_sourcecategory")]
        [Input("Source Category")]
        public InArgument<EntityReference> SourceCategoryId { get; set; }

        /// <summary>
        /// The ID of Source Method that is associated with the contact.
        /// </summary>
        [ReferenceTarget("cmc_sourcemethod")]
        [Input("Source Method")]
        public InArgument<EntityReference> SourceMethodId { get; set; }

        /// <summary>
        /// The ID of Source Program that is associated with the contact.
        /// </summary>
        [ReferenceTarget("mshied_program")]
        [Input("Source Program")]
        public InArgument<EntityReference> SourceProgramId { get; set; }

        /// <summary>
        /// The ID of Source Program Level that is associated with the contact.
        /// </summary>
        [ReferenceTarget("mshied_programlevel")]
        [Input("Source Program Level")]
        public InArgument<EntityReference> SourceProgramLevelId { get; set; }

        /// <summary>
        /// The ID of Source Referring Contact that is associated with the contact.
        /// </summary>
        [ReferenceTarget(Contact.EntityLogicalName)]
        [Input("Source Referring Contact")]
        public InArgument<EntityReference> SourceReferringContactId { get; set; }

        /// <summary>
        /// The ID of Source Referring Organization that is associated with the contact.
        /// </summary>
        [ReferenceTarget(Account.EntityLogicalName)]
        [Input("Source Referring Organization")]
        public InArgument<EntityReference> SourceReferringOrganizationId { get; set; }

        /// <summary>
        /// The ID of Source Referring Staff that is associated with the contact.
        /// </summary>
        [ReferenceTarget(SystemUser.EntityLogicalName)]
        [Input("Source Referring Staff")]
        public InArgument<EntityReference> SourceReferringStaffId { get; set; }

        /// <summary>
        /// The ID of Source Campaign that is associated with the contact.
        /// </summary>
        [ReferenceTarget("campaign")]
        [Input("Source Campaign")]
        public InArgument<EntityReference> SourceCampaignId { get; set; }

        /// <summary>
        /// The ID of Source Sub Category that is associated with the contact.
        /// </summary>
        [ReferenceTarget("cmc_sourcesubcategory")]
        [Input("Source Sub Category")]
        public InArgument<EntityReference> SourceSubCategoryId { get; set; }

        /// <summary>
        /// Lifecycle of given Contact
        /// </summary>
        [ReferenceTarget(Opportunity.EntityLogicalName)]
        [Output("Lifecycle")]
        public OutArgument<EntityReference> LifecycleId { get; set; }
    }
}
