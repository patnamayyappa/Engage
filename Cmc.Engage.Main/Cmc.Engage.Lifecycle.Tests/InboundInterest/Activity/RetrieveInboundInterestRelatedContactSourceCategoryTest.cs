using System;
using System.Collections.Generic;
using Cmc.Core.Xrm.ServerExtension.Logging;
using Cmc.Engage.Models;
using FakeXrmEasy;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Moq;

namespace Cmc.Engage.Lifecycle.Tests.InboundInterest.Activity
{
    [TestClass]
    public class RetrieveInboundInterestRelatedContactSourceCategoryTest
    {
        [TestCategory("Activity"), TestCategory("Positive")]
        [TestMethod]
        public void SourceCategory_RetrieveInboundInterestRelatedContact_ContactLoockUp()
        {
            #region ARRANGE
            var contactId = Guid.NewGuid();
            var academic = PrepareAcadamicPeriod();
            var lead = PrepareLead(contactId);
            var sourceCategory = PrepareSourceCategory();
            var contact = PrepareContact(contactId, academic.Id, lead.Id, sourceCategory.Id);
            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>()
            {
                sourceCategory,
                contact,
                lead
            });
            #endregion ARRANGE
            #region ACT     
            var mockLogger = new Mock<ILogger>();
            var mockInboundInterestService =
                new InboundInterestService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
            var resultInboundInterestContactLookup =
                mockInboundInterestService.RetrieveInboundInterestContactLookup("cmc_sourcecategoryid", lead.ToEntityReference());
            #endregion  ACT
            #region ASSERT
            Assert.IsNotNull(resultInboundInterestContactLookup.Id);
            #endregion ASERT
        }
        private Contact PrepareContact(Guid contactId, Guid academicGuid, Guid leadGuid, Guid sourceCategoryGuid)
        {
            var contact = new Contact()
            {
                Id = contactId,
                mshied_CurrentAcademicPeriodId = new EntityReference(mshied_academicperiod.EntityLogicalName, academicGuid),
                OriginatingLeadId = new EntityReference(Lead.EntityLogicalName, leadGuid),
                cmc_sourcecategoryid = new EntityReference("cmc_sourcecategoryid", sourceCategoryGuid)
            };
            return contact;
        }
        private mshied_academicperiod PrepareAcadamicPeriod()
        {
            var acadamicPeriod = new mshied_academicperiod()
            {
                Id = Guid.NewGuid()
            };
            return acadamicPeriod;
        }
        private Lead PrepareLead(Guid contactId)
        {
            var lead = new Lead()
            {
                Id = Guid.NewGuid(),
                CustomerId = new EntityReference(Contact.EntityLogicalName, contactId)
            };
            return lead;
        }
        private Entity PrepareSourceCategory()
        {
            return new Entity("cmc_sourcecategory", Guid.NewGuid())
            {
                ["cmc_sourcecategory"] = "sourcecategory"
            };
        }
    }
}
