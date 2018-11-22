using System;
using System.Collections.Generic;
using System.Linq;
using Cmc.Core.Xrm.ServerExtension.Core;
using Cmc.Core.Xrm.ServerExtension.Logging;
using Cmc.Engage.Common;
using Cmc.Engage.Common.Plugins.Tests.Utilities;
using Cmc.Engage.Models.Tests;
using FakeItEasy;
using FakeXrmEasy;
using FakeXrmEasy.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Moq;
using cmc_attributenametobeassigned = Cmc.Engage.Models.cmc_attributenametobeassigned;
using cmc_list_cmc_marketinglisttype = Cmc.Engage.Models.cmc_list_cmc_marketinglisttype;
using cmc_runassignmentforentity = Cmc.Engage.Models.cmc_runassignmentforentity;
using Contact = Cmc.Engage.Models.Contact;
using list_createdfromcode = Cmc.Engage.Models.list_createdfromcode;
using ListState = Cmc.Engage.Models.ListState;
using SystemUser = Cmc.Engage.Models.SystemUser;

namespace Cmc.Engage.Lifecycle.Tests.DomMaster.Plugin
{

    [TestClass]
    public class ValidateDomMasterPluginTest : XrmUnitTestBase
    {
        [TestCategory("Plugin"), TestCategory("Positive")]
        [TestMethod]
        public void ValidateDomMasterPlugin_DomMasterService_Dommaster_Create()
        {
            #region ARRANGE
            var marketingListInstance = PreparemarketlistInstance();
            var domMasterInstance = PrepareDomMasterInstance1(marketingListInstance);
            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>()
            {
                marketingListInstance,
                domMasterInstance,
            });
            var mockServiceProvider = InitializeMockService(xrmFakedContext, domMasterInstance, Operation.Create);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            #endregion ARRANGE

            #region ACT
            var mockLogger = new Mock<ILogger>();
            var mockLanguageService = new Mock<ILanguageService>();
            var DOMService = new DomMasterService(mockLogger.Object, mockLanguageService.Object, xrmFakedContext.GetFakedOrganizationService());
            DOMService.ValidateDomMasterService(mockExecutionContext.Object);
            #endregion ACT

            #region ASSERT
            Assert.IsTrue(true);
            #endregion ASSERT

        }
        
        [TestCategory("Plugin"), TestCategory("Positive")]
        [TestMethod]
        public void ValidateDomMasterPlugin_DomMasterService_Dommaster_Update_AttributenametobeassignedOtheruserLookUp()
        {
            #region ARRANGE

            var xrmFakedContext = new XrmFakedContext();
            var calledId = Guid.NewGuid();
            xrmFakedContext.CallerId = new EntityReference("SystemUser", calledId);

            var systemUser= PrepareSystemUser(xrmFakedContext.CallerId);
            var contact = PrepareContact(systemUser);
            var marketingListInstance = PreparemarketlistInstance();
            var domMasterInstance = PrepareDomMasterInstance(marketingListInstance.Id);
            var preImage = PrepareDomMasterImage(domMasterInstance);
            var language = PrepareLanguage();
            var userSetting = PrepareUserSetting(systemUser);

            

            xrmFakedContext.Initialize(new List<Entity>()
            {
                //contact,
                language,
                marketingListInstance,
                domMasterInstance,
                userSetting,
                systemUser,
            });
            var mockServiceProvider = InitializeMockService(xrmFakedContext, domMasterInstance, Operation.Update);
            AddPreEntityImage(mockServiceProvider, "Target", preImage);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);

            var entityMetadata = new EntityMetadata() { LogicalName = contact.LogicalName, MetadataId = Guid.NewGuid() };


            var attributeMetadata = new LookupAttributeMetadata() { DisplayName = new Label() { UserLocalizedLabel = new LocalizedLabel("frFirstName", 1036) }, LogicalName = "ownerid", };
            
            xrmFakedContext.InitializeMetadata(entityMetadata);

            xrmFakedContext.AddRelationship("user_settings", new XrmFakedRelationship
            {
                RelationshipType = XrmFakedRelationship.enmFakeRelationshipType.OneToMany,
                Entity1LogicalName = "systemuser",
                Entity1Attribute = "systemuserid",
                Entity2LogicalName = "usersettings",
                Entity2Attribute = "systemuserid"

            });


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
            var mockLogger = new Mock<ILogger>();
            var mockOrganizationService = new Mock<IOrganizationService>();
            var mockLanguageService = new LanguageService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
            
            mockOrganizationService.Setup(r => r.Execute(It.IsAny<RetrieveMetadataChangesRequest>())).Returns(retrieveMetadataChangesResponse);
            var DOMService = new DomMasterService(mockLogger.Object, mockLanguageService, mockOrganizationService.Object);
            
            #endregion ACT

            #region ASSERT
            Assert.ThrowsException<InvalidPluginExecutionException>(()=>DOMService.ValidateDomMasterService(mockExecutionContext.Object));
            #endregion ASSERT

        }

        [TestCategory("Plugin"), TestCategory("Positive")]
        [TestMethod]
        public void ValidateDomMasterPlugin_DomMasterService_Dommaster_Update_AttributenametobeassignedRecordOwner()
        {
            #region ARRANGE

            var xrmFakedContext = new XrmFakedContext();
            var calledId = Guid.NewGuid();
            xrmFakedContext.CallerId = new EntityReference("SystemUser", calledId);

            var systemUser = PrepareSystemUser(xrmFakedContext.CallerId);
            var contact = PrepareContact(systemUser);
            var marketingListInstance = PreparemarketlistInstance();
            var domMasterInstance = PrepareDomMasterInstance1(marketingListInstance);
            var preImage = PrepareDomMasterImage(domMasterInstance);
            var language = PrepareLanguage();
            var userSetting = PrepareUserSetting(systemUser);



            xrmFakedContext.Initialize(new List<Entity>()
            {
                //contact,
                language,
                marketingListInstance,
                domMasterInstance,
                userSetting,
                systemUser,
            });

            var entityMetadata = new EntityMetadata() { LogicalName = contact.LogicalName, MetadataId = Guid.NewGuid() };
            var attributeMetadata = new StringAttributeMetadata() { DisplayName = new Label() { UserLocalizedLabel = new LocalizedLabel("frFirstName", 1036) }, LogicalName = "ownerid", };

            var mockServiceProvider = InitializeMockService(xrmFakedContext, domMasterInstance, Operation.Update);
            AddPreEntityImage(mockServiceProvider, "Target", preImage);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
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
            var mockLogger = new Mock<ILogger>();
            var mockOrganizationService = new Mock<IOrganizationService>();
            var mockLanguageService = new LanguageService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());

            mockOrganizationService.Setup(r => r.Execute(It.IsAny<RetrieveMetadataChangesRequest>())).Returns(retrieveMetadataChangesResponse);
            var DOMService = new DomMasterService(mockLogger.Object, mockLanguageService, mockOrganizationService.Object);
            DOMService.ValidateDomMasterService(mockExecutionContext.Object);
            #endregion ACT

            #region ASSERT
            Assert.IsTrue(true);
            #endregion ASSERT

        }

        [TestCategory("Plugin"), TestCategory("Positive")]
        [TestMethod]
        public void ValidateDomMasterPlugin_DomMasterService_Dommaster_ChangeRunAssignmentforentity()
        {
            #region ARRANGE
            var marketingListInstance = PreparemarketlistWithOption(true);
            var domMasterInstance = PrepareDomMasterWithOption(marketingListInstance.Id,true);
            var preImage = PrepareDomMasterImage2(domMasterInstance);
            var domDefinitionExecutionOrder = PrepareDomDefinitionExecutionOrder(domMasterInstance);
            var domDefinition = PrepareDomDefinition(domMasterInstance);
            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>()
            {
                marketingListInstance,
                domMasterInstance,
                domDefinitionExecutionOrder,
                domDefinition,
                preImage,

            });
            var mockServiceProvider = InitializeMockService(xrmFakedContext, domMasterInstance, Operation.Update);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            var entityMetadata = new EntityMetadata() { LogicalName = "Account", MetadataId = Guid.NewGuid() };
            var attributeMetadata = new StringAttributeMetadata() { DisplayName = new Label() { UserLocalizedLabel = new LocalizedLabel("frFirstName", 1036) }, LogicalName = "ownerid", };


            AddPreEntityImage(mockServiceProvider, "Target", preImage);
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
            var mockLogger = new Mock<ILogger>();
            var mockLanguageService = new LanguageService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());

            A.CallTo(() => xrmFakedContext.GetFakedOrganizationService().Execute(A<RetrieveMetadataChangesRequest>.Ignored)).Returns(retrieveMetadataChangesResponse);
            var DOMService = new DomMasterService(mockLogger.Object, mockLanguageService, xrmFakedContext.GetFakedOrganizationService());

            #endregion ACT

            #region ASSERT
            Assert.ThrowsException<InvalidPluginExecutionException>(() => DOMService.ValidateDomMasterService(mockExecutionContext.Object));
            #endregion ASSERT

        }
        
        [TestCategory("Plugin"), TestCategory("Negative")]
        [TestMethod]
        public void ValidateDomMasterPlugin_DomMasterService_Dommaster_MarketingListNull()
        {
            #region ARRANGE
            
            var domMasterInstance = PrepareDomMasterInstanceWithoutList();
            
            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>()
            {
                domMasterInstance

            });
            var mockServiceProvider = InitializeMockService(xrmFakedContext, domMasterInstance, Operation.Update);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);

            
            var optionMetadata = new OptionMetadata() {  Label = new Label("Account", 1033), Value = 175490000 };
            var optionMetadataCollection = new OptionMetadataCollection {optionMetadata};
            
            var retrieveAttributeResponse = new RetrieveAttributeResponse()
            {
                Results = new ParameterCollection
                {
                    { "EntityMetadata", optionMetadataCollection}
                }
            };
            
            A.CallTo(() => xrmFakedContext.GetFakedOrganizationService().Execute(A<RetrieveAttributeRequest>.Ignored)).Returns(retrieveAttributeResponse);
            #endregion ARRANGE

            #region ACT
            var mockLogger = new Mock<ILogger>();
            var mockLanguageService = new LanguageService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
            
            var DOMService = new DomMasterService(mockLogger.Object, mockLanguageService, xrmFakedContext.GetFakedOrganizationService());

            #endregion ACT

            #region ASSERT
            Assert.ThrowsException<InvalidPluginExecutionException>(() => DOMService.ValidateDomMasterService(mockExecutionContext.Object));
            #endregion ASSERT

        }

        [TestCategory("Plugin"), TestCategory("Negative")]
        [TestMethod]
        public void ValidateDomMasterPlugin_DomMasterService_Dommaster_ChangeRunAssignmentforInValidentity()
        {
            #region ARRANGE
            var marketingListInstance = Preparemarketlist_createdfromcode();
            var domMasterInstance = PrepareDomMasterWithOption(marketingListInstance.Id,false);

            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>()
            {
                marketingListInstance,
                domMasterInstance,
                
            });

            var mockServiceProvider = InitializeMockService(xrmFakedContext, domMasterInstance, Operation.Update);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);


            var entityMetadata = new[] {  new EntityMetadata() {DisplayName = new Label() { UserLocalizedLabel = new LocalizedLabel("Account", 1036) }, LogicalName = Account.EntityLogicalName, MetadataId = Guid.NewGuid() } };

            var metadataService = new MetadataService(xrmFakedContext.GetFakedOrganizationService());


            var retrieveAllEntitiesResponse = new RetrieveAllEntitiesResponse()
            {
                Results = new ParameterCollection
                {
                    { "EntityMetadata", entityMetadata}
                }
            };

            var optionMetadata = new OptionMetadata() { Label = new Label("Account", 1033), Value = 175490000 };
            var optionMetadataCollection = new OptionMetadataCollection { optionMetadata };

            var retrieveAttributeResponse = new RetrieveAttributeResponse()
            {
                Results = new ParameterCollection
                {
                    { "EntityMetadata", optionMetadataCollection}
                }
            };

            A.CallTo(() => xrmFakedContext.GetFakedOrganizationService().Execute(A<RetrieveAttributeRequest>.Ignored)).Returns(retrieveAttributeResponse);

            A.CallTo(() => xrmFakedContext.GetFakedOrganizationService().Execute(A<RetrieveAllEntitiesRequest>.Ignored)).Returns(retrieveAllEntitiesResponse);
            
            //A.CallTo(() => metadataService.GetEntityNameFromTypeCode(A<int>.Ignored,A<bool>.Ignored)).Returns("Account");
            #endregion ARRANGE

            #region ACT
            var mockLogger = new Mock<ILogger>();
            var mockLanguageService = new LanguageService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
            var DOMService = new DomMasterService(mockLogger.Object, mockLanguageService, xrmFakedContext.GetFakedOrganizationService());

            #endregion ACT

            #region ASSERT
            Assert.ThrowsException<InvalidPluginExecutionException>(() => DOMService.ValidateDomMasterService(mockExecutionContext.Object));
            #endregion ASSERT

        }

        [TestCategory("Plugin"), TestCategory("Negative")]
        [TestMethod]
        public void ValidateDomMasterPlugin_NegativeScenario()
        {
            #region ARRANGE

          var xrmFakedContext = new XrmFakedContext();

            #endregion
            #region ACT
            var mockLogger = new Mock<ILogger>();
            var mockLanguageService = new LanguageService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());

          #endregion ACT

            #region ASSERT
            Assert.ThrowsException<ArgumentNullException>(() => new DomMasterService(null, mockLanguageService,xrmFakedContext.GetFakedOrganizationService()));
            Assert.ThrowsException<ArgumentException>(() => new DomMasterService(mockLogger.Object, null,xrmFakedContext.GetFakedOrganizationService()));
            #endregion ASSERT

        }


        private Entity PreparemarketlistInstance()
        {
            var marketingListInstance = new Entity("List", Guid.NewGuid())
            {
                ["cmc_marketinglisttype"] = cmc_list_cmc_marketinglisttype.StudentGroup,
                ["cmc_expirationdate"] = DateTime.Today.AddDays(-1).Date,
                ["createdfromcode"] = new OptionSetValue(2),
                ["statecode"] = ListState.Active
            };
            return marketingListInstance;
        }
        private Entity PrepareDomMasterInstance1(Entity marketingList)
        {
            var instance = new Entity("cmc_dommaster", Guid.NewGuid())
            {
                ["cmc_marketinglistid"] = marketingList.ToEntityReference(),
                ["cmc_runassignmentforentity"] = new OptionSetValue((int)cmc_runassignmentforentity.Contact),
                ["cmc_attributenametobeassigned"] = new OptionSetValue((int)cmc_attributenametobeassigned.RecordOwner),
                ["createdfromcode"] = new OptionSetValue(2)
            };

            return instance;
        }
        private Entity PrepareDomMasterInstance(Guid marketingListInstanceId)
        {
            var instance = new Entity("cmc_dommaster", Guid.NewGuid())
            {
                ["cmc_marketinglistid"] = new EntityReference("List", marketingListInstanceId),
                ["cmc_runassignmentforentity"] = new OptionSetValue((int)cmc_runassignmentforentity.Contact),
                ["cmc_attributenametobeassigned"] = new OptionSetValue((int)cmc_attributenametobeassigned.OtherUserLookup),
                ["cmc_otheruserlookup"] = "contact.ownerid",
                ["createdfromcode"] = new OptionSetValue(2)
            };

            return instance;
        }        
        private Contact PrepareContact(Entity systemUserEntity)
        {
            return new Contact()
            {
                Id = Guid.NewGuid(),
                OwnerId = systemUserEntity.ToEntityReference()
            };
        }
        private SystemUser PrepareSystemUser(EntityReference callerId)
        {
            return new SystemUser()
            {
                Id = callerId.Id
            };
        }
        private UserSettings PrepareUserSetting(Entity systemUser)
        {
            return new UserSettings()
            {
                Id = Guid.NewGuid(),
                UILanguageId = 1033,
                ["systemuserid"] = systemUser.ToEntityReference()
            };
        }
        private Entity PrepareDomMasterImage(Entity domMasterEntity)
        {
            return new Entity("cmc_dommaster", domMasterEntity.Id)
            {
                ["cmc_marketinglistid"] = domMasterEntity.GetAttributeValue<EntityReference>("cmc_marketinglistid"),
                ["cmc_runassignmentforentity"] = domMasterEntity.GetAttributeValue<OptionSetValue>("cmc_runassignmentforentity"),
                ["cmc_attributenametobeassigned"] = domMasterEntity.GetAttributeValue<OptionSetValue>("cmc_attributenametobeassigned"),
                ["createdfromcode"] = "contact.ownerid",
            };
        }
        private cmc_languagevalue PrepareLanguage()
        {
            return new cmc_languagevalue()
            {
                Id = Guid.NewGuid(),
                cmc_keyname = "Invalid_Other_User_Lookup",
                cmc_value = "{0} is not a valid field for {1}. Please make sure this field exists as an updatable User lookup.",
                cmc_languagecode = 1033
            };
        }

        private cmc_dommaster PrepareDomMasterInstanceWithoutList()
        {
            var instance = new cmc_dommaster()
            {
                Id = Guid.NewGuid(),
                cmc_runassignmentforentity = new OptionSetValue((int)cmc_runassignmentforentity.Contact),
                cmc_attributenametobeassigned = new OptionSetValue((int)cmc_attributenametobeassigned.OtherUserLookup),
                ["cmc_otheruserlookup"] = "contact.ownerid",
                ["createdfromcode"] = new OptionSetValue(2)
            };

            return instance;
        }
        
        private Entity PrepareDomMasterImage2(Entity domMasterEntity)
        {
            return new Entity("cmc_dommaster", domMasterEntity.Id)
            {
                ["cmc_marketinglistid"] = domMasterEntity.GetAttributeValue<EntityReference>("cmc_marketinglistid"),
                ["cmc_runassignmentforentity"] = new OptionSetValue((int)cmc_runassignmentforentity.Contact),
                ["cmc_attributenametobeassigned"] = domMasterEntity.GetAttributeValue<OptionSetValue>("cmc_attributenametobeassigned"),
                ["createdfromcode"] = "contact.ownerid",
            };
        }

        private Entity PrepareDomDefinitionExecutionOrder(Entity domMasterEntity)
        {
            return new Entity("cmc_domdefinitionexecutionorder", domMasterEntity.Id)
            {
                ["cmc_dommasterid"] = domMasterEntity.ToEntityReference(),
                
            };
        }
        private Entity PrepareDomDefinition(Entity domMasterEntity)
        {
            return new Entity("cmc_domdefinition", domMasterEntity.Id)
            {
                ["cmc_dommasterid"] = domMasterEntity.ToEntityReference(),

            };
        }
        private Entity PrepareDomMasterWithOption(Guid marketingListInstanceId,bool option)
        {
            var instance = new Entity("cmc_dommaster", Guid.NewGuid())
            {
                ["cmc_marketinglistid"] = new EntityReference("List", marketingListInstanceId),
                ["cmc_runassignmentforentity"] = option ? new OptionSetValue((int)cmc_runassignmentforentity.Account) : new OptionSetValue((int)cmc_runassignmentforentity.InboundInterest),
                ["cmc_attributenametobeassigned"] = new OptionSetValue((int)cmc_attributenametobeassigned.OtherUserLookup),
                ["cmc_otheruserlookup"] = "contact.ownerid",
                ["createdfromcode"] = new OptionSetValue(2)
            };

            return instance;
        }

        private Entity PreparemarketlistWithOption(bool option)
        {
            var marketingListInstance = new Entity("List", Guid.NewGuid())
            {
                ["cmc_marketinglisttype"] = cmc_list_cmc_marketinglisttype.StudentGroup,
                ["cmc_expirationdate"] = DateTime.Today.AddDays(-1).Date,
                ["createdfromcode"] = option ? new OptionSetValue(1) : new OptionSetValue(4),
                ["statecode"] = ListState.Active
            };
            return marketingListInstance;
        }

        private Entity Preparemarketlist_createdfromcode()
        {
            var marketingListInstance = new Entity("List", Guid.NewGuid())
            {
                ["cmc_marketinglisttype"] = cmc_list_cmc_marketinglisttype.StudentGroup,
                ["cmc_expirationdate"] = DateTime.Today.AddDays(-1).Date,
                ["statecode"] = ListState.Active
            };
            return marketingListInstance;
        }
    }
    
}
