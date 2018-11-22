using System;
using System.Collections.Generic;
using Cmc.Core.Xrm.ServerExtension.Logging;
using Cmc.Engage.Common;
using Cmc.Engage.Common.Plugins.Tests.Utilities;
using Cmc.Engage.Models;
using FakeItEasy;
using FakeXrmEasy;
using FakeXrmEasy.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Moq;

namespace Cmc.Engage.Lifecycle.Tests.DomDefinition.Plugin
{
    /// <summary>
    /// Summary description for ValidateDomDefinitionExecutionOrderPluginTest
    /// </summary>
    [TestClass]
    public class ValidateDomDefinitionExecutionOrderPluginTest : XrmUnitTestBase
    {
        [TestCategory("Activity"), TestCategory("Positive")]
        [TestMethod]
        public void ValidateDomDefinitionExecutionOrderPlugin_CreateDomDefintionExecutionOrder_Test()
        {
            #region ARRANGE
            var contact = PrepareContact();
            var listInstance = PrepareMarketingList();
            var domMaster = PrepareDomMaster(listInstance.Id);
            var domDefinitionExecutionOrder = PrepareDomDefintionExecutionOrder(domMaster.Id);
            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>()
            {
                listInstance,
                domMaster,
                domDefinitionExecutionOrder,
                contact
            });
            //Initialize the Mock Service
            var mockServiceProvider = InitializeMockService(xrmFakedContext, domDefinitionExecutionOrder, Operation.Create);
            //Fetch the Mock Execution Context
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);


            var entityMetadata = new EntityMetadata() { LogicalName = contact.LogicalName, MetadataId = Guid.NewGuid() };
            var attributeMetadata = new AttributeMetadata() { LogicalName = "cmc_isstudent" };
            xrmFakedContext.InitializeMetadata(entityMetadata);
            entityMetadata.SetAttributeCollection(new List<AttributeMetadata>() { attributeMetadata });
            var entityMetadataCollection = new EntityMetadataCollection();
            entityMetadataCollection.Add(entityMetadata);
            var retrieveMetadataChangesResponse = new RetrieveMetadataChangesResponse()
            {
                Results = new ParameterCollection
                        {
                            { "EntityMetadata", entityMetadataCollection}
                        }
            };

            #endregion

            #region ACT
            //Mock the ILogger
            var mockLogger = new Mock<ILogger>();
            var mockLanguageService = new Mock<ILanguageService>();
            A.CallTo(() => xrmFakedContext.GetFakedOrganizationService().Execute(A<RetrieveMetadataChangesRequest>.Ignored)).Returns(retrieveMetadataChangesResponse);
            var mockDomDefinitionExecutionOrderService = new DomDefinitionExecutionOrderService(mockLogger.Object,
                 mockLanguageService.Object, xrmFakedContext.GetFakedOrganizationService());

            mockDomDefinitionExecutionOrderService.ValidateDomDefinitionExecutionOrder(mockExecutionContext.Object);

            #endregion
            #region ASSERT
            Assert.IsTrue(true);
            #endregion ASSERT
        }

        [TestCategory("Activity"), TestCategory("Negative")]
        [TestMethod]
        public void ValidateDomDefinitionExecutionOrderPlugin_NegaticeScenario()
        {
            #region ARRANGE
            XrmFakedContext xrmFakedContext = new XrmFakedContext();
            #endregion

            #region ACT
            var mocklogger = new Mock<ILogger>();
            var mockLanguageService = new LanguageService(mocklogger.Object, xrmFakedContext.GetFakedOrganizationService());
           
            #endregion

            #region ASSERT
            Assert.ThrowsException<ArgumentNullException>(() => new DomDefinitionExecutionOrderService(null, mockLanguageService, xrmFakedContext.GetFakedOrganizationService()));
            Assert.ThrowsException<ArgumentException>(() => new DomDefinitionExecutionOrderService(mocklogger.Object, null,null));
            #endregion
        }

        private cmc_domdefinitionexecutionorder PrepareDomDefintionExecutionOrder(Guid domMasterId)
        {
            var domDefinitionExecutionOrder = new cmc_domdefinitionexecutionorder()
            {
                Id = Guid.NewGuid(),
                cmc_attributeschema = "contact.firstname",
                cmc_dommasterid = new EntityReference("cmc_dommaster", domMasterId),
            };
            return domDefinitionExecutionOrder;
        }

        private cmc_dommaster PrepareDomMaster(Guid marketingListId)
        {
            var domMaster = new cmc_dommaster()
            {
                Id = Guid.NewGuid(),
                cmc_marketinglistid = new EntityReference("List", marketingListId),
                cmc_runassignmentforentity = new OptionSetValue(175490001)
            };
            return domMaster;
        }

        private Entity PrepareMarketingList()
        {
            var marketingListInstance = new Entity("List", Guid.NewGuid())
            {
            };
            return marketingListInstance;
        }

        private Contact PrepareContact()
        {
            var contact = new Contact()
            {
                Id = Guid.NewGuid()
            };
            return contact;
        }
    }
}
