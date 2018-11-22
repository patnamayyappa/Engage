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
    public class RetrieveInboundInterestRelatedContactCurrentAcademicPeriodTest
    {
        [TestCategory("Activity"), TestCategory("Positive")]
        [TestMethod]
        public void RetrieveInboundInterestRelatedContactCurrentAcademicPeriod_ContactLookup()
        {
            #region ARRANGE

            var contactId = Guid.NewGuid();
            var academic = PrepareAcadamicPeriod();
            var lead = PrepareLead(contactId);
            var contact = PrepareContact(contactId, academic.Id, lead.Id);
            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>()
            {
                academic,
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
                mockInboundInterestService.RetrieveInboundInterestContactLookup("mshied_currentacademicperiodid", lead.ToEntityReference());

            #endregion

            #region ASSERT
            

            #endregion
        }

        private Contact PrepareContact(Guid contactId ,Guid academicGuid,Guid leadGuid)
        {
            var contact=new Contact()
            {
                Id = contactId,
                mshied_CurrentAcademicPeriodId = new EntityReference(mshied_academicperiod.EntityLogicalName, academicGuid),
                OriginatingLeadId = new EntityReference(Lead.EntityLogicalName,leadGuid)
            };
            return contact;
        }

        private mshied_academicperiod PrepareAcadamicPeriod()
        {
            var acadamicPeriod=new mshied_academicperiod()
            {
                Id = Guid.NewGuid()
            };
            return acadamicPeriod;
        }

        private Lead PrepareLead(Guid contactId)
        {
            var lead=new Lead()
            {
                Id = Guid.NewGuid(),
                CustomerId = new EntityReference(Contact.EntityLogicalName, contactId)
            };
            return lead;
        }
    }
}
