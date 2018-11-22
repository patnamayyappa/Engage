using Microsoft.Xrm.Sdk;
using System;

namespace Cmc.Engage.Lifecycle
{
    public class RetrieveContactOpenLifecycleFilters
    {
        [OpportunityField("parentcontactid")]
        public EntityReference ContactId { get; set; }

        [OpportunityField("parentaccountid")]
        public EntityReference CampusId { get; set; }

        [OpportunityField("cmc_completeappreceiveddate")]
        [ContainsDataCondition]
        public bool CompleteAppReceivedContainsData { get; set; }

        [OpportunityField("cmc_depositreceiveddate")]
        [ContainsDataCondition]
        public bool DepositReceivedContainsData { get; set; }

        [OpportunityField("cmc_expstartdateid")]
        public EntityReference ExpectedStartDateId { get; set; }

        [OpportunityField("cmc_lifecycletype")]
        [OptionSetField]
        public string LifecycleType { get; set; }

        [OpportunityField("cmc_prgmid")]
        public EntityReference ProgramId { get; set; }

        [OpportunityField("cmc_prgmlevelid")]
        public EntityReference ProgramLevelId { get; set; }

        [OpportunityField("cmc_sourcecampusid")]
        public EntityReference SourceCampusId { get; set; }

        [OpportunityField("cmc_sourcecategoryid")]
        public EntityReference SourceCategoryId { get; set; }

        [OpportunityField("cmc_sourcemethodid")]
        public EntityReference SourceMethodId { get; set; }

        [OpportunityField("cmc_sourceprgmid")]
        public EntityReference SourceProgramId { get; set; }

        [OpportunityField("cmc_srcprgmlevelid")]
        public EntityReference SourceProgramLevelId { get; set; }

        [OpportunityField("cmc_sourcereferringcontactid")]
        public EntityReference SourceReferringContactId { get; set; }

        [OpportunityField("cmc_sourcereferringorganizationid")]
        public EntityReference SourceReferringOrganizationId { get; set; }

        [OpportunityField("cmc_sourcereferringstaffid")]
        public EntityReference SourceReferringStaffId { get; set; }

        [OpportunityField("cmc_sourcecampaignid")]
        public EntityReference SourceCampaignId { get; set; }

        [OpportunityField("cmc_sourcesubcategoryid")]
        public EntityReference SourceSubCategoryId { get; set; }

        public class OpportunityField : Attribute
        {
            public string LogicalName { get; set; }
            public OpportunityField(string logicalName)
            {
                LogicalName = logicalName;
            }
        }

        public class ContainsDataCondition : Attribute
        {
        }

        public class OptionSetField : Attribute
        {
        }
    }
}
