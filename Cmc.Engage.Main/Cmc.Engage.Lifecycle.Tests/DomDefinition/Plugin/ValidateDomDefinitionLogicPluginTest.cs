using System;
using System.Collections.Generic;
using System.Reflection;
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
    [TestClass]
    public class ValidateDomDefinitionLogicPluginTest : XrmUnitTestBase
    {
        [TestCategory("Plugin"), TestCategory("Positive")]
        [TestMethod]
        public void ValidateDomDefinitionLogicPlugin_CreateDomDefinitionLogic_Create()
        {
            #region ARRANGE
            var contact = PrepareContactInstance();
            var marketingList = PrepareMarketingListInstance();
            var domMaster = PrepareCmcDomMasterInstance(marketingList.Id);
            var domDefinition = PrepareCmcDomDefinitionInstance(domMaster.Id);
            var domDefinitionLogic = PrepareCmcDomDefinitionLogicInstance(domDefinition.Id);
            var xrmFakedContext = new XrmFakedContext { ProxyTypesAssembly = Assembly.Load("Cmc.Engage.Contracts.Tests") };

            xrmFakedContext.AddRelationship("mshied_academicperiod_contact_CurrentAcademicPeriod", new XrmFakedRelationship
            {
                RelationshipType = XrmFakedRelationship.enmFakeRelationshipType.OneToMany,
                Entity1LogicalName = "contact",
                Entity1Attribute = "mshied_currentacademicperiodid",
                Entity2LogicalName = "mshied_academicperiod",
                Entity2Attribute = "mshied_academicperiodid"
            });
            xrmFakedContext.Initialize(new List<Entity>()
            {
                contact,
                domMaster,
                marketingList,
                domDefinition,
                domDefinitionLogic
            });

            var entityCollection = new EntityCollection();
            entityCollection.Entities.Add(domDefinition);

            var entityMetadata = new EntityMetadata() { LogicalName = contact.LogicalName, MetadataId = Guid.NewGuid() };
            var attributeMetadata = new AttributeMetadata() { LogicalName = "firstname" };
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

            //Initialize the Mock Service
            var mockServiceProvider = InitializeMockService(xrmFakedContext, domDefinitionLogic, Operation.Create);
            //Fetch the Mock Execution Context
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            #endregion

            #region ACT
            var mockLogger = new Mock<ILogger>();
            var mockLanguageService = new Mock<ILanguageService>();
            A.CallTo(() => xrmFakedContext.GetFakedOrganizationService().Execute(A<RetrieveMetadataChangesRequest>._)).Returns(retrieveMetadataChangesResponse);
            var mockDomDefinitionLogic = new DomDefinitionLogicService(mockLogger.Object, mockLanguageService.Object,
                xrmFakedContext.GetFakedOrganizationService());
            mockDomDefinitionLogic.ValidateDomDefinitionLogic(mockExecutionContext.Object);
            #endregion

            #region ASSERT
            Assert.IsTrue(true);
            #endregion ASSERT

        }

        [TestCategory("Plugin"), TestCategory("Positive")]
        [TestMethod]
        public void ValidateDomDefinitionLogicPlugin_CreateDomDefinitionLogic_Create_ManytoMany()
        {
            #region ARRANGE
            var contact = PrepareContactInstance();            
            var account = GetAccount();
            var lead = GetLead();
            var marketingList = PrepareMarketingListInstance();
            var domMaster = PrepareCmcDomMasterInstance1(marketingList.Id);
            var domDefinition = PrepareCmcDomDefinitionInstance(domMaster.Id);
            var domDefinitionLogic = PrepareCmcDomDefinitionLogicInstance1(domDefinition.Id);
            var xrmFakedContext = new XrmFakedContext { ProxyTypesAssembly = Assembly.Load("Cmc.Engage.Contracts.Tests") };

            xrmFakedContext.AddRelationship("accountleads_association", new XrmFakedRelationship
            {
                RelationshipType = XrmFakedRelationship.enmFakeRelationshipType.ManyToMany,
                Entity1LogicalName = "account",
                Entity1Attribute = "leadid",
                Entity2LogicalName = "lead",
                Entity2Attribute = "accountid",
                IntersectEntity = "accountleads"
            });
            var retrieveRequest = new RetrieveRelationshipRequest()
            {
                Name = "accountleads_association",
                RequestId = Guid.NewGuid()
            };

            var fakeRelationShip = xrmFakedContext.GetRelationship(retrieveRequest.Name);
            var response = new RetrieveRelationshipResponse();
            response.Results.Add("RelationshipMetadata", GetRelationshipMetadata(fakeRelationShip));
            A.CallTo(() => xrmFakedContext.GetFakedOrganizationService().Execute(A<RetrieveRelationshipRequest>._)).Returns(response);
            xrmFakedContext.Initialize(new List<Entity>()
            {
                contact,
                account,
                lead,
                domMaster,
                marketingList,
                domDefinition,
                domDefinitionLogic
            });

            var entityCollection = new EntityCollection();
            entityCollection.Entities.Add(domDefinition);
            
            var entityMetadata = new EntityMetadata() { LogicalName = lead.LogicalName, MetadataId = Guid.NewGuid() };
            var attributeMetadata = new AttributeMetadata() { LogicalName = "firstname" };
            
            xrmFakedContext.InitializeMetadata(entityMetadata);
            entityMetadata.SetAttributeCollection(new List<AttributeMetadata>() { attributeMetadata });
            var entityMetadataCollection = new EntityMetadataCollection();
            entityMetadataCollection.Add(entityMetadata);
            entityMetadata.SetSealedPropertyValue("PrimaryIdAttribute", "leadid");
            var retrieveMetadataChangesResponse = new RetrieveMetadataChangesResponse()
            {
                Results = new ParameterCollection
                        {
                            { "EntityMetadata", entityMetadataCollection}
                        }
            };
            var retrieveEntityResponse = new RetrieveEntityResponse()
            {
                Results = new ParameterCollection
                        {
                           { "EntityMetadata", entityMetadata},

                        }
            };

            A.CallTo(() => xrmFakedContext.GetFakedOrganizationService().Execute(A<RetrieveEntityRequest>._)).Returns(retrieveEntityResponse);
            var entityMetadata2 = new EntityMetadata() { LogicalName = account.LogicalName, MetadataId = Guid.NewGuid() };
            entityMetadata2.SetSealedPropertyValue("PrimaryIdAttribute", "accountid");
            var retrieveEntityResponse2 = new RetrieveEntityResponse()
            {
                Results = new ParameterCollection
                        {
                           { "EntityMetadata", entityMetadata2},
                    
                        }
            };
            
            A.CallTo(() => xrmFakedContext.GetFakedOrganizationService().Execute(A<RetrieveEntityRequest>._)).Returns(retrieveEntityResponse2);

            //Initialize the Mock Service
            var mockServiceProvider = InitializeMockService(xrmFakedContext, domDefinitionLogic, Operation.Create);
            //Fetch the Mock Execution Context
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            #endregion


            #region ACT
            var mockLogger = new Mock<ILogger>();
            var mockLanguageService = new Mock<ILanguageService>();
            A.CallTo(() => xrmFakedContext.GetFakedOrganizationService().Execute(A<RetrieveMetadataChangesRequest>._)).Returns(retrieveMetadataChangesResponse);
            var mockDomDefinitionLogic = new DomDefinitionLogicService(mockLogger.Object, mockLanguageService.Object,
                xrmFakedContext.GetFakedOrganizationService());
            mockDomDefinitionLogic.ValidateDomDefinitionLogic(mockExecutionContext.Object);
            #endregion

            #region ASSERT
            Assert.IsTrue(true);
            #endregion ASSERT
        }

        [TestCategory("Plugin"), TestCategory("Positive")]
        [TestMethod]
        public void ValidateDomDefinitionLogicPlugin_CreateDomDefinitionLogic_Create_ManytoMany_ElseIfPart()
        {
            #region ARRANGE
            var contact = PrepareContactInstance();
            var account = GetAccount();
            var lead = GetLead();
            var marketingList = PrepareMarketingListInstance();
            var domMaster = PrepareCmcDomMasterInstance1(marketingList.Id);
            var domDefinition = PrepareCmcDomDefinitionInstance(domMaster.Id);
            var domDefinitionLogic = PrepareCmcDomDefinitionLogicInstance1(domDefinition.Id);
            var xrmFakedContext = new XrmFakedContext { ProxyTypesAssembly = Assembly.Load("Cmc.Engage.Contracts.Tests") };

            xrmFakedContext.AddRelationship("accountleads_association", new XrmFakedRelationship
            {
                RelationshipType = XrmFakedRelationship.enmFakeRelationshipType.ManyToMany,                
                Entity1Attribute = "accountid",
                Entity1LogicalName = "lead",
                Entity2Attribute = "leadid",
                Entity2LogicalName = "account",
                IntersectEntity = "accountleads"
            });
            var retrieveRequest = new RetrieveRelationshipRequest()
            {
                Name = "accountleads_association",
                RequestId = Guid.NewGuid()
            };

            var fakeRelationShip = xrmFakedContext.GetRelationship(retrieveRequest.Name);
            var response = new RetrieveRelationshipResponse();
            response.Results.Add("RelationshipMetadata", GetRelationshipMetadata(fakeRelationShip));
            A.CallTo(() => xrmFakedContext.GetFakedOrganizationService().Execute(A<RetrieveRelationshipRequest>._)).Returns(response);
            xrmFakedContext.Initialize(new List<Entity>()
            {
                contact,               
                account,
                lead,
                domMaster,
                marketingList,
                domDefinition,
                domDefinitionLogic
            });

            var entityCollection = new EntityCollection();
            entityCollection.Entities.Add(domDefinition);


            var entityMetadata = new EntityMetadata() { LogicalName = account.LogicalName, MetadataId = Guid.NewGuid() };
            var attributeMetadata = new AttributeMetadata() { LogicalName = "firstname" };

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
            var entityMetadata2 = new EntityMetadata() { LogicalName = lead.LogicalName, MetadataId = Guid.NewGuid() };
            entityMetadata2.SetSealedPropertyValue("PrimaryIdAttribute", "leadid");
            var retrieveEntityResponse = new RetrieveEntityResponse()
            {
                Results = new ParameterCollection
                        {
                           { "EntityMetadata", entityMetadata2},

                        }
            };

            A.CallTo(() => xrmFakedContext.GetFakedOrganizationService().Execute(A<RetrieveEntityRequest>._)).Returns(retrieveEntityResponse);

            //Initialize the Mock Service
            var mockServiceProvider = InitializeMockService(xrmFakedContext, domDefinitionLogic, Operation.Create);
            //Fetch the Mock Execution Context
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            #endregion


            #region ACT
            var mockLogger = new Mock<ILogger>();
            var mockLanguageService = new Mock<ILanguageService>();
            A.CallTo(() => xrmFakedContext.GetFakedOrganizationService().Execute(A<RetrieveMetadataChangesRequest>._)).Returns(retrieveMetadataChangesResponse);
            var mockDomDefinitionLogic = new DomDefinitionLogicService(mockLogger.Object, mockLanguageService.Object,
                xrmFakedContext.GetFakedOrganizationService());
            mockDomDefinitionLogic.ValidateDomDefinitionLogic(mockExecutionContext.Object);
            #endregion

            #region ASSERT
            Assert.IsTrue(true);
            #endregion ASSERT
        }

        [TestCategory("Plugin"), TestCategory("Negative")]
        [TestMethod]
        public void ValidateDomDefinitionLogicPlugin_CreateDomDefinitionLogic_Create_ManytoMany_ElsePart()
        {
            //When relationship type is neither one-to-many nor many-to-many
            #region ARRANGE
            var contact = PrepareContactInstance();
            var successplanassignment = PrepareSuccessPlanAssignment();
            var marketingList = PrepareMarketingListInstance();
            var domMaster = PrepareCmcDomMasterInstance(marketingList.Id);
            var domDefinition = PrepareCmcDomDefinitionInstance(domMaster.Id);
            var domDefinitionLogic = PrepareCmcDomDefinitionLogicInstance2(domDefinition.Id);
            var xrmFakedContext = new XrmFakedContext { ProxyTypesAssembly = Assembly.Load("Cmc.Engage.Contracts.Tests") };

            xrmFakedContext.AddRelationship("cmc_contact_successplanassignment", new XrmFakedRelationship
            {
                Entity1Attribute = "contactid",
                Entity1LogicalName = "contact",
                Entity2Attribute = "cmc_successplanassignmentid",
                Entity2LogicalName = "cmc_successplanassignment",
                IntersectEntity = "cmc_contact_successplanassignment"
            });
            var retrieveRequest = new RetrieveRelationshipRequest()
            {
                Name = "cmc_contact_successplanassignment",
                RequestId = Guid.NewGuid()
            };

            var response = new RetrieveRelationshipResponse();
            
            A.CallTo(() => xrmFakedContext.GetFakedOrganizationService().Execute(A<RetrieveRelationshipRequest>._)).Returns(response);
            xrmFakedContext.Initialize(new List<Entity>()
            {
                contact,
                successplanassignment,
                domMaster,
                marketingList,
                domDefinition,
                domDefinitionLogic
            });

            var entityCollection = new EntityCollection();
            entityCollection.Entities.Add(domDefinition);


            var entityMetadata = new EntityMetadata() { LogicalName = contact.LogicalName, MetadataId = Guid.NewGuid() };
            var attributeMetadata = new AttributeMetadata() { LogicalName = "firstname" };
            
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
            var entityMetadata2 = new EntityMetadata() { LogicalName = successplanassignment.LogicalName, MetadataId = Guid.NewGuid() };
            entityMetadata2.SetSealedPropertyValue("PrimaryIdAttribute", "successplanassignment");
            var retrieveEntityResponse = new RetrieveEntityResponse()
            {
                Results = new ParameterCollection
                        {
                           { "EntityMetadata", entityMetadata2},

                        }
            };

            A.CallTo(() => xrmFakedContext.GetFakedOrganizationService().Execute(A<RetrieveEntityRequest>._)).Returns(retrieveEntityResponse);

            //Initialize the Mock Service
            var mockServiceProvider = InitializeMockService(xrmFakedContext, domDefinitionLogic, Operation.Create);
            //Fetch the Mock Execution Context
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            #endregion


            #region ACT
            
            var mockLogger = new Mock<ILogger>();
            var mockLanguageService = new LanguageService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
            
            A.CallTo(() => xrmFakedContext.GetFakedOrganizationService().Execute(A<RetrieveMetadataChangesRequest>._)).Returns(retrieveMetadataChangesResponse);
            var mockDomDefinitionLogic = new DomDefinitionLogicService(mockLogger.Object, mockLanguageService,
                xrmFakedContext.GetFakedOrganizationService());

            #endregion

            #region ASSERT
            Assert.ThrowsException<InvalidPluginExecutionException>(() => mockDomDefinitionLogic.ValidateDomDefinitionLogic(mockExecutionContext.Object));
            #endregion ASSERT
        }

        [TestCategory("Plugin"), TestCategory("Positive")]
        [TestMethod]
        public void ValidateDomDefinitionLogicPlugin_CreateDomDefinitionLogic_AttributeType_Update()
        {
            #region ARRANGE
            var contact = PrepareContactInstance();
            var marketingList = PrepareMarketingListInstance();
            var domMaster = PrepareCmcDomMasterInstance(marketingList.Id);
            var domDefinition = PrepareCmcDomDefinitionInstance(domMaster.Id);
            var domDefinitionLogic = PrepareCmcDomDefinitionLogicInstance(domDefinition.Id);
            var domDefinitionLogicRcrdOwnr = PrepareCmcDomDefinitionLogicInstance_RecrdOwnr(domDefinition.Id);
            var domDefinitionLogicValue = PrepareCmcDomDefinitionLogicInstance_Value(domDefinition.Id);

            Dictionary<string, cmc_domdefinitionlogic> cmc_Domdefinitionlogics = new Dictionary<string, cmc_domdefinitionlogic>()
            {
               {"domDefinitionLogicBoolean",domDefinitionLogic},{"domDefinitionLogicPicklist",domDefinitionLogic},{"domDefinitionLogicStatus",domDefinitionLogic}, { "domDefinitionLogicState",domDefinitionLogic},{ "domDefinitionLogicRcrdOwnr",domDefinitionLogicRcrdOwnr},{ "domDefinitionLogicValue",domDefinitionLogicValue}
            };

            var xrmFakedContext = new XrmFakedContext { ProxyTypesAssembly = Assembly.Load("Cmc.Engage.Contracts.Tests") };


            xrmFakedContext.AddRelationship("mshied_academicperiod_contact_CurrentAcademicPeriod", new XrmFakedRelationship
            {
                RelationshipType = XrmFakedRelationship.enmFakeRelationshipType.OneToMany,
                Entity1LogicalName = "contact",
                Entity1Attribute = "mshied_currentacademicperiodid",
                Entity2LogicalName = "mshied_academicperiod",
                Entity2Attribute = "mshied_academicperiodid"
            });
            xrmFakedContext.Initialize(new List<Entity>
            {
                contact,
                domMaster,
                marketingList,
                domDefinition,
                domDefinitionLogicValue,
                domDefinitionLogic,
                domDefinitionLogicRcrdOwnr

            });
            foreach (var item in cmc_Domdefinitionlogics)
            {
                var preImage = PreparePreImageDomDefinitionEntityCmcValue(item.Value, domDefinition);
                var mockServiceProvider = InitializeMockService(xrmFakedContext, item.Value, Operation.Update);
                var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
                AddPreEntityImage(mockServiceProvider, "PreImage", preImage);
                #endregion

                #region ACT

                var entityMetadata = new EntityMetadata() { LogicalName = contact.LogicalName, MetadataId = Guid.NewGuid() };
                if (item.Key == "domDefinitionLogicState")
                {
                    var attributeMetadata = new StateAttributeMetadata() { DisplayName = new Label() { UserLocalizedLabel = new LocalizedLabel("frFirstName", 1036) }, LogicalName = "firstname", OptionSet = new OptionSetMetadata() { Options = { new OptionMetadata(new Label("2018", 2018), null) } } };
                    entityMetadata.SetAttributeCollection(new List<AttributeMetadata>() { attributeMetadata });
                }
                if (item.Key == "domDefinitionLogicStatus")
                {
                    var attributeMetadata = new StatusAttributeMetadata() { DisplayName = new Label() { UserLocalizedLabel = new LocalizedLabel("frFirstName", 1036) }, LogicalName = "firstname", OptionSet = new OptionSetMetadata() { Options = { new OptionMetadata(new Label("2018", 2018), null) } } };
                    entityMetadata.SetAttributeCollection(new List<AttributeMetadata>() { attributeMetadata });
                }
                if (item.Key == "domDefinitionLogicPicklist")
                {
                    var attributeMetadata = new PicklistAttributeMetadata() { DisplayName = new Label() { UserLocalizedLabel = new LocalizedLabel("frFirstName", 1036) }, LogicalName = "firstname", OptionSet = new OptionSetMetadata() { Options = { new OptionMetadata(new Label("2018", 2018), null) } } };
                    entityMetadata.SetAttributeCollection(new List<AttributeMetadata>() { attributeMetadata });
                }
                if (item.Key == "domDefinitionLogicBoolean")
                {
                    var attributeMetadata = new BooleanAttributeMetadata() { LogicalName = "firstname", OptionSet = new BooleanOptionSetMetadata(new OptionMetadata(new Label("20181", 0), 0), new OptionMetadata(new Label("2018", 1), 1)) };
                    entityMetadata.SetAttributeCollection(new List<AttributeMetadata>() { attributeMetadata });
                }

                if (item.Key == "domDefinitionLogicRcrdOwnr")
                {
                    var attributeMetadata = new LookupAttributeMetadata() { DisplayName = new Label() { UserLocalizedLabel = new LocalizedLabel("frFirstName", 1036) }, LogicalName = "firstname", Targets = new string[] { contact.LogicalName }, LinkedAttributeId = Guid.NewGuid() };
                    entityMetadata.SetAttributeCollection(new List<LookupAttributeMetadata>() { attributeMetadata });

                    var entityMetadata2 = new EntityMetadata() { LogicalName = contact.LogicalName, MetadataId = Guid.NewGuid() };
                    var ownerAttributeMetadata = new OwnerAttributeMetadata();
                    entityMetadata2.SetAttributeCollection(new List<OwnerAttributeMetadata>() { ownerAttributeMetadata });
                    var retrieveEntityResponse = new RetrieveEntityResponse()
                    {
                        Results = new ParameterCollection
                        {
                           { "EntityMetadata", entityMetadata2}
                        }
                    };
                    A.CallTo(() => xrmFakedContext.GetFakedOrganizationService().Execute(A<RetrieveEntityRequest>._)).Returns(retrieveEntityResponse);
                }

                if (item.Key == "domDefinitionLogicValue")
                {
                    var attributeMetadata = new DateTimeAttributeMetadata() { DisplayName = new Label() { UserLocalizedLabel = new LocalizedLabel("frFirstName", 1036) }, LogicalName = "firstname" };
                    entityMetadata.SetAttributeCollection(new List<DateTimeAttributeMetadata>() { attributeMetadata });
                }

                var entityMetadataCollection = new EntityMetadataCollection();
                entityMetadataCollection.Add(entityMetadata);
                RetrieveMetadataChangesResponse retrieveMetadataChangesResponse = new RetrieveMetadataChangesResponse()
                {
                    Results = new ParameterCollection
                        {
                           { "EntityMetadata", entityMetadataCollection}

                        }
                };

                var mockLogger = new Mock<ILogger>();
                var mockLanguageService = new LanguageService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
                A.CallTo(() => xrmFakedContext.GetFakedOrganizationService().Execute(A<RetrieveMetadataChangesRequest>._)).Returns(retrieveMetadataChangesResponse);
                var mockDomService = new DomDefinitionLogicService(mockLogger.Object, mockLanguageService,
                xrmFakedContext.GetFakedOrganizationService());
                mockDomService.ValidateDomDefinitionLogic(mockExecutionContext.Object);
                #endregion

                #region ASSERT
                Assert.IsTrue(true);
                #endregion ASSERT  
            }
        }

        [TestCategory("Plugin"), TestCategory("Positive")]
        [TestMethod]
        public void ValidateDomDefinitionLogicPlugin_CreateMultipleDomDefinitionLogic_Update()
        {
            #region ARRANGE
            var contact = PrepareContactInstance();
            var marketingList = PrepareMarketingListInstance();
            var domMaster = PrepareCmcDomMasterInstance(marketingList.Id);
            var domDefinition = PrepareCmcDomDefinitionInstance(domMaster.Id);
            var domDefinitionLogicBegnWth = PrepareCmcDomDefinitionLogic_BeginWith(domDefinition.Id);
            var domDefinitionLogicEqual = PrepareCmcDomDefinitionLogicInstance_Equal(domDefinition.Id);
            var domDefinitionLogicRange = PrepareCmcDomDefinitionLogicInstance_Range(domDefinition.Id);
            List<cmc_domdefinitionlogic> cmc_Domdefinitionlogics = new List<cmc_domdefinitionlogic>() { domDefinitionLogicBegnWth, domDefinitionLogicEqual, domDefinitionLogicRange };
            var xrmFakedContext = new XrmFakedContext { ProxyTypesAssembly = Assembly.Load("Cmc.Engage.Contracts.Tests") };


            xrmFakedContext.AddRelationship("mshied_academicperiod_contact_CurrentAcademicPeriod", new XrmFakedRelationship
            {
                RelationshipType = XrmFakedRelationship.enmFakeRelationshipType.OneToMany,
                Entity1LogicalName = "contact",
                Entity1Attribute = "mshied_currentacademicperiodid",
                Entity2LogicalName = "mshied_academicperiod",
                Entity2Attribute = "mshied_academicperiodid"
            });
            xrmFakedContext.Initialize(new List<Entity>
            {
                contact,
                domMaster,
                marketingList,
                domDefinition,
                domDefinitionLogicBegnWth,
                domDefinitionLogicEqual,
                domDefinitionLogicRange
            });
            foreach (var item in cmc_Domdefinitionlogics)
            {
                var preImage = PreparePreImageDomDefinitionEntityCmcValue(item, domDefinition);
                var mockServiceProvider = InitializeMockService(xrmFakedContext, item, Operation.Update);
                var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
                AddPreEntityImage(mockServiceProvider, "PreImage", preImage);
                #endregion

                #region ACT
                var mockLogger = new Mock<ILogger>();
                var mockILanguageService = new Mock<ILanguageService>();
                var entityMetadata = new EntityMetadata() { LogicalName = contact.LogicalName, MetadataId = Guid.NewGuid() };
                var attributeMetadata = new AttributeMetadata() { DisplayName = new Label() { UserLocalizedLabel = new LocalizedLabel("frFirstName", 1036) }, LogicalName = "firstname" };
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
                A.CallTo(() => xrmFakedContext.GetFakedOrganizationService().Execute(A<RetrieveMetadataChangesRequest>._)).Returns(retrieveMetadataChangesResponse);
                var mockDomService = new DomDefinitionLogicService(mockLogger.Object, mockILanguageService.Object,
                    xrmFakedContext.GetFakedOrganizationService());
                mockDomService.ValidateDomDefinitionLogic(mockExecutionContext.Object);
                #endregion

                #region ASSERT
                Assert.IsTrue(true);
                #endregion ASSERT  
            }
        }

        [TestCategory("Plugin"), TestCategory("Negative")]
        [TestMethod]
        public void ValidateDomDefinitionLogicPlugin_CreateDomDefinitionLogic_CreateForExceptipn()
        {
            #region ARRANGE

           XrmFakedContext xrmFakedContext = new XrmFakedContext();

            #endregion

            #region ACT
            var mockLogger = new Mock<ILogger>();
            var mockLanguageService = new Mock<ILanguageService>();
         
           
            #endregion

            #region ASSERT
            Assert.ThrowsException<ArgumentNullException>(() => new DomDefinitionLogicService(null, null, null));
            Assert.ThrowsException<ArgumentException>(() => new DomDefinitionLogicService(mockLogger.Object, null, xrmFakedContext.GetFakedOrganizationService()));
           
            #endregion ASSERT

        }

        private static object GetRelationshipMetadata(XrmFakedRelationship fakeRelationShip)
        {
            if (fakeRelationShip.RelationshipType == XrmFakedRelationship.enmFakeRelationshipType.ManyToMany)
            {
                var mtm = new ManyToManyRelationshipMetadata();
                mtm.Entity1LogicalName = fakeRelationShip.Entity1LogicalName;
                mtm.Entity1IntersectAttribute = fakeRelationShip.Entity1Attribute;
                mtm.Entity2LogicalName = fakeRelationShip.Entity2LogicalName;
                mtm.Entity2IntersectAttribute = fakeRelationShip.Entity2Attribute;
                mtm.SchemaName = fakeRelationShip.IntersectEntity;
                mtm.IntersectEntityName = fakeRelationShip.IntersectEntity.ToLower();
                return mtm;
            }
            else
            {
                var otm = new OneToManyRelationshipMetadata();
                otm.ReferencedEntityNavigationPropertyName = fakeRelationShip.IntersectEntity;
                otm.ReferencingAttribute = fakeRelationShip.Entity1Attribute;
                otm.ReferencingEntity = fakeRelationShip.Entity1LogicalName;
                otm.ReferencedAttribute = fakeRelationShip.Entity2Attribute;
                otm.ReferencedEntity = fakeRelationShip.Entity2LogicalName;
                otm.SchemaName = fakeRelationShip.IntersectEntity;
                return otm;
            }
        }
        private Account GetAccount()
        {
            return new Account()
            {
                Name = "Test Account",
                Id = Guid.NewGuid()
            };
        }
        private Lead GetLead()
        {
            return new Lead()
            {
                Id = Guid.NewGuid(),
                FirstName = "Test lead"
            };
        }
        private cmc_domdefinitionlogic PrepareCmcDomDefinitionLogicInstance1(Guid cmcDomDefinition)
        {
            var domDefinitionLogicInstance = new cmc_domdefinitionlogic()
            {
                Id = Guid.NewGuid(),
                cmc_domdefinitionid = new EntityReference("cmc_domdefinition", cmcDomDefinition),
                cmc_attributeschema = "account.accountleads_association.leadid",
                cmc_conditiontype = new OptionSetValue((int)cmc_domconditiontype.BeginsWith),
                cmc_value = "2018"
            };
            return domDefinitionLogicInstance;
        }
        private cmc_domdefinitionlogic PrepareCmcDomDefinitionLogicInstance2(Guid cmcDomDefinition)
        {
            var domDefinitionLogicInstance = new cmc_domdefinitionlogic()
            {
                Id = Guid.NewGuid(),
                cmc_domdefinitionid = new EntityReference("cmc_domdefinition", cmcDomDefinition),
                cmc_attributeschema = "contact.cmc_contact_successplanassignment.cmc_successplanassignmentid",
                cmc_conditiontype = new OptionSetValue((int)cmc_domconditiontype.BeginsWith),
                cmc_value = "2018"
            };
            return domDefinitionLogicInstance;
        }
        private cmc_domdefinitionlogic PrepareCmcDomDefinitionLogicInstance(Guid cmcDomDefinition)
        {
            var domDefinitionLogicInstance = new cmc_domdefinitionlogic()
            {
                Id = Guid.NewGuid(),
                cmc_domdefinitionid = new EntityReference("cmc_domdefinition", cmcDomDefinition),
                cmc_attributeschema = "contact.mshied_academicperiod_contact_CurrentAcademicPeriod.mshied_academicperiodid",
                cmc_conditiontype = new OptionSetValue((int)cmc_domconditiontype.Equals),
                cmc_value = "2018"
            };
            return domDefinitionLogicInstance;
        }

        private cmc_domdefinitionlogic PrepareCmcDomDefinitionLogic_BeginWith(Guid cmcDomDefinition)
        {
            var domDefinitionLogicInstance = new cmc_domdefinitionlogic()
            {
                Id = Guid.NewGuid(),
                cmc_domdefinitionid = new EntityReference("cmc_domdefinition", cmcDomDefinition),
                cmc_attributeschema = "contact.cmc_dateofbirth",
                cmc_conditiontype = new OptionSetValue((int)cmc_domconditiontype.BeginsWith),
                cmc_value = "a",
                cmc_maximum = "12/31/2017",
                cmc_minimum = "01/01/1999"
            };
            return domDefinitionLogicInstance;
        }

        private cmc_domdefinitionlogic PrepareCmcDomDefinitionLogicInstance_Equal(Guid cmcDomDefinition)
        {
            var domDefinitionLogicInstance = new cmc_domdefinitionlogic()
            {
                Id = Guid.NewGuid(),
                cmc_domdefinitionid = new EntityReference("cmc_domdefinition", cmcDomDefinition),
                cmc_attributeschema = "contact.mshied_academicperiod_contact_CurrentAcademicPeriod.mshied_academicperiodid",
                cmc_conditiontype = new OptionSetValue((int)cmc_domconditiontype.Equals),
                cmc_value = "2018",
                cmc_maximum = "12/31/2017",
                cmc_minimum = "01/01/1999"
            };
            return domDefinitionLogicInstance;
        }
        private cmc_domdefinitionlogic PrepareCmcDomDefinitionLogicInstance_Range(Guid cmcDomDefinition)
        {
            var domDefinitionLogicInstance = new cmc_domdefinitionlogic()
            {
                Id = Guid.NewGuid(),
                cmc_domdefinitionid = new EntityReference("cmc_domdefinition", cmcDomDefinition),
                cmc_attributeschema = "contact.cmc_age",
                cmc_conditiontype = new OptionSetValue((int)cmc_domconditiontype.Range),
                cmc_maximum = "20",
                cmc_minimum = "17"
            };
            return domDefinitionLogicInstance;
        }
        private cmc_domdefinitionlogic PrepareCmcDomDefinitionLogicInstance_RecrdOwnr(Guid cmcDomDefinition)
        {
            var domDefinitionLogicInstance = new cmc_domdefinitionlogic()
            {
                Id = Guid.NewGuid(),
                cmc_domdefinitionid = new EntityReference("cmc_domdefinition", cmcDomDefinition),
                cmc_attributeschema = "contact.cmc_age",
                cmc_conditiontype = new OptionSetValue((int)cmc_domconditiontype.RecordOwner),
                cmc_maximum = "20",
                cmc_minimum = "17"
            };
            return domDefinitionLogicInstance;
        }
        private cmc_domdefinitionlogic PrepareCmcDomDefinitionLogicInstance_Value(Guid cmcDomDefinition)
        {
            var domDefinitionLogicInstance = new cmc_domdefinitionlogic()
            {
                Id = Guid.NewGuid(),
                cmc_domdefinitionid = new EntityReference("cmc_domdefinition", cmcDomDefinition),
                cmc_attributeschema = "contact.createdon",
                cmc_conditiontype = new OptionSetValue((int)cmc_domconditiontype.Equals),
                cmc_value = "1/8/2018",
                cmc_maximum = "12/31/2017",
                cmc_minimum = "01/01/1999"
            };
            return domDefinitionLogicInstance;
        }

        private cmc_domdefinition PrepareCmcDomDefinitionInstance(Guid domMaster)
        {
            var domDefinitionInstance = new cmc_domdefinition()
            {
                Id = Guid.NewGuid(),
                statecode = cmc_domdefinitionState.Active,
                cmc_dommasterid = new EntityReference("cmc_dommaster", domMaster),
                cmc_domdefinitionname = "Test Dom Defintion"
            };
            return domDefinitionInstance;
        }

        private cmc_dommaster PrepareCmcDomMasterInstance(Guid marketingList)
        {
            var domMaster = new cmc_dommaster()
            {
                Id = Guid.NewGuid(),
                cmc_marketinglistid = new EntityReference("List", marketingList),
                cmc_runassignmentforentity = new OptionSetValue((int)cmc_runassignmentforentity.Contact)
            };
            return domMaster;
        }

        private cmc_dommaster PrepareCmcDomMasterInstance1(Guid marketingList)
        {
            var domMaster = new cmc_dommaster()
            {
                Id = Guid.NewGuid(),
                cmc_marketinglistid = new EntityReference("List", marketingList),
                cmc_runassignmentforentity = new OptionSetValue((int)cmc_runassignmentforentity.Account)
            };
            return domMaster;
        }
        private Entity PrepareMarketingListInstance()
        {
            var marketingListInstance = new Entity("List", Guid.NewGuid())
            {
                ["cmc_marketinglisttype"] = cmc_list_cmc_marketinglisttype.StudentGroup,
                ["cmc_expirationdate"] = DateTime.Today.AddDays(-1).Date,
                ["CreatedFromCode"] = list_createdfromcode.Contact,
                ["participationtypemask"] = new OptionSetValue(2),
                ["statecode"] = ListState.Active
            };
            return marketingListInstance;
        }

        private Contact PrepareContactInstance()
        {
            return new Contact()
            {
                Id = Guid.NewGuid(),
                LogicalName = "contact",
                FirstName = "test",
                StateCode = ContactState.Active,
                cmc_domstatus = new OptionSetValue((int)cmc_domstatus.PendingAssignment)
            };

        }
        private cmc_successplanassignment PrepareSuccessPlanAssignment()
        {
            return new cmc_successplanassignment()
            {
                Id = Guid.NewGuid(),
                LogicalName = "cmc_successplanassignment",
                ["cmc_successplanassignmentname"] = "Test name"
               
            };
        }

        private Entity PreparePreImageDomDefinitionEntityCmcValue(Entity domdefinitionlogic, Entity cmcDomDefinition)
        {
            return new Entity("cmc_domdefinitionlogic", domdefinitionlogic.Id)
            {
                ["cmc_domdefinitionid"] = cmcDomDefinition.ToEntityReference(),
                ["cmc_attributeschema"] = domdefinitionlogic.GetAttributeValue<string>("cmc_attributeschema"),
                ["cmc_conditiontype"] = domdefinitionlogic.GetAttributeValue<OptionSetValue>("cmc_conditiontype"),
                ["cmc_value"] = "2018",
                ["statecode"] = domdefinitionlogic.GetAttributeValue<OptionSetValue>("statecode"),
                ["cmc_maximum"] = domdefinitionlogic.GetAttributeValue<string>("cmc_maximum"),
                ["cmc_minimum"] = domdefinitionlogic.GetAttributeValue<string>("cmc_minimum")
            };

        }

        [TestCategory("Plugin"), TestCategory("Negative")]
        [TestMethod]
        public void ValidateDomDefinitionLogicPlugin_CreateMultipleDomDefinitionLogic_Update_ValidateConditionType()
        {
            #region ARRANGE
            var contact = PrepareContactInstance();
            var marketingList = PrepareMarketingListInstance();
            var domMaster = PrepareCmcDomMasterInstance(marketingList.Id);
            var domDefinition = PrepareCmcDomDefinitionInstance(domMaster.Id);
            var domDefinitionLogicRangeNoValue = PrepareCmcDomDefinitionLogic_RangeNoValue(domDefinition.Id);
            var domDefinitionLogicRange = PrepareCmcDomDefinitionLogicInstance_Range(domDefinition.Id);
            var domDefinitionLogicEqualNoValue = PrepareCmcDomDefinitionLogic_EqualNoValue(domDefinition.Id);
            var domDefinitionLogicBegnWthNoValue = PrepareCmcDomDefinitionLogic_BeginWithNoValue(domDefinition.Id);
            var domDefinitionLogicBegnWth = PrepareCmcDomDefinitionLogic_BeginWith(domDefinition.Id);
            var domDefinitionLogicRcrdOwnrNoValue = PrepareCmcDomDefinitionLogic_RecordOwnerNoValue(domDefinition.Id);
            var domDefinitionLogicBegnWthRcdOwner = PrepareCmcDomDefinitionLogicInstance_RecrdOwnr(domDefinition.Id);
            var domDefinitionLogicNoCndtionType = PrepareCmcDomDefinitionLogic_NoConditiontype(domDefinition.Id);

            var attributeTypes = new List<string>()
            {
               "domDefinitionLogicNoCndtionType","domDefinitionLogicRcrdOwnrNoValue_GetEntity", "AllRelatedLookupRcrdOwnr","domDefinitionLogicRcrdOwnrNoValue","BooleanRange","lookupRange","OptionSetRange","domDefinitionLogicRangeNoValue","domDefinitionLogicEqualNoValue","domDefinitionLogicBegnWthNoValue","DateTime","Boolean","OptionSet",
            };


            var xrmFakedContext = new XrmFakedContext { ProxyTypesAssembly = Assembly.Load("Cmc.Engage.Contracts.Tests") };
            xrmFakedContext.AddRelationship("mshied_academicperiod_contact_CurrentAcademicPeriod", new XrmFakedRelationship
            {
                RelationshipType = XrmFakedRelationship.enmFakeRelationshipType.OneToMany,
                Entity1LogicalName = "contact",
                Entity1Attribute = "mshied_currentacademicperiodid",
                Entity2LogicalName = "mshied_academicperiod",
                Entity2Attribute = "mshied_academicperiodid"
            });
            xrmFakedContext.Initialize(new List<Entity>
            {
                contact,
                domMaster,
                marketingList,
                domDefinition,
                domDefinitionLogicBegnWth

            });
            foreach (string i in attributeTypes)
            {
                Entity preImage = null; Mock<IServiceProvider> mockServiceProvider = null;
                if (i == "domDefinitionLogicRangeNoValue")
                {
                    preImage = PreparePreImageDomDefinitionEntityNoMaxiMini(domDefinitionLogicRangeNoValue, domDefinition);
                    mockServiceProvider = InitializeMockService(xrmFakedContext, domDefinitionLogicRangeNoValue, Operation.Update);
                }
                else if (i == "domDefinitionLogicBegnWthNoValue")
                {
                    preImage = PreparePreImageDomDefinitionEntityNoCmcValue(domDefinitionLogicBegnWthNoValue, domDefinition);
                    mockServiceProvider = InitializeMockService(xrmFakedContext, domDefinitionLogicBegnWthNoValue, Operation.Update);
                }
                else if (i == "domDefinitionLogicEqualNoValue")
                {
                    preImage = PreparePreImageDomDefinitionEntityNoCmcValue(domDefinitionLogicEqualNoValue, domDefinition);
                    mockServiceProvider = InitializeMockService(xrmFakedContext, domDefinitionLogicEqualNoValue, Operation.Update);
                }
                else if (i == "domDefinitionLogicRcrdOwnrNoValue")
                {
                    preImage = PreparePreImageDomDefinitionEntityNoCmcValue(domDefinitionLogicRcrdOwnrNoValue, domDefinition);
                    mockServiceProvider = InitializeMockService(xrmFakedContext, domDefinitionLogicRcrdOwnrNoValue, Operation.Update);
                }
                else if (i == "domDefinitionLogicNoCndtionType")
                {
                    preImage = PreparePreImageDomDefinitionEntityNoCmcValue(domDefinitionLogicNoCndtionType, domDefinition);
                    mockServiceProvider = InitializeMockService(xrmFakedContext, domDefinitionLogicNoCndtionType, Operation.Update);
                }
                else if (i == "DateTime" || i == "Boolean" || i == "OptionSet")
                {
                    preImage = PreparePreImageDomDefinitionEntityNoCmcValue(domDefinitionLogicBegnWth, domDefinition);
                    mockServiceProvider = InitializeMockService(xrmFakedContext, domDefinitionLogicBegnWth, Operation.Update);
                }
                else if (i == "OptionSetRange" || i == "lookupRange" || i == "BooleanRange")
                {
                    preImage = PreparePreImageDomDefinitionEntityNoMaxiMini(domDefinitionLogicRange, domDefinition);
                    mockServiceProvider = InitializeMockService(xrmFakedContext, domDefinitionLogicRange, Operation.Update);
                }
                else if (i == "AllRelatedLookupRcrdOwnr" || i == "domDefinitionLogicRcrdOwnrNoValue_GetEntity")
                {
                    preImage = PreparePreImageDomDefinitionEntityNoCmcValue(domDefinitionLogicBegnWthRcdOwner, domDefinition);
                    mockServiceProvider = InitializeMockService(xrmFakedContext, domDefinitionLogicBegnWthRcdOwner, Operation.Update);
                }
                var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
                AddPreEntityImage(mockServiceProvider, "PreImage", preImage);
                #endregion

                #region ACT
                var mockLogger = new Mock<ILogger>();
                // var mockILanguageService = new Mock<ILanguageService>();
                var mockILanguageService = new LanguageService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());

                var entityMetadata = new EntityMetadata() { LogicalName = contact.LogicalName, MetadataId = Guid.NewGuid() };

                if (i == "domDefinitionLogicBegnWthNoValue" || i == "domDefinitionLogicEqualNoValue" || i == "domDefinitionLogicRangeNoValue" || i == "domDefinitionLogicRcrdOwnrNoValue" || i == "domDefinitionLogicNoCndtionType")
                {
                    var attributeMetadata = new AttributeMetadata() { DisplayName = new Label() { UserLocalizedLabel = new LocalizedLabel("frFirstName", 1036) }, LogicalName = "firstname" };
                    entityMetadata.SetAttributeCollection(new List<AttributeMetadata>() { attributeMetadata });
                }
                if (i == "DateTime")
                {
                    var attributeMetadata = new DateTimeAttributeMetadata() { DisplayName = new Label() { UserLocalizedLabel = new LocalizedLabel("frFirstName", 1036) }, LogicalName = "firstname" };
                    entityMetadata.SetAttributeCollection(new List<AttributeMetadata>() { attributeMetadata });
                }
                if (i == "OptionSet" || i == "OptionSetRange")
                {
                    var attributeMetadata = new StateAttributeMetadata() { DisplayName = new Label() { UserLocalizedLabel = new LocalizedLabel("frFirstName", 1036) }, LogicalName = "firstname" };
                    entityMetadata.SetAttributeCollection(new List<AttributeMetadata>() { attributeMetadata });
                }
                if (i == "Boolean" || i == "BooleanRange")
                {
                    var attributeMetadata = new BooleanAttributeMetadata() { DisplayName = new Label() { UserLocalizedLabel = new LocalizedLabel("frFirstName", 1036) }, LogicalName = "firstname" };
                    entityMetadata.SetAttributeCollection(new List<AttributeMetadata>() { attributeMetadata });
                }
                if (i == "lookupRange")
                {
                    var attributeMetadata = new LookupAttributeMetadata() { DisplayName = new Label() { UserLocalizedLabel = new LocalizedLabel("frFirstName", 1036) }, LogicalName = "firstname" };
                    entityMetadata.SetAttributeCollection(new List<AttributeMetadata>() { attributeMetadata });
                }
                if (i == "AllRelatedLookupRcrdOwnr")
                {
                    var attributeMetadata = new LookupAttributeMetadata() { DisplayName = new Label() { UserLocalizedLabel = new LocalizedLabel("frFirstName", 1036) }, LogicalName = contact.LogicalName, Targets = new string[] { contact.LogicalName }, LinkedAttributeId = Guid.NewGuid() };
                    entityMetadata.SetAttributeCollection(new List<LookupAttributeMetadata>() { attributeMetadata });

                    var entityMetadata2 = new EntityMetadata() { LogicalName = contact.LogicalName, MetadataId = Guid.NewGuid() };
                    var ownerAttributeMetadata = new AttributeMetadata();
                    entityMetadata2.SetAttributeCollection(new List<AttributeMetadata>() { ownerAttributeMetadata });
                    var retrieveEntityResponse = new RetrieveEntityResponse()
                    {
                        Results = new ParameterCollection
                        {
                           { "EntityMetadata", entityMetadata2}
                        }
                    };
                    A.CallTo(() => xrmFakedContext.GetFakedOrganizationService().Execute(A<RetrieveEntityRequest>._)).Returns(retrieveEntityResponse);
                }
                if (i == "domDefinitionLogicRcrdOwnrNoValue_GetEntity")
                {
                    var attributeMetadata = new LookupAttributeMetadata() { DisplayName = new Label() { UserLocalizedLabel = new LocalizedLabel("frFirstName", 1036) }, LogicalName = contact.LogicalName, Targets = new string[] { contact.LogicalName }, LinkedAttributeId = Guid.NewGuid() };
                    entityMetadata.SetAttributeCollection(new List<LookupAttributeMetadata>() { attributeMetadata });

                    var entityMetadata2 = new EntityMetadata() { LogicalName = contact.LogicalName, MetadataId = Guid.NewGuid() };
                    var ownerAttributeMetadata = new AttributeMetadata();
                    entityMetadata2.SetAttributeCollection(new List<AttributeMetadata>() { ownerAttributeMetadata });
                    var retrieveEntityResponse = new RetrieveEntityResponse()
                    {
                        Results = new ParameterCollection
                        {
                           { "EntityMetadata", entityMetadata2}
                        }
                    };
                    A.CallTo(() => xrmFakedContext.GetFakedOrganizationService().Execute(A<RetrieveEntityRequest>._)).Returns(retrieveEntityResponse);
                }


                var entityMetadataCollection = new EntityMetadataCollection();
                entityMetadataCollection.Add(entityMetadata);

                var retrieveMetadataChangesResponse = new RetrieveMetadataChangesResponse()
                {
                    Results = new ParameterCollection
                    {
                      {"EntityMetadata", entityMetadataCollection}
                    }
                };
                A.CallTo(() => xrmFakedContext.GetFakedOrganizationService().Execute(A<RetrieveMetadataChangesRequest>._)).Returns(retrieveMetadataChangesResponse);
                var mockDomService = new DomDefinitionLogicService(mockLogger.Object, mockILanguageService,
                   xrmFakedContext.GetFakedOrganizationService());
                try
                {
                    mockDomService.ValidateDomDefinitionLogic(mockExecutionContext.Object);
                }
                catch (Exception) { }

                #endregion

                #region ASSERT
                Assert.IsTrue(true);
                #endregion ASSERT                
            }
        }

        [TestCategory("Plugin"), TestCategory("Negative")]
        [TestMethod]
        public void ValidateDomDefinitionLogicPlugin_CreateMultipleDomDefinitionLogic_Update_ValidateValueExists_ValidateDateTimeField_ValidateDefinitionLogic()
        {
            #region ARRANGE
            var contact = PrepareContactInstance();
            var marketingList = PrepareMarketingListInstance();
            var domMaster = PrepareCmcDomMasterInstance(marketingList.Id);
            var domDefinition = PrepareCmcDomDefinitionInstance(domMaster.Id);
            var domDefinitionLogicNoConditiontype = PrepareCmcDomDefinitionLogic_NoConditiontype(domDefinition.Id);
            var domDefinitionLogic = PrepareCmcDomDefinitionLogicInstance(domDefinition.Id);
            var domDefinitionLogicValueDate = PrepareCmcDomDefinitionLogicInstance_WrngDateValue(domDefinition.Id);
            var domDefinitionLogicValueDateDfltGuid = PrepareCmcDomDefinitionLogicInstance_DefltGuidValue(domDefinition.Id);

            Dictionary<string, cmc_domdefinitionlogic> cmc_Domdefinitionlogics = new Dictionary<string, cmc_domdefinitionlogic>()
            {
              {"NoValidDomDefinationService",domDefinitionLogicValueDate},{"domDefinitionLogicValueDatedfrnt",domDefinitionLogicValueDate},{"domDefinitionLogicValueDate",domDefinitionLogicValueDate},{ "domDefinitionLogicValueDateDfltGuid",domDefinitionLogicValueDateDfltGuid}, {"domDefinitionLogicBooleanTrue",domDefinitionLogic},{"domDefinitionLogicBooleanFalse",domDefinitionLogic},{"domDefinitionLogicNoConditiontype",domDefinitionLogicNoConditiontype}
            };

            var xrmFakedContext = new XrmFakedContext { ProxyTypesAssembly = Assembly.Load("Cmc.Engage.Contracts.Tests") };

            xrmFakedContext.AddRelationship("mshied_academicperiod_contact_CurrentAcademicPeriod", new XrmFakedRelationship
            {
                RelationshipType = XrmFakedRelationship.enmFakeRelationshipType.OneToMany,
                Entity1LogicalName = "contact",
                Entity1Attribute = "mshied_currentacademicperiodid",
                Entity2LogicalName = "mshied_academicperiod",
                Entity2Attribute = "mshied_academicperiodid"
            });
            xrmFakedContext.Initialize(new List<Entity>
            {
                contact,
                domMaster,
                marketingList,
                domDefinition,
                domDefinitionLogic,
                domDefinitionLogicNoConditiontype,
                domDefinitionLogicValueDate

            });
            foreach (var item in cmc_Domdefinitionlogics)
            {
                Entity preImage = null;
                if (item.Key == "domDefinitionLogicValueDatedfrnt")
                    preImage = PreparePreImageDomDefinitionEntityDifferentAttributeSchemaName(item.Value, domDefinition);
                
                else
                    preImage = PreparePreImageDomDefinitionEntityNoCmcValue(item.Value, domDefinition);

                Mock<IServiceProvider> mockServiceProvider = null;
                if (item.Key == "NoValidDomDefinationService")               
                    mockServiceProvider = InitializeMockService(xrmFakedContext, domDefinition, Operation.Update);               
                else
                    mockServiceProvider = InitializeMockService(xrmFakedContext, item.Value, Operation.Update);
                var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
                AddPreEntityImage(mockServiceProvider, "PreImage", preImage);
                #endregion

                #region ACT

                var entityMetadata = new EntityMetadata() { LogicalName = contact.LogicalName, MetadataId = Guid.NewGuid() };
                if (item.Key == "domDefinitionLogicNoConditiontype")
                {
                    var attributeMetadata = new AttributeMetadata() { DisplayName = new Label() { UserLocalizedLabel = new LocalizedLabel("frFirstName", 1036) }, LogicalName = "firstname" };
                    entityMetadata.SetAttributeCollection(new List<AttributeMetadata>() { attributeMetadata });
                }

                if (item.Key == "domDefinitionLogicBooleanFalse")
                {
                    var attributeMetadata = new BooleanAttributeMetadata() { LogicalName = "firstname", OptionSet = new BooleanOptionSetMetadata(new OptionMetadata(new Label("20181", 0), 0), new OptionMetadata(new Label("20181", 1), 1)) };
                    entityMetadata.SetAttributeCollection(new List<AttributeMetadata>() { attributeMetadata });
                }
                if (item.Key == "domDefinitionLogicBooleanTrue")
                {
                    var attributeMetadata = new BooleanAttributeMetadata() { LogicalName = "firstname", OptionSet = new BooleanOptionSetMetadata(new OptionMetadata(new Label("2018", 0), 0), new OptionMetadata(new Label("2018", 1), 1)) };
                    entityMetadata.SetAttributeCollection(new List<AttributeMetadata>() { attributeMetadata });
                }
                if (item.Key == "domDefinitionLogicValueDate" || item.Key == "domDefinitionLogicValueDateDfltGuid" || item.Key == "domDefinitionLogicValueDatedfrnt")
                {
                    var attributeMetadata = new DateTimeAttributeMetadata() { DisplayName = new Label() { UserLocalizedLabel = new LocalizedLabel("frFirstName", 1036) }, LogicalName = "firstname" };
                    entityMetadata.SetAttributeCollection(new List<DateTimeAttributeMetadata>() { attributeMetadata });
                }


                var entityMetadataCollection = new EntityMetadataCollection();
                entityMetadataCollection.Add(entityMetadata);
                RetrieveMetadataChangesResponse retrieveMetadataChangesResponse = new RetrieveMetadataChangesResponse()
                {
                    Results = new ParameterCollection
                        {
                           { "EntityMetadata", entityMetadataCollection}

                        }
                };

                var mockLogger = new Mock<ILogger>();
                var mockLanguageService = new LanguageService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
                A.CallTo(() => xrmFakedContext.GetFakedOrganizationService().Execute(A<RetrieveMetadataChangesRequest>._)).Returns(retrieveMetadataChangesResponse);
                var mockDomService = new DomDefinitionLogicService(mockLogger.Object, mockLanguageService,
                xrmFakedContext.GetFakedOrganizationService());
                try
                {
                    mockDomService.ValidateDomDefinitionLogic(mockExecutionContext.Object);
                }
                catch { }
                #endregion

                #region ASSERT
                Assert.IsTrue(true);
                #endregion ASSERT  
            }
        }
        private cmc_domdefinitionlogic PrepareCmcDomDefinitionLogic_BeginWithNoValue(Guid cmcDomDefinition)
        {
            var domDefinitionLogicInstance = new cmc_domdefinitionlogic()
            {
                Id = Guid.NewGuid(),
                cmc_domdefinitionid = new EntityReference("cmc_domdefinition", cmcDomDefinition),
                cmc_attributeschema = "contact.cmc_dateofbirth",
                cmc_conditiontype = new OptionSetValue((int)cmc_domconditiontype.BeginsWith)
            };
            return domDefinitionLogicInstance;
        }
        private cmc_domdefinitionlogic PrepareCmcDomDefinitionLogic_EqualNoValue(Guid cmcDomDefinition)
        {
            var domDefinitionLogicInstance = new cmc_domdefinitionlogic()
            {
                Id = Guid.NewGuid(),
                cmc_domdefinitionid = new EntityReference("cmc_domdefinition", cmcDomDefinition),
                cmc_attributeschema = "contact.cmc_dateofbirth",
                cmc_conditiontype = new OptionSetValue((int)cmc_domconditiontype.Equals)
            };
            return domDefinitionLogicInstance;
        }
        private cmc_domdefinitionlogic PrepareCmcDomDefinitionLogic_RangeNoValue(Guid cmcDomDefinition)
        {
            var domDefinitionLogicInstance = new cmc_domdefinitionlogic()
            {
                Id = Guid.NewGuid(),
                cmc_domdefinitionid = new EntityReference("cmc_domdefinition", cmcDomDefinition),
                cmc_attributeschema = "contact.cmc_dateofbirth",
                cmc_conditiontype = new OptionSetValue((int)cmc_domconditiontype.Range)
            };
            return domDefinitionLogicInstance;
        }
        private cmc_domdefinitionlogic PrepareCmcDomDefinitionLogic_RecordOwnerNoValue(Guid cmcDomDefinition)
        {
            var domDefinitionLogicInstance = new cmc_domdefinitionlogic()
            {
                Id = Guid.NewGuid(),
                cmc_domdefinitionid = new EntityReference("cmc_domdefinition", cmcDomDefinition),
                cmc_attributeschema = "contact.cmc_dateofbirth",
                cmc_conditiontype = new OptionSetValue((int)cmc_domconditiontype.RecordOwner)
            };
            return domDefinitionLogicInstance;
        }
        private cmc_domdefinitionlogic PrepareCmcDomDefinitionLogic_NoConditiontype(Guid cmcDomDefinition)
        {
            var domDefinitionLogicInstance = new cmc_domdefinitionlogic()
            {
                Id = Guid.NewGuid(),
                cmc_domdefinitionid = new EntityReference("cmc_domdefinition", cmcDomDefinition),
                cmc_attributeschema = "contact.cmc_dateofbirth",
                cmc_conditiontype = new OptionSetValue((int)cmc_domconditiontype.IsNull)
            };
            return domDefinitionLogicInstance;
        }
        private cmc_domdefinitionlogic PrepareCmcDomDefinitionLogicInstance_WrngDateValue(Guid cmcDomDefinition)
        {
            var domDefinitionLogicInstance = new cmc_domdefinitionlogic()
            {
                Id = Guid.NewGuid(),
                cmc_domdefinitionid = new EntityReference("cmc_domdefinition", cmcDomDefinition),
                cmc_attributeschema = "contact.createdon",
                cmc_conditiontype = new OptionSetValue((int)cmc_domconditiontype.Equals),
                cmc_value = "18/18/2018",
            };
            return domDefinitionLogicInstance;
        }
        private cmc_domdefinitionlogic PrepareCmcDomDefinitionLogicInstance_DefltGuidValue(Guid cmcDomDefinition)
        {
            var domDefinitionLogicInstance = new cmc_domdefinitionlogic()
            {
                Id = Guid.NewGuid(),
                cmc_domdefinitionid = new EntityReference("cmc_domdefinition", default(Guid)),
                cmc_attributeschema = "contact.createdon",
                cmc_conditiontype = new OptionSetValue((int)cmc_domconditiontype.Equals),
                cmc_value = "1/8/2018",
            };
            return domDefinitionLogicInstance;
        }
        private Entity PreparePreImageDomDefinitionEntityNoCmcValue(Entity domdefinitionlogic, Entity cmcDomDefinition)
        {
            return new Entity("cmc_domdefinitionlogic", domdefinitionlogic.Id)
            {
                ["cmc_domdefinitionid"] = cmcDomDefinition.ToEntityReference(),
                ["cmc_attributeschema"] = domdefinitionlogic.GetAttributeValue<string>("cmc_attributeschema"),
                ["cmc_conditiontype"] = domdefinitionlogic.GetAttributeValue<OptionSetValue>("cmc_conditiontype"),
                ["statecode"] = domdefinitionlogic.GetAttributeValue<OptionSetValue>("statecode"),
                ["cmc_maximum"] = domdefinitionlogic.GetAttributeValue<string>("cmc_maximum"),
                ["cmc_minimum"] = domdefinitionlogic.GetAttributeValue<string>("cmc_minimum")
            };
        }
        private Entity PreparePreImageDomDefinitionEntityDifferentAttributeSchemaName(Entity domdefinitionlogic, Entity cmcDomDefinition)
        {
            return new Entity("cmc_domdefinitionlogic", domdefinitionlogic.Id)
            {
                ["cmc_domdefinitionid"] = cmcDomDefinition.ToEntityReference(),
                ["cmc_attributeschema"] = "contact.firstname",
                ["cmc_conditiontype"] = domdefinitionlogic.GetAttributeValue<OptionSetValue>("cmc_conditiontype"),
                ["statecode"] = domdefinitionlogic.GetAttributeValue<OptionSetValue>("statecode"),
                ["cmc_maximum"] = domdefinitionlogic.GetAttributeValue<string>("cmc_maximum"),
                ["cmc_minimum"] = domdefinitionlogic.GetAttributeValue<string>("cmc_minimum")
            };
        }
        private Entity PreparePreImageDomDefinitionEntityNoMaxiMini(Entity domdefinitionlogic, Entity cmcDomDefinition)
        {
            return new Entity("cmc_domdefinitionlogic", domdefinitionlogic.Id)
            {
                ["cmc_domdefinitionid"] = cmcDomDefinition.ToEntityReference(),
                ["cmc_attributeschema"] = domdefinitionlogic.GetAttributeValue<string>("cmc_attributeschema"),
                ["cmc_conditiontype"] = domdefinitionlogic.GetAttributeValue<OptionSetValue>("cmc_conditiontype"),
                ["statecode"] = domdefinitionlogic.GetAttributeValue<OptionSetValue>("statecode"),
            };
        }
    }

}
