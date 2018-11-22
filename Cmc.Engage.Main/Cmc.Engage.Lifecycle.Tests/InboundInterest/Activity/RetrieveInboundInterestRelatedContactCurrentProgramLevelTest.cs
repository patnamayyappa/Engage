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
    public class RetrieveInboundInterestRelatedContactCurrentProgramLevelTest
    {
        [TestCategory("Activity"), TestCategory("Positive")]
        [TestMethod]
        public void CurrentProgramLevel_RetrieveInboundInterestRelatedContact_ContactLoockUp()
        {
            #region ARRANGE
            var contactId = Guid.NewGuid();
            var academic = PrepareAcadamicPeriod();
            var lead = PrepareLead(contactId);
            var programLevel = PrepareProgramLevel();
            var contact = PrepareContact(contactId, academic.Id, lead.Id, programLevel.Id);
            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>()
            {
                programLevel,
                contact,
                lead
            });
            #endregion ARRANGE

            #region ACT     
            var mockLogger = new Mock<ILogger>();
            var mockInboundInterestService =
                new InboundInterestService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
            var resultInboundInterestContactLookup =
                mockInboundInterestService.RetrieveInboundInterestContactLookup("mshied_currentprogramlevelid", lead.ToEntityReference());
            #endregion  ACT

            #region ASSERT
            Assert.IsNotNull(resultInboundInterestContactLookup.Id);
            #endregion ASERT



        }
        private Contact PrepareContact(Guid contactId, Guid academicGuid, Guid leadGuid, Guid programLevelGuid)
        {
            var contact = new Contact()
            {
                Id = contactId,
                mshied_CurrentAcademicPeriodId = new EntityReference(mshied_academicperiod.EntityLogicalName, academicGuid),
                OriginatingLeadId = new EntityReference(Lead.EntityLogicalName, leadGuid),
                mshied_CurrentProgramLevelId = new EntityReference("mshied_programlevel", programLevelGuid)
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

        private Entity PrepareProgramLevel()
        {
            return new Entity("mshied_programlevel", Guid.NewGuid())
            {
                ["mshied_name"] = "Test Program"
            };
        }
    }
}
