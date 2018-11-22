using System;
using System.Collections.Generic;
using Cmc.Core.Xrm.ServerExtension.Logging;
using Cmc.Engage.Common.Plugins.Tests.Utilities;
using Cmc.Engage.Models;
using FakeXrmEasy;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Moq;

namespace Cmc.Engage.Lifecycle.Tests.InboundInterest.Activity
{
    [TestClass]
    public class RetrieveInboundInterestRelatedContactSourceReferringStaffTest : XrmUnitTestBase
    {
        [TestCategory("Activity"), TestCategory("Positive")]
        [TestMethod]
        public void RetrieveInboundInterestRelatedContactSourceReferringStaff_ContactLookup()
        {
            #region ARRANGE
            var contactId = Guid.NewGuid();
            var systemuser = PreparingSystemUser();
            var lead = PreparingLead(contactId);
            var contact = PreparingContact(contactId, systemuser.Id, lead.Id);

            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>()
            {
                systemuser,
                lead,
                contact
            });
            #endregion

            #region ACT
            var mockLogger = new Mock<ILogger>();

            var inboundInterestService = new InboundInterestService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
            var result = inboundInterestService.RetrieveInboundInterestContactLookup("cmc_sourcereferringstaffid", lead.ToEntityReference());

            #endregion

            #region ASSERT

            Assert.AreEqual(systemuser.Id, result.Id);

            #endregion
        }

        private SystemUser PreparingSystemUser()
        {
            var systemuser = new SystemUser()
            {
                Id = Guid.NewGuid()
            };
            return systemuser;
        }

        private Lead PreparingLead(Guid contactId)
        {
            var lead = new Lead()
            {
                Id = Guid.NewGuid(),
                CustomerId = new EntityReference(Contact.EntityLogicalName, contactId)
            };
            return lead;
        }

        private Contact PreparingContact(Guid contactId, Guid systemuserId, Guid leadId)
        {
            var contact = new Contact()
            {
                Id = contactId,
                cmc_sourcereferringstaffid = new EntityReference(SystemUser.EntityLogicalName, systemuserId),
                OriginatingLeadId = new EntityReference(Lead.EntityLogicalName, leadId)
            };
            return contact;
        }
    }
}
