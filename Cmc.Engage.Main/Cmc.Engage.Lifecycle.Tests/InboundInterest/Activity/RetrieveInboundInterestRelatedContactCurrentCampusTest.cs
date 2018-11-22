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
    public class RetrieveInboundInterestRelatedContactCurrentCampusTest
    {
        [TestCategory("Activity"), TestCategory("Positive")]
        [TestMethod]
        public void RetrieveInboundInterestRelatedContactCurrentCampus_ContactLookup()
        {
            #region ARRANGE

            var contactId = Guid.NewGuid();
            var account = PrepareAccount();
            var lead = PrepareLead(contactId);
            var contact = PrepareContact(contactId, account.Id, lead.Id);

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
            var mockInboundInterestService =
                new InboundInterestService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
            var resultInboundInterestContactLookup =
                mockInboundInterestService.RetrieveInboundInterestContactLookup("parentcustomerid", lead.ToEntityReference());

            #endregion

        }

        private Contact PrepareContact(Guid contactId, Guid accountId, Guid leadGuid)
        {
            var contact = new Contact()
            {
                Id = contactId,
                OriginatingLeadId = new EntityReference(Lead.EntityLogicalName, leadGuid),
                ParentCustomerId = new EntityReference(Account.EntityLogicalName,accountId)
            };
            return contact;
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

        private Account PrepareAccount()
        {
            var account = new Account()
            {
                Id = Guid.NewGuid(),
            };
            return account;
        }
    }
}
