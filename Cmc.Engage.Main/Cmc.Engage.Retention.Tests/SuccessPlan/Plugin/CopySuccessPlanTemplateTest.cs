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

namespace Cmc.Engage.Retention.Tests.SuccessPlan.Plugin
{
    [TestClass]
    public class CopySuccessPlanTemplateTest : XrmUnitTestBase
    {
        [TestCategory("Plugin"), TestCategory("Positive")]
        [TestMethod]
        public void CopySuccessPlanTemplateService_CreateCmcSuccessplantemplate()
        {
            //Need to mock all the dependency Injections passed to the services constructor
            #region ARRANGE
            //Prepare the Target Instance for which the plugin would be trigerred
            var entitySuccessPlanTemplate = PrepareEntitySuccessPlanTemplate();

            var entitySuccessPlantodoTemplate = PrepareEntitySuccessPlantodoTemplate(entitySuccessPlanTemplate);

            var successPlanTemplate = PrepareSuccessPlanTemplate(entitySuccessPlanTemplate);

            //Prepare the FakeContext, here add all the entities 
            //which we expect to be in the database system
            //using the Initialize method
            var xrmFakedContext = new XrmFakedContext();

            xrmFakedContext.Initialize(new List<Entity>
            {
                entitySuccessPlanTemplate,
                entitySuccessPlantodoTemplate,
            });

            var entityCollection = new EntityCollection();

            entityCollection.Entities.Add(entitySuccessPlantodoTemplate);

            //Initialize the Mock Service
            var mockServiceProvider = InitializeMockService(xrmFakedContext, successPlanTemplate, Operation.Create);

            //Fetch the Mock Execution Context
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            var entityMetadata = new EntityMetadata() { LogicalName = entitySuccessPlanTemplate.LogicalName, MetadataId = Guid.NewGuid() };
            var attributeMetadata = new AttributeMetadata() { LogicalName = "cmc_successplantodotemplateid", };
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
            #endregion ARRANGE

            #region ACT
            //Instantiate the Class with the mocked dependencies.
            //Need to mock all the dependency Injections passed to the services constructor
            //Mock the ILogger
            var mockLogger = new Mock<ILogger>();
            var mockILanguageService = new Mock<ILanguageService>();
            var mockOrganizationService = new Mock<IOrganizationService>();
            A.CallTo(() => xrmFakedContext.GetFakedOrganizationService().Execute(A<RetrieveMetadataChangesRequest>._)).Returns(retrieveMetadataChangesResponse);
            var successPlanService = new SuccessPlanService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService(), mockILanguageService.Object);
            successPlanService.CopySuccessPlanTemplate(mockExecutionContext.Object);


            #endregion ACT

            #region ASSERT
            var preSuccessplantemplateData = new Entity("cmc_successplantodotemplate");
            xrmFakedContext.Data["cmc_successplantodotemplate"].TryGetValue(entitySuccessPlanTemplate.Id, out preSuccessplantemplateData);

            //Assert if the business logic performed is correct.

            #endregion ASSERT
        }

        [TestCategory("Plugin"), TestCategory("Positive")]
        [TestMethod]
        public void CopySuccessPlanTemplateService_cmc_copyfromsuccessplantemplateidIsNull()
        {
            //Need to mock all the dependency Injections passed to the services constructor
            #region ARRANGE
            //Prepare the Target Instance for which the plugin would be trigerred
            var entitySuccessPlanTemplate = PrepareEntitySuccessPlanTemplate();

            var successPlanTemplate = PrepareSuccessPlanTemplate(entitySuccessPlanTemplate);
            successPlanTemplate.cmc_copyfromsuccessplantemplateid = null;
            //Prepare the FakeContext, here add all the entities 
            //which we expect to be in the database system
            //using the Initialize method
            var xrmFakedContext = new XrmFakedContext();

            xrmFakedContext.Initialize(new List<Entity>
            {
                entitySuccessPlanTemplate,
            });

            //Initialize the Mock Service
            var mockServiceProvider = InitializeMockService(xrmFakedContext, successPlanTemplate, Operation.Create);

            //Fetch the Mock Execution Context
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            var mockLogger = new Mock<ILogger>();
            var mockILanguageService = new Mock<ILanguageService>();
            var successPlanService = new SuccessPlanService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService(), mockILanguageService.Object);
            successPlanService.CopySuccessPlanTemplate(mockExecutionContext.Object);

            #endregion ACT

            #region Assert
            xrmFakedContext.Data["cmc_successplantodotemplate"].TryGetValue(entitySuccessPlanTemplate.Id, out entitySuccessPlanTemplate);
            var result = entitySuccessPlanTemplate.Attributes.Contains("cmc_copyfromsuccessplantemplateid");
            Assert.IsFalse(result);
            #endregion

        }
        private Entity PrepareEntitySuccessPlanTemplate()
        {
            var entitySuccessPlanTemplate = new Entity("cmc_successplantodotemplate", Guid.NewGuid())// cmc_successplantemplate
            {
                ["cmc_successplantemplatename"] = "Test Success Plan Template"
            };
            return entitySuccessPlanTemplate;
        }
        private Entity PrepareEntitySuccessPlantodoTemplate(Entity entitySPTemplateId)
        {
            var entitySuccessPlantodoTemplate = new cmc_successplantodotemplate()
            {
                Id = Guid.NewGuid(),
                cmc_successplantodotemplatename = "Test Success Plan Do To Template",
                cmc_successplantemplateid = entitySPTemplateId.ToEntityReference(),
            };

            return entitySuccessPlantodoTemplate;
        }
        private cmc_successplantemplate PrepareSuccessPlanTemplate(Entity entitySPTemplateId)
        {
            var successPlanTemplate = new cmc_successplantemplate()
            {
                cmc_copyfromsuccessplantemplateid = entitySPTemplateId.ToEntityReference(),
            };
            return successPlanTemplate;
        }
    }
}
