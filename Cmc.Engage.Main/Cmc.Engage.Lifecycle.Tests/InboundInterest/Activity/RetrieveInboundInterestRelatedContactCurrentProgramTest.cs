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
    public class RetrieveInboundInterestRelatedContactCurrentProgramTest
    {
        [TestCategory("Activity"), TestCategory("Positive")]
        [TestMethod]
        public void CurrentProgram_RetrieveInboundInterestRelatedContact_ContactLookup()
        {
            #region ARRANGE

            var contactId = Guid.NewGuid();
            var academic = PrepareAcadamicPeriod();
            var lead = PrepareLead(contactId);
            var program = PrepareProgram();
            var contact = PrepareContact(contactId, academic.Id, lead.Id, program.Id);
           
            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>()
            {
                program,
                contact,
                lead
            });

            #endregion
            //var leadEntityRef=new EntityReference("lead",lead.Id);
            #region ACT

            var mockLogger = new Mock<ILogger>();
            var mockInboundInterestService =
                new InboundInterestService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
            var resultInboundInterestContactLookup =
                mockInboundInterestService.RetrieveInboundInterestContactLookup("mshied_currentprogramid", lead.ToEntityReference());

            #endregion

            #region ASSERT

            Assert.IsNotNull(resultInboundInterestContactLookup.Id);

            #endregion
        }

        private Contact PrepareContact(Guid contactId, Guid academicGuid, Guid leadGuid, Guid programGuid)
        {
            var contact = new Contact()
            {
                Id = contactId,
                mshied_CurrentAcademicPeriodId = new EntityReference(mshied_academicperiod.EntityLogicalName, academicGuid),
                OriginatingLeadId = new EntityReference(Lead.EntityLogicalName, leadGuid),
                mshied_CurrentProgramId = new EntityReference("cmc_program", programGuid)
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

        private Entity PrepareProgram()
        {
            return new Entity("mshied_program", Guid.NewGuid())
            {
                ["mshied_name"] ="Test Program"
            };
        }
    }
}

