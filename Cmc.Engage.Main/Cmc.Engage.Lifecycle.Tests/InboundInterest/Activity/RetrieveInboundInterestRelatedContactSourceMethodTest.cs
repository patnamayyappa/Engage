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
    public class RetrieveInboundInterestRelatedContactSourceMethodTest
    {
        [TestCategory("Activity"), TestCategory("Positive")]
        [TestMethod]
        public void SourceMethod_RetrieveInboundInterestRelatedContact_ContactLoockUp()
        {
            #region ARRANGE
            var contactId = Guid.NewGuid();
            var academic = PrepareAcadamicPeriod();
            var lead = PrepareLead(contactId);
            var sourceMethod = PrepareSourceMethod();
            var contact = PrepareContact(contactId, academic.Id, lead.Id, sourceMethod.Id);
            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>()
            {
                sourceMethod,
                contact,
                lead
            });
            #endregion ARRANGE
            #region ACT     
            var mockLogger = new Mock<ILogger>();
            var mockInboundInterestService =
                new InboundInterestService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
            var resultInboundInterestContactLookup =
                mockInboundInterestService.RetrieveInboundInterestContactLookup("cmc_sourcemethodid", lead.ToEntityReference());
            #endregion  ACT
            #region ASSERT
            Assert.IsNotNull(resultInboundInterestContactLookup.Id);
            #endregion ASERT
        }
        private Contact PrepareContact(Guid contactId, Guid academicGuid, Guid leadGuid, Guid sourceMethodGuid)
        {
            var contact = new Contact()
            {
                Id = contactId,
                mshied_CurrentAcademicPeriodId = new EntityReference(mshied_academicperiod.EntityLogicalName, academicGuid),
                OriginatingLeadId = new EntityReference(Lead.EntityLogicalName, leadGuid),
                cmc_sourcemethodid = new EntityReference("cmc_sourcemethodid", sourceMethodGuid)
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
        private Entity PrepareSourceMethod()
        {
            return new Entity("cmc_sourcemethod", Guid.NewGuid())
            {
                ["cmc_sourcemethod"] = "Test SourceMethod"
            };
        }
    }
}
