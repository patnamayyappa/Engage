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
    public class RetrieveInboundInterestRelatedContactSourceSubCategoryTest : XrmUnitTestBase
    {
        [TestCategory("Activity"), TestCategory("Positive")]
        [TestMethod]
        public void RetrieveInboundInterestRelatedContactSourceSubCategory_ContactLookup()
        {
            #region ARRANGE
            var contactId = Guid.NewGuid();
            var source = PreparingSourceSubCategory();
            var lead = PreparingLead(contactId);
            var contact = PreparingContact(contactId, source.Id, lead.Id);

            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>()
            {
                source,
                lead,
                contact
            });
            #endregion

            #region ACT

            var mockLogger = new Mock<ILogger>();

            var inboundInterestService = new InboundInterestService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
            var result = inboundInterestService.RetrieveInboundInterestContactLookup("cmc_sourcesubcategoryid", lead.ToEntityReference());

            #endregion

            #region ASSERT

            Assert.AreEqual(source.Id, result.Id);

            #endregion
        }

        private Entity PreparingSourceSubCategory()
        {
            var source = new Entity("cmc_sourcesubcategory")
            {
                Id = Guid.NewGuid()
            };
            return source;
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

        private Contact PreparingContact(Guid contactId, Guid sourceId, Guid leadId)
        {
            var contact = new Contact()
            {
                Id = contactId,
                cmc_sourcesubcategoryid = new EntityReference("cmc_sourcesubcategory", sourceId),
                OriginatingLeadId = new EntityReference(Lead.EntityLogicalName, leadId)
            };
            return contact;
        }
    
    }
}
