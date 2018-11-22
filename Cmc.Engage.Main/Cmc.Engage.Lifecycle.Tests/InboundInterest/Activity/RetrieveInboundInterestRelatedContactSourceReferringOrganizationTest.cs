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
    public class RetrieveInboundInterestRelatedContactSourceReferringOrganizationTest : XrmUnitTestBase
    {
        [TestCategory("Activity"), TestCategory("Positive")]
        [TestMethod]
        public void RetrieveInboundInterestRelatedContactSourceReferringOrganization_ContactLookup()
        {
            #region ARRANGE
            var contactId = Guid.NewGuid();
            var account = PreparingAccount();
            var lead = PreparingLead(contactId);
            var contact = PreparingContact(contactId, account.Id, lead.Id);

            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>()
            {
                account,
                lead,
                contact
            });
            #endregion

            #region ACT

            var mockLogger = new Mock<ILogger>();

            var inboundInterestService = new InboundInterestService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
            var result = inboundInterestService.RetrieveInboundInterestContactLookup("cmc_sourcereferringorganizationid", lead.ToEntityReference());

            #endregion

            #region ASSERT
            
            Assert.AreEqual(account.Id, result.Id);           

            #endregion
        }

        private Account PreparingAccount()
        {
            var sro = new Account()
            {
                Id = Guid.NewGuid()
            };
            return sro;
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

        private Contact PreparingContact(Guid contactId, Guid accountId, Guid leadId)
        {
            var contact = new Contact()
            {
                Id = contactId,                            
                cmc_sourcereferringorganizationid = new EntityReference(Account.EntityLogicalName,accountId),
                OriginatingLeadId = new EntityReference(Lead.EntityLogicalName, leadId)
            };
            return contact;
        }
    }
}
