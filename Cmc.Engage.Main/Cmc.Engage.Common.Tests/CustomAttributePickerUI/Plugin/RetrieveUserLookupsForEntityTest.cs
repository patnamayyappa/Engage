using System;
using System.Collections.Generic;
using Cmc.Core.Xrm.ServerExtension.Logging;
using Cmc.Engage.Common.Plugins.Tests.Utilities;
using FakeXrmEasy;
using FakeXrmEasy.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using Moq;

namespace Cmc.Engage.Common.Tests.CustomAttributePickerUI.Plugin
{
    [TestClass]
    public class RetrieveUserLookupsForEntityTest : XrmUnitTestBase
    {
        [TestCategory("Plugin"), TestCategory("Positive")]
        [TestMethod]
        public void RetrieveUser_RetrieveUserLookups_EntityMetadataJson()
        {
            #region ARRANGE
            var xrmFakedContext = new XrmFakedContext();
            var calledId = Guid.NewGuid();
            xrmFakedContext.CallerId = new EntityReference("SystemUser", calledId);

            var systemUser = new Entity("SystemUser", xrmFakedContext.CallerId.Id)
            {
                ["systemuserid"] = xrmFakedContext.CallerId.Id,
            };

            var mockUserSettings = new Entity("usersettings", Guid.NewGuid())
            {
                ["localeid"] = 1033,
                ["systemuserid"] = systemUser.ToEntityReference()
            };

            xrmFakedContext.Initialize(new List<Entity>() { mockUserSettings, systemUser });


            xrmFakedContext.AddRelationship("user_settings", new XrmFakedRelationship
            {
                RelationshipType = XrmFakedRelationship.enmFakeRelationshipType.OneToMany,
                Entity1LogicalName = "systemuser",
                Entity1Attribute = "systemuserid",
                Entity2LogicalName = "usersettings",
                Entity2Attribute = "systemuserid"

            });

            //Initialize the Mock Service
            var mockServiceProvider = InitializeMockService(xrmFakedContext, mockUserSettings, Operation.cmc_retrieveuserlookupsforentity);
            //Adding Input Parameter
            
            AddInputParameters(mockServiceProvider, "EntityLogicalName", "contact");

            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);

            var retrieveMetadataChangesRequest = new RetrieveMetadataChangesRequest(){ };

            string[] target = { "systemuser"};

            var userLocalizedLabel = new LocalizedLabel("systemuser", 1033);
            LocalizedLabel[] labels = { userLocalizedLabel  };


            var entityMetadata = new EntityMetadata() { LogicalName = "contact", DisplayName = new Label(userLocalizedLabel, labels) };
            entityMetadata.SetAttributeCollection(new List<AttributeMetadata>()
            {
                new UniqueIdentifierAttributeMetadata("contactid"),
                new StringAttributeMetadata("contactname"),
                new LookupAttributeMetadata(){
                    SchemaName = "systemuser",
                    DisplayName = new Label("systemuser Lookup",1033),
                    Targets = target
                }
            });

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
            var mockLogger = new Mock<ILogger>();

            var entityCollection = new EntityCollection();
            entityCollection.Entities.Add(mockUserSettings);
            var mockOrganizationService = new Mock<IOrganizationService>();
            mockOrganizationService.Setup(r => r.RetrieveMultiple(It.IsAny<FetchExpression>())).Returns(entityCollection);
            mockOrganizationService.Setup(r => r.Execute(It.IsAny<RetrieveMetadataChangesRequest>())).Returns(retrieveMetadataChangesResponse);
            //var mockCustomAttributePickerUIService = new CustomAttributePickerUIService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
            var mockCustomAttributePickerUIService = new CustomAttributePickerUIService(mockLogger.Object, mockOrganizationService.Object);
            mockCustomAttributePickerUIService.RetrieveUserLookupsForEntity(mockExecutionContext.Object);

            #endregion ACT

            #region ASSERT

            var mockPluginExecutionContext = (IPluginExecutionContext)mockServiceProvider.Object.GetService(typeof(IPluginExecutionContext));

            var result = mockPluginExecutionContext.OutputParameters["AttributeJson"];

            Assert.IsNotNull(result);

            #endregion ASSERT
        }


    }
}
