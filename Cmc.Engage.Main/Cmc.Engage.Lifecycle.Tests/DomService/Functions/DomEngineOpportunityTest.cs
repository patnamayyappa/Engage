using System;
using System.Collections.Generic;
using System.Reflection;
using Cmc.Core.Xrm.ServerExtension.Logging;
using Cmc.Engage.Common;
using Cmc.Engage.Common.Plugins.Tests.Utilities;
using Cmc.Engage.Common.Utilities.Constants;
using Cmc.Engage.Models;
using FakeItEasy;
using FakeXrmEasy;
using FakeXrmEasy.Extensions;
using Microsoft.Crm.Sdk.Messages;
//using Microsoft.IdentityModel.Protocols.WSFederation.Metadata;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using Moq;


namespace Cmc.Engage.Lifecycle.Plugins.Test.DomService.Functions
{
    [TestClass]
    public class DomEngineOpportunityTest : XrmUnitTestBase
    {
        [TestCategory("Function"), TestCategory("Positive")]
        [TestMethod]
        public void DomEngineOpportunity()
        {
            #region ARRANGE
            var bingMapKeyConfigInstance = GetConfiguration();

            var xrmFakedContext = new XrmFakedContext { ProxyTypesAssembly = Assembly.Load("Cmc.Engage.Contracts.Tests") };
            var calledId = Guid.NewGuid();
            xrmFakedContext.CallerId = new EntityReference("SystemUser", calledId);
            var timezonedefinition = new Entity("timezonedefinition", Guid.NewGuid())
            {
                ["timezonecode"] = 190,
                ["standardname"] = "India Standard Time"
            };
            var systemUser = new Entity("SystemUser", xrmFakedContext.CallerId.Id)
            {
                ["systemuserid"] = xrmFakedContext.CallerId.Id,
            };


            var list = GetList(systemUser);
            var domMaster = Getcmc_dommaster(systemUser, list.ToEntityReference());
            var domdefinitionexecutionorder = Getdomdefinitionexecutionorder(systemUser, domMaster.Id);
            var opportunity = GetOpportunityEntity(systemUser);

            var domdefinition = Getdomdefinition(domMaster.ToEntityReference(), systemUser);
            var domdefinitionLogic = GetdomdefinitionLogic(domdefinition.ToEntityReference(), systemUser);

            var contact = new Entity("contact", Guid.NewGuid())
            {
                ["firstname"] = "Cameron2",
                ["contactid"] = systemUser.ToEntityReference()
            };
            var mockUserSettings = new Entity("usersettings", Guid.NewGuid())
            {
                ["localeid"] = 1033,
                ["systemuserid"] = systemUser.ToEntityReference(),
                ["timezonecode"] = timezonedefinition.GetAttributeValue<int>("timezonecode")
            };
            xrmFakedContext.AddRelationship("user_settings", new XrmFakedRelationship
            {
                RelationshipType = XrmFakedRelationship.enmFakeRelationshipType.OneToMany,
                Entity1LogicalName = "systemuser",
                Entity1Attribute = "systemuserid",
                Entity2LogicalName = "usersettings",
                Entity2Attribute = "systemuserid"
            });
            var assocationListMember = new Entity("listmember", Guid.NewGuid())
            {
                ["entityid"] = contact.Id,
                ["listid"] = list.Id
            };
            xrmFakedContext.AddRelationship("listcontact_association", new XrmFakedRelationship
            {
                IntersectEntity = "listmember",
                RelationshipType = XrmFakedRelationship.enmFakeRelationshipType.ManyToMany,
                Entity1LogicalName = List.EntityLogicalName,
                Entity1Attribute = "listid",
                Entity2LogicalName = Contact.EntityLogicalName,
                Entity2Attribute = "entityid"
            });
            xrmFakedContext.AddRelationship("opportunitycompetitors_association", new XrmFakedRelationship
            {
                IntersectEntity = "opportunitycompetitors",
                RelationshipType = XrmFakedRelationship.enmFakeRelationshipType.ManyToMany,
                Entity1LogicalName = Opportunity.EntityLogicalName,
                Entity1Attribute = "opportunityid",
                Entity2LogicalName = "competitor",
                Entity2Attribute = "competitorid"
            });

            xrmFakedContext.AddRelationship("timezonedefinition", new XrmFakedRelationship
            {
                RelationshipType = XrmFakedRelationship.enmFakeRelationshipType.OneToMany,
                Entity1LogicalName = SystemUser.EntityLogicalName,
                Entity1Attribute = "timezonecode",
                Entity2LogicalName = "timezonedefinition",
                Entity2Attribute = "timezonecode"
            });

            xrmFakedContext.AddRelationship("cmc_contact_opportunity_contactid", new XrmFakedRelationship
            {
                RelationshipType = XrmFakedRelationship.enmFakeRelationshipType.OneToMany,
                Entity1LogicalName = "opportunity",
                Entity1Attribute = "cmc_contactid",
                Entity2LogicalName = "contact",
                Entity2Attribute = "contactid"
            });

            xrmFakedContext.Initialize(new List<Entity>
            {
                contact,
                assocationListMember,
                timezonedefinition,
                mockUserSettings,
                systemUser,
                bingMapKeyConfigInstance,

                domMaster,
                domdefinitionexecutionorder,
                list,
                opportunity,
                domdefinition,
                domdefinitionLogic

            });
            #endregion

            #region ACT
            var mockLogger = new Mock<ILogger>();
            var mockILanguageService = new Mock<ILanguageService>();
            var mockConfigurationService = new ConfigurationService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
            var localTimeFromUtcTimeResponse = new LocalTimeFromUtcTimeResponse
            {
                Results = new ParameterCollection
                {
                    { "LocalTime",DateTime.Now}
                }
            };
            var responses = new ExecuteMultipleResponseItemCollection();
            responses.Add(new ExecuteMultipleResponseItem()
            {
                Fault = null
            });
            var executeMultipleResponse = new ExecuteMultipleResponse
            {
                Results = new ParameterCollection
                {
                    { "Responses",responses}
                }
            };
            var entityMetadata = new EntityMetadata() { LogicalName = "contact" };
            entityMetadata.SetAttributeCollection(new List<AttributeMetadata>()
            {
                new UniqueIdentifierAttributeMetadata("contactid") {LogicalName =   "contactid",},
                new LookupAttributeMetadata(){
                    LogicalName =   "firstname",
                    SchemaName = "firstname",
                    DisplayName = new Label("systemuser Lookup",1033),
                    Targets = new []{ "firstname" }
                },
            });

            QueryExpression contactquery = new QueryExpression()
            {
                EntityName = Contact.EntityLogicalName
            };
            var fetchXmlToQueryExpressionResponse = new FetchXmlToQueryExpressionResponse()
            {
                Results = new ParameterCollection
                        {
                            { "Query", contactquery}
                        }
            };

            //Excute FetchXmlToQueryExpressionRequest if the name contain contact
            A.CallTo(() => xrmFakedContext.GetFakedOrganizationService().Execute(A<FetchXmlToQueryExpressionRequest>.That.Matches(r => r.FetchXml.Contains("name='contact'"))))
                .ReturnsLazily((req) => { return fetchXmlToQueryExpressionResponse; });

            xrmFakedContext.InitializeMetadata(entityMetadata);
            A.CallTo(() => xrmFakedContext.GetFakedOrganizationService().Execute(A<LocalTimeFromUtcTimeRequest>._)).Returns(localTimeFromUtcTimeResponse);
            var mockDomService = new DOMService(mockLogger.Object, mockILanguageService.Object, xrmFakedContext.GetFakedOrganizationService(), mockConfigurationService);           
            mockDomService.ProcessDomAssignment("opportunity", xrmFakedContext.GetFakedOrganizationService());
            #endregion

            #region ASSERT
            Assert.IsTrue(xrmFakedContext.Data["post"] != null);
            #endregion ASSERT  
        }

        [TestCategory("Function"), TestCategory("Positive")]
        [TestMethod]
        public void DomEngineOpportunity_ConditionType_IsBeginWith()
        {
            #region ARRANGE
            var bingMapKeyConfigInstance = GetConfiguration();

            var xrmFakedContext = new XrmFakedContext { ProxyTypesAssembly = Assembly.Load("Cmc.Engage.Contracts.Tests") };
            var calledId = Guid.NewGuid();
            xrmFakedContext.CallerId = new EntityReference("SystemUser", calledId);
            var timezonedefinition = new Entity("timezonedefinition", Guid.NewGuid())
            {
                ["timezonecode"] = 190,
                ["standardname"] = "India Standard Time"
            };
            var systemUser = new Entity("SystemUser", xrmFakedContext.CallerId.Id)
            {
                ["systemuserid"] = xrmFakedContext.CallerId.Id,
            };

            var systemUser2 = new Entity("SystemUser", Guid.NewGuid());

            var list = GetList(systemUser);
            var domMaster = Getcmc_dommaster2(systemUser, list.ToEntityReference(), systemUser2);
            var domdefinitionexecutionorder = Getdomdefinitionexecutionorder(systemUser, domMaster.Id);
            var opportunity = GetOpportunityEntity(systemUser);

            var domdefinition = Getdomdefinition(domMaster.ToEntityReference(), systemUser);
            var domdefinitionLogic = GetdomdefinitionLogic1(domdefinition.ToEntityReference(), systemUser);

            var contact = new Entity("contact", systemUser.Id)
            {
                ["firstname"] = "Cameron2",
                //["contactid"] = systemUser.ToEntityReference()
            };
            var mockUserSettings = new Entity("usersettings", Guid.NewGuid())
            {
                ["localeid"] = 1033,
                ["systemuserid"] = systemUser.ToEntityReference(),
                ["timezonecode"] = timezonedefinition.GetAttributeValue<int>("timezonecode")
            };
            xrmFakedContext.AddRelationship("user_settings", new XrmFakedRelationship
            {
                RelationshipType = XrmFakedRelationship.enmFakeRelationshipType.OneToMany,
                Entity1LogicalName = "systemuser",
                Entity1Attribute = "systemuserid",
                Entity2LogicalName = "usersettings",
                Entity2Attribute = "systemuserid"
            });
            var assocationListMember = new Entity("listmember", Guid.NewGuid())
            {
                ["entityid"] = contact.Id,
                ["listid"] = list.Id
            };
            xrmFakedContext.AddRelationship("listcontact_association", new XrmFakedRelationship
            {
                IntersectEntity = "listmember",
                RelationshipType = XrmFakedRelationship.enmFakeRelationshipType.ManyToMany,
                Entity1LogicalName = List.EntityLogicalName,
                Entity1Attribute = "listid",
                Entity2LogicalName = Contact.EntityLogicalName,
                Entity2Attribute = "entityid"
            });
            xrmFakedContext.AddRelationship("opportunitycompetitors_association", new XrmFakedRelationship
            {
                IntersectEntity = "opportunitycompetitors",
                RelationshipType = XrmFakedRelationship.enmFakeRelationshipType.ManyToMany,
                Entity1LogicalName = Opportunity.EntityLogicalName,
                Entity1Attribute = "opportunityid",
                Entity2LogicalName = "competitor",
                Entity2Attribute = "competitorid"
            });

            xrmFakedContext.AddRelationship("timezonedefinition", new XrmFakedRelationship
            {
                RelationshipType = XrmFakedRelationship.enmFakeRelationshipType.OneToMany,
                Entity1LogicalName = SystemUser.EntityLogicalName,
                Entity1Attribute = "timezonecode",
                Entity2LogicalName = "timezonedefinition",
                Entity2Attribute = "timezonecode"
            });

            xrmFakedContext.AddRelationship("cmc_contact_opportunity_contactid", new XrmFakedRelationship
            {
                RelationshipType = XrmFakedRelationship.enmFakeRelationshipType.OneToMany,
                Entity1LogicalName = "opportunity",
                Entity1Attribute = "cmc_contactid",
                Entity2LogicalName = "contact",
                Entity2Attribute = "contactid"
            });

            xrmFakedContext.Initialize(new List<Entity>
            {
                contact,
                assocationListMember,
                timezonedefinition,
                mockUserSettings,
                systemUser,
                bingMapKeyConfigInstance,

                domMaster,
                domdefinitionexecutionorder,
                list,
                opportunity,
                domdefinition,
                domdefinitionLogic

            });
            #endregion

            #region ACT
            var mockLogger = new Mock<ILogger>();
            var mockILanguageService = new Mock<ILanguageService>();
            var mockConfigurationService = new ConfigurationService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
            var localTimeFromUtcTimeResponse = new LocalTimeFromUtcTimeResponse
            {
                Results = new ParameterCollection
                {
                    { "LocalTime",DateTime.Now}
                }
            };
            var responses = new ExecuteMultipleResponseItemCollection();
            responses.Add(new ExecuteMultipleResponseItem()
            {
                Fault = null
            });
            var executeMultipleResponse = new ExecuteMultipleResponse
            {
                Results = new ParameterCollection
                {
                    { "Responses",responses}
                }
            };
            var entityMetadata = new EntityMetadata() { LogicalName = "contact", MetadataId = Guid.NewGuid()};
            var attributeMetadata = new LookupAttributeMetadata()
            {
                LogicalName = "contact",
                SchemaName = "contact",
                DisplayName = new Label("owner", 1033),
                Targets = new[] { "systemuser" },
            };
            var userLocalizedLabel = new LocalizedLabel("systemuser", 1033);
            LocalizedLabel[] labels = {userLocalizedLabel};
            entityMetadata.SetAttributeCollection(new List<AttributeMetadata>()
            {
                new UniqueIdentifierAttributeMetadata("contactid") {LogicalName =   "contactid"},
                new LookupAttributeMetadata(){

                    LogicalName =   "ownerid",
                    SchemaName = "owner",
                    DisplayName = new Label(userLocalizedLabel,labels),
                    Targets = new []{ "systemuser" },
                },
            });

            var primaryNameAttribute = entityMetadata.GetType().GetProperty("PrimaryNameAttribute");
            if (primaryNameAttribute != null) primaryNameAttribute.SetValue(entityMetadata, "contact");

            QueryExpression contactquery = new QueryExpression()
            {
                EntityName = Contact.EntityLogicalName
            };
            var fetchXmlToQueryExpressionResponse = new FetchXmlToQueryExpressionResponse()
            {
                Results = new ParameterCollection
                        {
                            { "Query", contactquery}
                        }
            };
            var retrieveEntityResponse = new RetrieveEntityResponse()
            {
                Results = new ParameterCollection
                {
                    { "EntityMetadata", entityMetadata }
                }
            };
            var retrieveAttributedata=new RetrieveAttributeResponse()
            {
                Results = new ParameterCollection()
                {
                    { "AttributeMetadata", attributeMetadata }
                }
            };

            //A.CallTo(() => entityMetadata.PrimaryNameAttribute).Returns("firstname");

            A.CallTo(() => xrmFakedContext.GetFakedOrganizationService().Execute(A<RetrieveEntityRequest>.Ignored)).Returns(retrieveEntityResponse);
            A.CallTo(() => xrmFakedContext.GetFakedOrganizationService().Execute(A<RetrieveAttributeRequest>.Ignored)).Returns(retrieveAttributedata);

            //Excute FetchXmlToQueryExpressionRequest if the name contain contact
            A.CallTo(() => xrmFakedContext.GetFakedOrganizationService().Execute(A<FetchXmlToQueryExpressionRequest>.That.Matches(r => r.FetchXml.Contains("name='contact'"))))
                .ReturnsLazily((req) => { return fetchXmlToQueryExpressionResponse; });

            xrmFakedContext.InitializeMetadata(entityMetadata);

            A.CallTo(() => xrmFakedContext.GetFakedOrganizationService().Execute(A<LocalTimeFromUtcTimeRequest>._)).Returns(localTimeFromUtcTimeResponse);
            var mockDomService = new DOMService(mockLogger.Object, mockILanguageService.Object, xrmFakedContext.GetFakedOrganizationService(), mockConfigurationService);
          
           
            mockDomService.ProcessDomAssignment("opportunity", xrmFakedContext.GetFakedOrganizationService());
            #endregion

            #region ASSERT
            Assert.IsTrue(xrmFakedContext.Data["post"] != null);
            #endregion ASSERT  
        }

        [TestCategory("Function"), TestCategory("Positive")]
        [TestMethod]
        public void DomEngineOpportunity_ConditionType_IsEquals()
        {
            #region ARRANGE
            var bingMapKeyConfigInstance = GetConfiguration();

            var xrmFakedContext = new XrmFakedContext { ProxyTypesAssembly = Assembly.Load("Cmc.Engage.Contracts.Tests") };
            var calledId = Guid.NewGuid();
            xrmFakedContext.CallerId = new EntityReference("SystemUser", calledId);
            var timezonedefinition = new Entity("timezonedefinition", Guid.NewGuid())
            {
                ["timezonecode"] = 190,
                ["standardname"] = "India Standard Time"
            };
            var systemUser = new Entity("SystemUser", xrmFakedContext.CallerId.Id)
            {
                ["systemuserid"] = xrmFakedContext.CallerId.Id,
            };

            var systemUser2 = new Entity("SystemUser", Guid.NewGuid());

            var list = GetList(systemUser);
            var domMaster = Getcmc_dommaster2(systemUser, list.ToEntityReference(), systemUser2);
            var domdefinitionexecutionorder = Getdomdefinitionexecutionorder(systemUser, domMaster.Id);
            var opportunity = GetOpportunityEntity(systemUser);

            var domdefinition = Getdomdefinition(domMaster.ToEntityReference(), systemUser);
            var domdefinitionLogic = GetdomdefinitionLogic2(domdefinition.ToEntityReference(), systemUser);

            var contact = new Entity("contact", systemUser.Id)
            {
                ["firstname"] = "Cameron2",
                //["contactid"] = systemUser.ToEntityReference()
            };
            var mockUserSettings = new Entity("usersettings", Guid.NewGuid())
            {
                ["localeid"] = 1033,
                ["systemuserid"] = systemUser.ToEntityReference(),
                ["timezonecode"] = timezonedefinition.GetAttributeValue<int>("timezonecode")
            };
            xrmFakedContext.AddRelationship("user_settings", new XrmFakedRelationship
            {
                RelationshipType = XrmFakedRelationship.enmFakeRelationshipType.OneToMany,
                Entity1LogicalName = "systemuser",
                Entity1Attribute = "systemuserid",
                Entity2LogicalName = "usersettings",
                Entity2Attribute = "systemuserid"
            });
            var assocationListMember = new Entity("listmember", Guid.NewGuid())
            {
                ["entityid"] = contact.Id,
                ["listid"] = list.Id
            };
            xrmFakedContext.AddRelationship("listcontact_association", new XrmFakedRelationship
            {
                IntersectEntity = "listmember",
                RelationshipType = XrmFakedRelationship.enmFakeRelationshipType.ManyToMany,
                Entity1LogicalName = List.EntityLogicalName,
                Entity1Attribute = "listid",
                Entity2LogicalName = Contact.EntityLogicalName,
                Entity2Attribute = "entityid"
            });
            xrmFakedContext.AddRelationship("opportunitycompetitors_association", new XrmFakedRelationship
            {
                IntersectEntity = "opportunitycompetitors",
                RelationshipType = XrmFakedRelationship.enmFakeRelationshipType.ManyToMany,
                Entity1LogicalName = Opportunity.EntityLogicalName,
                Entity1Attribute = "opportunityid",
                Entity2LogicalName = "competitor",
                Entity2Attribute = "competitorid"
            });

            xrmFakedContext.AddRelationship("timezonedefinition", new XrmFakedRelationship
            {
                RelationshipType = XrmFakedRelationship.enmFakeRelationshipType.OneToMany,
                Entity1LogicalName = SystemUser.EntityLogicalName,
                Entity1Attribute = "timezonecode",
                Entity2LogicalName = "timezonedefinition",
                Entity2Attribute = "timezonecode"
            });

            xrmFakedContext.AddRelationship("cmc_contact_opportunity_contactid", new XrmFakedRelationship
            {
                RelationshipType = XrmFakedRelationship.enmFakeRelationshipType.OneToMany,
                Entity1LogicalName = "opportunity",
                Entity1Attribute = "cmc_contactid",
                Entity2LogicalName = "contact",
                Entity2Attribute = "contactid"
            });

            xrmFakedContext.Initialize(new List<Entity>
            {
                contact,
                assocationListMember,
                timezonedefinition,
                mockUserSettings,
                systemUser,
                bingMapKeyConfigInstance,

                domMaster,
                domdefinitionexecutionorder,
                list,
                opportunity,
                domdefinition,
                domdefinitionLogic

            });
            #endregion

            #region ACT
            var mockLogger = new Mock<ILogger>();
            var mockILanguageService = new Mock<ILanguageService>();
            var mockConfigurationService = new ConfigurationService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
            var localTimeFromUtcTimeResponse = new LocalTimeFromUtcTimeResponse
            {
                Results = new ParameterCollection
                {
                    { "LocalTime",DateTime.Now}
                }
            };
            var responses = new ExecuteMultipleResponseItemCollection();
            responses.Add(new ExecuteMultipleResponseItem()
            {
                Fault = null
            });
            var executeMultipleResponse = new ExecuteMultipleResponse
            {
                Results = new ParameterCollection
                {
                    { "Responses",responses}
                }
            };
            var entityMetadata = new EntityMetadata() { LogicalName = "contact", MetadataId = Guid.NewGuid() };
            var attributeMetadata = new LookupAttributeMetadata()
            {
                LogicalName = "contact",
                SchemaName = "contact",
                DisplayName = new Label("owner", 1033),
                Targets = new[] { "systemuser" },
            };
            var userLocalizedLabel = new LocalizedLabel("systemuser", 1033);
            LocalizedLabel[] labels = { userLocalizedLabel };
            entityMetadata.SetAttributeCollection(new List<AttributeMetadata>()
            {
                new UniqueIdentifierAttributeMetadata("contactid") {LogicalName =   "contactid"},
                new LookupAttributeMetadata(){

                    LogicalName =   "ownerid",
                    SchemaName = "owner",
                    DisplayName = new Label(userLocalizedLabel,labels),
                    Targets = new []{ "systemuser" },
                },
            });

            var primaryNameAttribute = entityMetadata.GetType().GetProperty("PrimaryNameAttribute");
            if (primaryNameAttribute != null) primaryNameAttribute.SetValue(entityMetadata, "contact");

            QueryExpression contactquery = new QueryExpression()
            {
                EntityName = Contact.EntityLogicalName
            };
            var fetchXmlToQueryExpressionResponse = new FetchXmlToQueryExpressionResponse()
            {
                Results = new ParameterCollection
                        {
                            { "Query", contactquery}
                        }
            };
            var retrieveEntityResponse = new RetrieveEntityResponse()
            {
                Results = new ParameterCollection
                {
                    { "EntityMetadata", entityMetadata }
                }
            };
            var retrieveAttributedata = new RetrieveAttributeResponse()
            {
                Results = new ParameterCollection()
                {
                    { "AttributeMetadata", attributeMetadata }
                }
            };

            //A.CallTo(() => entityMetadata.PrimaryNameAttribute).Returns("firstname");

            A.CallTo(() => xrmFakedContext.GetFakedOrganizationService().Execute(A<RetrieveEntityRequest>.Ignored)).Returns(retrieveEntityResponse);
            A.CallTo(() => xrmFakedContext.GetFakedOrganizationService().Execute(A<RetrieveAttributeRequest>.Ignored)).Returns(retrieveAttributedata);

            //Excute FetchXmlToQueryExpressionRequest if the name contain contact
            A.CallTo(() => xrmFakedContext.GetFakedOrganizationService().Execute(A<FetchXmlToQueryExpressionRequest>.That.Matches(r => r.FetchXml.Contains("name='contact'"))))
                .ReturnsLazily((req) => { return fetchXmlToQueryExpressionResponse; });

            xrmFakedContext.InitializeMetadata(entityMetadata);

            A.CallTo(() => xrmFakedContext.GetFakedOrganizationService().Execute(A<LocalTimeFromUtcTimeRequest>._)).Returns(localTimeFromUtcTimeResponse);
            var mockDomService = new DOMService(mockLogger.Object, mockILanguageService.Object, xrmFakedContext.GetFakedOrganizationService(), mockConfigurationService);
            
           
            mockDomService.ProcessDomAssignment("opportunity", xrmFakedContext.GetFakedOrganizationService());
            #endregion

            #region ASSERT
            Assert.IsTrue(xrmFakedContext.Data["post"] != null);
            #endregion ASSERT  
        }

        [TestCategory("Function"), TestCategory("Positive")]
        [TestMethod]
        public void DomEngineOpportunity_ConditionType_IsRecordOwner()
        {
            #region ARRANGE
            var bingMapKeyConfigInstance = GetConfiguration();

            var xrmFakedContext = new XrmFakedContext { ProxyTypesAssembly = Assembly.Load("Cmc.Engage.Contracts.Tests") };
            var calledId = Guid.NewGuid();
            xrmFakedContext.CallerId = new EntityReference("SystemUser", calledId);
            var timezonedefinition = new Entity("timezonedefinition", Guid.NewGuid())
            {
                ["timezonecode"] = 190,
                ["standardname"] = "India Standard Time"
            };
            var systemUser = new Entity("SystemUser", xrmFakedContext.CallerId.Id)
            {
                ["systemuserid"] = xrmFakedContext.CallerId.Id,
            };

            var systemUser2 = new Entity("SystemUser", Guid.NewGuid());

            var list = GetList(systemUser);
            var domMaster = Getcmc_dommaster2(systemUser, list.ToEntityReference(), systemUser2);
            var domdefinitionexecutionorder = Getdomdefinitionexecutionorder(systemUser, domMaster.Id);
            var opportunity = GetOpportunityEntity(systemUser);

            var domdefinition = Getdomdefinition(domMaster.ToEntityReference(), systemUser);
            var domdefinitionLogic = GetdomdefinitionLogic3(domdefinition.ToEntityReference(), systemUser);

            var contact = new Entity("contact", systemUser.Id)
            {
                ["firstname"] = "Cameron2",
                //["contactid"] = systemUser.ToEntityReference()
            };
            var mockUserSettings = new Entity("usersettings", Guid.NewGuid())
            {
                ["localeid"] = 1033,
                ["systemuserid"] = systemUser.ToEntityReference(),
                ["timezonecode"] = timezonedefinition.GetAttributeValue<int>("timezonecode")
            };
            xrmFakedContext.AddRelationship("user_settings", new XrmFakedRelationship
            {
                RelationshipType = XrmFakedRelationship.enmFakeRelationshipType.OneToMany,
                Entity1LogicalName = "systemuser",
                Entity1Attribute = "systemuserid",
                Entity2LogicalName = "usersettings",
                Entity2Attribute = "systemuserid"
            });
            var assocationListMember = new Entity("listmember", Guid.NewGuid())
            {
                ["entityid"] = contact.Id,
                ["listid"] = list.Id
            };
            xrmFakedContext.AddRelationship("listcontact_association", new XrmFakedRelationship
            {
                IntersectEntity = "listmember",
                RelationshipType = XrmFakedRelationship.enmFakeRelationshipType.ManyToMany,
                Entity1LogicalName = List.EntityLogicalName,
                Entity1Attribute = "listid",
                Entity2LogicalName = Contact.EntityLogicalName,
                Entity2Attribute = "entityid"
            });
            xrmFakedContext.AddRelationship("opportunitycompetitors_association", new XrmFakedRelationship
            {
                IntersectEntity = "opportunitycompetitors",
                RelationshipType = XrmFakedRelationship.enmFakeRelationshipType.ManyToMany,
                Entity1LogicalName = Opportunity.EntityLogicalName,
                Entity1Attribute = "opportunityid",
                Entity2LogicalName = "competitor",
                Entity2Attribute = "competitorid"
            });

            xrmFakedContext.AddRelationship("timezonedefinition", new XrmFakedRelationship
            {
                RelationshipType = XrmFakedRelationship.enmFakeRelationshipType.OneToMany,
                Entity1LogicalName = SystemUser.EntityLogicalName,
                Entity1Attribute = "timezonecode",
                Entity2LogicalName = "timezonedefinition",
                Entity2Attribute = "timezonecode"
            });

            xrmFakedContext.AddRelationship("cmc_contact_opportunity_contactid", new XrmFakedRelationship
            {
                RelationshipType = XrmFakedRelationship.enmFakeRelationshipType.OneToMany,
                Entity1LogicalName = "opportunity",
                Entity1Attribute = "cmc_contactid",
                Entity2LogicalName = "contact",
                Entity2Attribute = "contactid"
            });

            xrmFakedContext.Initialize(new List<Entity>
            {
                contact,
                assocationListMember,
                timezonedefinition,
                mockUserSettings,
                systemUser,
                bingMapKeyConfigInstance,

                domMaster,
                domdefinitionexecutionorder,
                list,
                opportunity,
                domdefinition,
                domdefinitionLogic

            });
            #endregion

            #region ACT
            var mockLogger = new Mock<ILogger>();
            var mockILanguageService = new Mock<ILanguageService>();
            var mockConfigurationService = new ConfigurationService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
            var localTimeFromUtcTimeResponse = new LocalTimeFromUtcTimeResponse
            {
                Results = new ParameterCollection
                {
                    { "LocalTime",DateTime.Now}
                }
            };
            var responses = new ExecuteMultipleResponseItemCollection();
            responses.Add(new ExecuteMultipleResponseItem()
            {
                Fault = null
            });
            var executeMultipleResponse = new ExecuteMultipleResponse
            {
                Results = new ParameterCollection
                {
                    { "Responses",responses}
                }
            };
            var entityMetadata = new EntityMetadata() { LogicalName = "contact", MetadataId = Guid.NewGuid() };
            var attributeMetadata = new LookupAttributeMetadata()
            {
                LogicalName = "contact",
                SchemaName = "contact",
                DisplayName = new Label("owner", 1033),
                Targets = new[] { "systemuser" },
            };
            var userLocalizedLabel = new LocalizedLabel("systemuser", 1033);
            LocalizedLabel[] labels = { userLocalizedLabel };
            entityMetadata.SetAttributeCollection(new List<AttributeMetadata>()
            {
                new UniqueIdentifierAttributeMetadata("contactid") {LogicalName =   "contactid"},
                new LookupAttributeMetadata(){

                    LogicalName =   "ownerid",
                    SchemaName = "owner",
                    DisplayName = new Label(userLocalizedLabel,labels),
                    Targets = new []{ "systemuser" },
                },
            });

            var primaryNameAttribute = entityMetadata.GetType().GetProperty("PrimaryIdAttribute");
            if (primaryNameAttribute != null) primaryNameAttribute.SetValue(entityMetadata, "contactid");

            QueryExpression contactquery = new QueryExpression()
            {
                EntityName = Contact.EntityLogicalName
            };
            var fetchXmlToQueryExpressionResponse = new FetchXmlToQueryExpressionResponse()
            {
                Results = new ParameterCollection
                        {
                            { "Query", contactquery}
                        }
            };
            var retrieveEntityResponse = new RetrieveEntityResponse()
            {
                Results = new ParameterCollection
                {
                    { "EntityMetadata", entityMetadata }
                }
            };
            var retrieveAttributedata = new RetrieveAttributeResponse()
            {
                Results = new ParameterCollection()
                {
                    { "AttributeMetadata", attributeMetadata }
                }
            };

            //A.CallTo(() => entityMetadata.PrimaryNameAttribute).Returns("firstname");

            A.CallTo(() => xrmFakedContext.GetFakedOrganizationService().Execute(A<RetrieveEntityRequest>.Ignored)).Returns(retrieveEntityResponse);
            A.CallTo(() => xrmFakedContext.GetFakedOrganizationService().Execute(A<RetrieveAttributeRequest>.Ignored)).Returns(retrieveAttributedata);

            //Excute FetchXmlToQueryExpressionRequest if the name contain contact
            A.CallTo(() => xrmFakedContext.GetFakedOrganizationService().Execute(A<FetchXmlToQueryExpressionRequest>.That.Matches(r => r.FetchXml.Contains("name='contact'"))))
                .ReturnsLazily((req) => { return fetchXmlToQueryExpressionResponse; });

            xrmFakedContext.InitializeMetadata(entityMetadata);

            A.CallTo(() => xrmFakedContext.GetFakedOrganizationService().Execute(A<LocalTimeFromUtcTimeRequest>._)).Returns(localTimeFromUtcTimeResponse);
            var mockDomService = new DOMService(mockLogger.Object, mockILanguageService.Object, xrmFakedContext.GetFakedOrganizationService(), mockConfigurationService);
           
           
            mockDomService.ProcessDomAssignment("opportunity", xrmFakedContext.GetFakedOrganizationService());
            #endregion

            #region ASSERT
            Assert.IsTrue(xrmFakedContext.Data["post"] != null);
            #endregion ASSERT  
        }

        [TestCategory("Function"), TestCategory("Positive")]
        [TestMethod]
        public void DomEngineOpportunity_ConditionType_IsRange()
        {
            #region ARRANGE
            var bingMapKeyConfigInstance = GetConfiguration();

            var xrmFakedContext = new XrmFakedContext { ProxyTypesAssembly = Assembly.Load("Cmc.Engage.Contracts.Tests") };
            var calledId = Guid.NewGuid();
            xrmFakedContext.CallerId = new EntityReference("SystemUser", calledId);
            var timezonedefinition = new Entity("timezonedefinition", Guid.NewGuid())
            {
                ["timezonecode"] = 190,
                ["standardname"] = "India Standard Time"
            };
            var systemUser = new Entity("SystemUser", xrmFakedContext.CallerId.Id)
            {
                ["systemuserid"] = xrmFakedContext.CallerId.Id,
            };

            var systemUser2 = new Entity("SystemUser", Guid.NewGuid());

            var list = GetList(systemUser);
            var domMaster = Getcmc_dommaster2(systemUser, list.ToEntityReference(), systemUser2);
            var domdefinitionexecutionorder = Getdomdefinitionexecutionorder(systemUser, domMaster.Id);
            var opportunity = GetOpportunityEntity(systemUser);

            var domdefinition = Getdomdefinition(domMaster.ToEntityReference(), systemUser);
            var domdefinitionLogic = GetdomdefinitionLogic4(domdefinition.ToEntityReference(), systemUser);

            var contact = new Entity("contact", systemUser.Id)
            {
                ["firstname"] = "Cameron2",
                //["contactid"] = systemUser.ToEntityReference()
            };
            var mockUserSettings = new Entity("usersettings", Guid.NewGuid())
            {
                ["localeid"] = 1033,
                ["systemuserid"] = systemUser.ToEntityReference(),
                ["timezonecode"] = timezonedefinition.GetAttributeValue<int>("timezonecode")
            };
            xrmFakedContext.AddRelationship("user_settings", new XrmFakedRelationship
            {
                RelationshipType = XrmFakedRelationship.enmFakeRelationshipType.OneToMany,
                Entity1LogicalName = "systemuser",
                Entity1Attribute = "systemuserid",
                Entity2LogicalName = "usersettings",
                Entity2Attribute = "systemuserid"
            });
            var assocationListMember = new Entity("listmember", Guid.NewGuid())
            {
                ["entityid"] = contact.Id,
                ["listid"] = list.Id
            };
            xrmFakedContext.AddRelationship("listcontact_association", new XrmFakedRelationship
            {
                IntersectEntity = "listmember",
                RelationshipType = XrmFakedRelationship.enmFakeRelationshipType.ManyToMany,
                Entity1LogicalName = List.EntityLogicalName,
                Entity1Attribute = "listid",
                Entity2LogicalName = Contact.EntityLogicalName,
                Entity2Attribute = "entityid"
            });
            xrmFakedContext.AddRelationship("opportunitycompetitors_association", new XrmFakedRelationship
            {
                IntersectEntity = "opportunitycompetitors",
                RelationshipType = XrmFakedRelationship.enmFakeRelationshipType.ManyToMany,
                Entity1LogicalName = Opportunity.EntityLogicalName,
                Entity1Attribute = "opportunityid",
                Entity2LogicalName = "competitor",
                Entity2Attribute = "competitorid"
            });

            xrmFakedContext.AddRelationship("timezonedefinition", new XrmFakedRelationship
            {
                RelationshipType = XrmFakedRelationship.enmFakeRelationshipType.OneToMany,
                Entity1LogicalName = SystemUser.EntityLogicalName,
                Entity1Attribute = "timezonecode",
                Entity2LogicalName = "timezonedefinition",
                Entity2Attribute = "timezonecode"
            });

            xrmFakedContext.AddRelationship("cmc_contact_opportunity_contactid", new XrmFakedRelationship
            {
                RelationshipType = XrmFakedRelationship.enmFakeRelationshipType.OneToMany,
                Entity1LogicalName = "opportunity",
                Entity1Attribute = "cmc_contactid",
                Entity2LogicalName = "contact",
                Entity2Attribute = "contactid"
            });

            xrmFakedContext.Initialize(new List<Entity>
            {
                contact,
                assocationListMember,
                timezonedefinition,
                mockUserSettings,
                systemUser,
                bingMapKeyConfigInstance,

                domMaster,
                domdefinitionexecutionorder,
                list,
                opportunity,
                domdefinition,
                domdefinitionLogic

            });
            #endregion

            #region ACT
            var mockLogger = new Mock<ILogger>();
            var mockILanguageService = new Mock<ILanguageService>();
            var mockConfigurationService = new ConfigurationService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
            var localTimeFromUtcTimeResponse = new LocalTimeFromUtcTimeResponse
            {
                Results = new ParameterCollection
                {
                    { "LocalTime",DateTime.Now}
                }
            };
            var responses = new ExecuteMultipleResponseItemCollection();
            responses.Add(new ExecuteMultipleResponseItem()
            {
                Fault = null
            });
            var executeMultipleResponse = new ExecuteMultipleResponse
            {
                Results = new ParameterCollection
                {
                    { "Responses",responses}
                }
            };
            var entityMetadata = new EntityMetadata() { LogicalName = "contact", MetadataId = Guid.NewGuid() };
            var attributeMetadata = new DateTimeAttributeMetadata()
            {
                LogicalName = "contact",
                SchemaName = "contact",
                DisplayName = new Label("owner", 1033),
                DateTimeBehavior = "UserLocal"
                //Targets = new[] { "systemuser" },
            };
            var userLocalizedLabel = new LocalizedLabel("systemuser", 1033);
            LocalizedLabel[] labels = { userLocalizedLabel };
            entityMetadata.SetAttributeCollection(new List<AttributeMetadata>()
            {
                new UniqueIdentifierAttributeMetadata("contactid") {LogicalName =   "contactid"},
                new DateTimeAttributeMetadata(){

                    LogicalName =   "ownerid",
                    SchemaName = "owner",
                    DisplayName = new Label(userLocalizedLabel,labels),
                    //Targets = new []{ "systemuser" },
                },
            });

            var primaryNameAttribute = entityMetadata.GetType().GetProperty("PrimaryNameAttribute");
            if (primaryNameAttribute != null) primaryNameAttribute.SetValue(entityMetadata, "contact");

            QueryExpression contactquery = new QueryExpression()
            {
                EntityName = Contact.EntityLogicalName
            };
            var fetchXmlToQueryExpressionResponse = new FetchXmlToQueryExpressionResponse()
            {
                Results = new ParameterCollection
                        {
                            { "Query", contactquery}
                        }
            };
            var retrieveEntityResponse = new RetrieveEntityResponse()
            {
                Results = new ParameterCollection
                {
                    { "EntityMetadata", entityMetadata }
                }
            };
            var retrieveAttributedata = new RetrieveAttributeResponse()
            {
                Results = new ParameterCollection()
                {
                    { "AttributeMetadata", attributeMetadata }
                }
            };

            //A.CallTo(() => entityMetadata.PrimaryNameAttribute).Returns("firstname");

            A.CallTo(() => xrmFakedContext.GetFakedOrganizationService().Execute(A<RetrieveEntityRequest>.Ignored)).Returns(retrieveEntityResponse);
            A.CallTo(() => xrmFakedContext.GetFakedOrganizationService().Execute(A<RetrieveAttributeRequest>.Ignored)).Returns(retrieveAttributedata);

            //Excute FetchXmlToQueryExpressionRequest if the name contain contact
            A.CallTo(() => xrmFakedContext.GetFakedOrganizationService().Execute(A<FetchXmlToQueryExpressionRequest>.That.Matches(r => r.FetchXml.Contains("name='contact'"))))
                .ReturnsLazily((req) => { return fetchXmlToQueryExpressionResponse; });

            xrmFakedContext.InitializeMetadata(entityMetadata);

            A.CallTo(() => xrmFakedContext.GetFakedOrganizationService().Execute(A<LocalTimeFromUtcTimeRequest>._)).Returns(localTimeFromUtcTimeResponse);
            var mockDomService = new DOMService(mockLogger.Object, mockILanguageService.Object, xrmFakedContext.GetFakedOrganizationService(), mockConfigurationService);
          
            mockDomService.ProcessDomAssignment("opportunity", xrmFakedContext.GetFakedOrganizationService());
            #endregion

            #region ASSERT
            Assert.IsTrue(xrmFakedContext.Data["post"] != null);
            #endregion ASSERT  
        }

        [TestCategory("Function"), TestCategory("Positive")]
        [TestMethod]
        public void DomEngineOpportunity_ConditionType_IsEquals_AttributeTypeIsNotLookup_TrueOptionLabel()
        {
            #region ARRANGE
            var bingMapKeyConfigInstance = GetConfiguration();

            var xrmFakedContext = new XrmFakedContext { ProxyTypesAssembly = Assembly.Load("Cmc.Engage.Contracts.Tests") };
            var calledId = Guid.NewGuid();
            xrmFakedContext.CallerId = new EntityReference("SystemUser", calledId);
            var timezonedefinition = new Entity("timezonedefinition", Guid.NewGuid())
            {
                ["timezonecode"] = 190,
                ["standardname"] = "India Standard Time"
            };
            var systemUser = new Entity("SystemUser", xrmFakedContext.CallerId.Id)
            {
                ["systemuserid"] = xrmFakedContext.CallerId.Id,
            };

            var systemUser2 = new Entity("SystemUser", Guid.NewGuid());

            var list = GetList(systemUser);
            var domMaster = Getcmc_dommaster2(systemUser, list.ToEntityReference(), systemUser2);
            var domdefinitionexecutionorder = Getdomdefinitionexecutionorder(systemUser, domMaster.Id);
            var opportunity = GetOpportunityEntity(systemUser);

            var domdefinition = Getdomdefinition(domMaster.ToEntityReference(), systemUser);
            var domdefinitionLogic = GetdomdefinitionLogic5(domdefinition.ToEntityReference(), systemUser);

            var contact = new Entity("contact", systemUser.Id)
            {
                ["firstname"] = "Cameron2",
                //["contactid"] = systemUser.ToEntityReference()
            };
            var mockUserSettings = new Entity("usersettings", Guid.NewGuid())
            {
                ["localeid"] = 1033,
                ["systemuserid"] = systemUser.ToEntityReference(),
                ["timezonecode"] = timezonedefinition.GetAttributeValue<int>("timezonecode")
            };
            xrmFakedContext.AddRelationship("user_settings", new XrmFakedRelationship
            {
                RelationshipType = XrmFakedRelationship.enmFakeRelationshipType.OneToMany,
                Entity1LogicalName = "systemuser",
                Entity1Attribute = "systemuserid",
                Entity2LogicalName = "usersettings",
                Entity2Attribute = "systemuserid"
            });
            var assocationListMember = new Entity("listmember", Guid.NewGuid())
            {
                ["entityid"] = contact.Id,
                ["listid"] = list.Id
            };
            xrmFakedContext.AddRelationship("listcontact_association", new XrmFakedRelationship
            {
                IntersectEntity = "listmember",
                RelationshipType = XrmFakedRelationship.enmFakeRelationshipType.ManyToMany,
                Entity1LogicalName = List.EntityLogicalName,
                Entity1Attribute = "listid",
                Entity2LogicalName = Contact.EntityLogicalName,
                Entity2Attribute = "entityid"
            });
            xrmFakedContext.AddRelationship("opportunitycompetitors_association", new XrmFakedRelationship
            {
                IntersectEntity = "opportunitycompetitors",
                RelationshipType = XrmFakedRelationship.enmFakeRelationshipType.ManyToMany,
                Entity1LogicalName = Opportunity.EntityLogicalName,
                Entity1Attribute = "opportunityid",
                Entity2LogicalName = "competitor",
                Entity2Attribute = "competitorid"
            });

            xrmFakedContext.AddRelationship("timezonedefinition", new XrmFakedRelationship
            {
                RelationshipType = XrmFakedRelationship.enmFakeRelationshipType.OneToMany,
                Entity1LogicalName = SystemUser.EntityLogicalName,
                Entity1Attribute = "timezonecode",
                Entity2LogicalName = "timezonedefinition",
                Entity2Attribute = "timezonecode"
            });

            xrmFakedContext.AddRelationship("cmc_contact_opportunity_contactid", new XrmFakedRelationship
            {
                RelationshipType = XrmFakedRelationship.enmFakeRelationshipType.OneToMany,
                Entity1LogicalName = "opportunity",
                Entity1Attribute = "cmc_contactid",
                Entity2LogicalName = "contact",
                Entity2Attribute = "contactid"
            });

            xrmFakedContext.Initialize(new List<Entity>
            {
                contact,
                assocationListMember,
                timezonedefinition,
                mockUserSettings,
                systemUser,
                bingMapKeyConfigInstance,

                domMaster,
                domdefinitionexecutionorder,
                list,
                opportunity,
                domdefinition,
                domdefinitionLogic

            });
            #endregion

            #region ACT
            var mockLogger = new Mock<ILogger>();
            var mockILanguageService = new Mock<ILanguageService>();
            var mockConfigurationService = new ConfigurationService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
            var localTimeFromUtcTimeResponse = new LocalTimeFromUtcTimeResponse
            {
                Results = new ParameterCollection
                {
                    { "LocalTime",DateTime.Now}
                }
            };
            var responses = new ExecuteMultipleResponseItemCollection();
            responses.Add(new ExecuteMultipleResponseItem()
            {
                Fault = null
            });
            var executeMultipleResponse = new ExecuteMultipleResponse
            {
                Results = new ParameterCollection
                {
                    { "Responses",responses}
                }
            };
            var entityMetadata = new EntityMetadata() { LogicalName = "contact", MetadataId = Guid.NewGuid() };

            var userLocalizedLabel = new LocalizedLabel("systemuser", 1033);
            LocalizedLabel[] labels = { userLocalizedLabel };
            var attributeMetadata = new BooleanAttributeMetadata()
            {
                OptionSet = new BooleanOptionSetMetadata()
                {
                    TrueOption = new OptionMetadata()
                    {
                        Label = new Label(userLocalizedLabel, labels),
                        Value = 5
                    },
                    FalseOption = new OptionMetadata()
                    {
                        Label = new Label(userLocalizedLabel, labels),
                        Value = 7
                    }
                },

                LogicalName = "contact",
                SchemaName = "contact",
                DisplayName = new Label("owner", 1033),

            };
            entityMetadata.SetAttributeCollection(new List<AttributeMetadata>()
            {
                new UniqueIdentifierAttributeMetadata("contactid") {LogicalName =   "contactid"},
                new BooleanAttributeMetadata(){

                    LogicalName =   "ownerid",
                    SchemaName = "owner",
                    DisplayName = new Label(userLocalizedLabel,labels),

                },
            });

            entityMetadata.SetAttribute(attributeMetadata);
            var primaryNameAttribute = entityMetadata.GetType().GetProperty("PrimaryNameAttribute");
            if (primaryNameAttribute != null) primaryNameAttribute.SetValue(entityMetadata, "contact");

            QueryExpression contactquery = new QueryExpression()
            {
                EntityName = Contact.EntityLogicalName
            };
            var fetchXmlToQueryExpressionResponse = new FetchXmlToQueryExpressionResponse()
            {
                Results = new ParameterCollection
                        {
                            { "Query", contactquery}
                        }
            };
            var retrieveEntityResponse = new RetrieveEntityResponse()
            {
                Results = new ParameterCollection
                {
                    { "EntityMetadata", entityMetadata }
                }
            };
            var retrieveAttributedata = new RetrieveAttributeResponse()
            {
                Results = new ParameterCollection()
                {
                    { "AttributeMetadata", attributeMetadata }
                }
            };

            //A.CallTo(() => entityMetadata.PrimaryNameAttribute).Returns("firstname");

            A.CallTo(() => xrmFakedContext.GetFakedOrganizationService().Execute(A<RetrieveEntityRequest>.Ignored)).Returns(retrieveEntityResponse);
            A.CallTo(() => xrmFakedContext.GetFakedOrganizationService().Execute(A<RetrieveAttributeRequest>.Ignored)).Returns(retrieveAttributedata);

            //Excute FetchXmlToQueryExpressionRequest if the name contain contact
            A.CallTo(() => xrmFakedContext.GetFakedOrganizationService().Execute(A<FetchXmlToQueryExpressionRequest>.That.Matches(r => r.FetchXml.Contains("name='contact'"))))
                .ReturnsLazily((req) => { return fetchXmlToQueryExpressionResponse; });

            xrmFakedContext.InitializeMetadata(entityMetadata);

            A.CallTo(() => xrmFakedContext.GetFakedOrganizationService().Execute(A<LocalTimeFromUtcTimeRequest>._)).Returns(localTimeFromUtcTimeResponse);
            var mockDomService = new DOMService(mockLogger.Object, mockILanguageService.Object, xrmFakedContext.GetFakedOrganizationService(), mockConfigurationService);
          
            mockDomService.ProcessDomAssignment("opportunity", xrmFakedContext.GetFakedOrganizationService());
            #endregion

            #region ASSERT
            //var fetchXml = $@"<fetch>
            //                    <entity name='post'>
            //                          <all-attributes/>                                               
            //                    </entity>
            //                </fetch>";
            //var data = xrmFakedContext.GetFakedOrganizationService().RetrieveMultipleAll(fetchXml)?.Entities?.FirstOrDefault();
            //Assert.IsTrue(data != null);
            Assert.IsTrue(xrmFakedContext.Data["post"] != null);
            #endregion ASSERT  
        }

        [TestCategory("Function"), TestCategory("Positive")]
        [TestMethod]
        public void DomEngineOpportunity_ConditionType_IsEquals_AttributeTypeIsNotLookup_FalseOptionLabel()
        {
            #region ARRANGE
            var bingMapKeyConfigInstance = GetConfiguration();

            var xrmFakedContext = new XrmFakedContext { ProxyTypesAssembly = Assembly.Load("Cmc.Engage.Contracts.Tests") };
            var calledId = Guid.NewGuid();
            xrmFakedContext.CallerId = new EntityReference("SystemUser", calledId);
            var timezonedefinition = new Entity("timezonedefinition", Guid.NewGuid())
            {
                ["timezonecode"] = 190,
                ["standardname"] = "India Standard Time"
            };
            var systemUser = new Entity("SystemUser", xrmFakedContext.CallerId.Id)
            {
                ["systemuserid"] = xrmFakedContext.CallerId.Id,
            };

            var systemUser2 = new Entity("SystemUser", Guid.NewGuid());

            var list = GetList(systemUser);
            var domMaster = Getcmc_dommaster2(systemUser, list.ToEntityReference(), systemUser2);
            var domdefinitionexecutionorder = Getdomdefinitionexecutionorder(systemUser, domMaster.Id);
            var opportunity = GetOpportunityEntity(systemUser);

            var domdefinition = Getdomdefinition(domMaster.ToEntityReference(), systemUser);
            var domdefinitionLogic = GetdomdefinitionLogic5(domdefinition.ToEntityReference(), systemUser);

            var contact = new Entity("contact", systemUser.Id)
            {
                ["firstname"] = "Cameron2",
                //["contactid"] = systemUser.ToEntityReference()
            };
            var mockUserSettings = new Entity("usersettings", Guid.NewGuid())
            {
                ["localeid"] = 1033,
                ["systemuserid"] = systemUser.ToEntityReference(),
                ["timezonecode"] = timezonedefinition.GetAttributeValue<int>("timezonecode")
            };
            xrmFakedContext.AddRelationship("user_settings", new XrmFakedRelationship
            {
                RelationshipType = XrmFakedRelationship.enmFakeRelationshipType.OneToMany,
                Entity1LogicalName = "systemuser",
                Entity1Attribute = "systemuserid",
                Entity2LogicalName = "usersettings",
                Entity2Attribute = "systemuserid"
            });
            var assocationListMember = new Entity("listmember", Guid.NewGuid())
            {
                ["entityid"] = contact.Id,
                ["listid"] = list.Id
            };
            xrmFakedContext.AddRelationship("listcontact_association", new XrmFakedRelationship
            {
                IntersectEntity = "listmember",
                RelationshipType = XrmFakedRelationship.enmFakeRelationshipType.ManyToMany,
                Entity1LogicalName = List.EntityLogicalName,
                Entity1Attribute = "listid",
                Entity2LogicalName = Contact.EntityLogicalName,
                Entity2Attribute = "entityid"
            });
            xrmFakedContext.AddRelationship("opportunitycompetitors_association", new XrmFakedRelationship
            {
                IntersectEntity = "opportunitycompetitors",
                RelationshipType = XrmFakedRelationship.enmFakeRelationshipType.ManyToMany,
                Entity1LogicalName = Opportunity.EntityLogicalName,
                Entity1Attribute = "opportunityid",
                Entity2LogicalName = "competitor",
                Entity2Attribute = "competitorid"
            });

            xrmFakedContext.AddRelationship("timezonedefinition", new XrmFakedRelationship
            {
                RelationshipType = XrmFakedRelationship.enmFakeRelationshipType.OneToMany,
                Entity1LogicalName = SystemUser.EntityLogicalName,
                Entity1Attribute = "timezonecode",
                Entity2LogicalName = "timezonedefinition",
                Entity2Attribute = "timezonecode"
            });

            xrmFakedContext.AddRelationship("cmc_contact_opportunity_contactid", new XrmFakedRelationship
            {
                RelationshipType = XrmFakedRelationship.enmFakeRelationshipType.OneToMany,
                Entity1LogicalName = "opportunity",
                Entity1Attribute = "cmc_contactid",
                Entity2LogicalName = "contact",
                Entity2Attribute = "contactid"
            });

            xrmFakedContext.Initialize(new List<Entity>
            {
                contact,
                assocationListMember,
                timezonedefinition,
                mockUserSettings,
                systemUser,
                bingMapKeyConfigInstance,

                domMaster,
                domdefinitionexecutionorder,
                list,
                opportunity,
                domdefinition,
                domdefinitionLogic

            });
            #endregion

            #region ACT
            var mockLogger = new Mock<ILogger>();
            var mockILanguageService = new Mock<ILanguageService>();
            var mockConfigurationService = new ConfigurationService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
            var localTimeFromUtcTimeResponse = new LocalTimeFromUtcTimeResponse
            {
                Results = new ParameterCollection
                {
                    { "LocalTime",DateTime.Now}
                }
            };
            var responses = new ExecuteMultipleResponseItemCollection();
            responses.Add(new ExecuteMultipleResponseItem()
            {
                Fault = null
            });
            var executeMultipleResponse = new ExecuteMultipleResponse
            {
                Results = new ParameterCollection
                {
                    { "Responses",responses}
                }
            };
            var entityMetadata = new EntityMetadata() { LogicalName = "contact", MetadataId = Guid.NewGuid() };

            var userLocalizedLabel = new LocalizedLabel("contact", 1033);
            LocalizedLabel[] labels = { userLocalizedLabel };
            var userLocalizedLabel1 = new LocalizedLabel("systemuser", 1033);
            LocalizedLabel[] labels1 = { userLocalizedLabel1 };
            var attributeMetadata = new BooleanAttributeMetadata()
            {
                OptionSet = new BooleanOptionSetMetadata()
                {
                    TrueOption = new OptionMetadata()
                    {
                        Label = new Label(userLocalizedLabel, labels),
                        Value = 5
                    },
                    FalseOption = new OptionMetadata()
                    {
                        Label = new Label(userLocalizedLabel1, labels1),
                        Value = 7
                    }
                },

                LogicalName = "contact",
                SchemaName = "contact",
                DisplayName = new Label("owner", 1033),

            };
            entityMetadata.SetAttributeCollection(new List<AttributeMetadata>()
            {
                new UniqueIdentifierAttributeMetadata("contactid") {LogicalName =   "contactid"},
                new BooleanAttributeMetadata(){

                    LogicalName =   "ownerid",
                    SchemaName = "owner",
                    DisplayName = new Label(userLocalizedLabel,labels),

                },
            });

            entityMetadata.SetAttribute(attributeMetadata);
            var primaryNameAttribute = entityMetadata.GetType().GetProperty("PrimaryNameAttribute");
            if (primaryNameAttribute != null) primaryNameAttribute.SetValue(entityMetadata, "contact");

            QueryExpression contactquery = new QueryExpression()
            {
                EntityName = Contact.EntityLogicalName
            };
            var fetchXmlToQueryExpressionResponse = new FetchXmlToQueryExpressionResponse()
            {
                Results = new ParameterCollection
                        {
                            { "Query", contactquery}
                        }
            };
            var retrieveEntityResponse = new RetrieveEntityResponse()
            {
                Results = new ParameterCollection
                {
                    { "EntityMetadata", entityMetadata }
                }
            };
            var retrieveAttributedata = new RetrieveAttributeResponse()
            {
                Results = new ParameterCollection()
                {
                    { "AttributeMetadata", attributeMetadata }
                }
            };

            //A.CallTo(() => entityMetadata.PrimaryNameAttribute).Returns("firstname");

            A.CallTo(() => xrmFakedContext.GetFakedOrganizationService().Execute(A<RetrieveEntityRequest>.Ignored)).Returns(retrieveEntityResponse);
            A.CallTo(() => xrmFakedContext.GetFakedOrganizationService().Execute(A<RetrieveAttributeRequest>.Ignored)).Returns(retrieveAttributedata);

            //Excute FetchXmlToQueryExpressionRequest if the name contain contact
            A.CallTo(() => xrmFakedContext.GetFakedOrganizationService().Execute(A<FetchXmlToQueryExpressionRequest>.That.Matches(r => r.FetchXml.Contains("name='contact'"))))
                .ReturnsLazily((req) => { return fetchXmlToQueryExpressionResponse; });

            xrmFakedContext.InitializeMetadata(entityMetadata);

            A.CallTo(() => xrmFakedContext.GetFakedOrganizationService().Execute(A<LocalTimeFromUtcTimeRequest>._)).Returns(localTimeFromUtcTimeResponse);
            var mockDomService = new DOMService(mockLogger.Object, mockILanguageService.Object, xrmFakedContext.GetFakedOrganizationService(), mockConfigurationService);
           
            mockDomService.ProcessDomAssignment("opportunity", xrmFakedContext.GetFakedOrganizationService());
            #endregion

            #region ASSERT
            //var fetchXml = $@"<fetch>
            //                    <entity name='post'>
            //                          <all-attributes/>                                               
            //                    </entity>
            //                </fetch>";
            //var data = xrmFakedContext.GetFakedOrganizationService().RetrieveMultipleAll(fetchXml)?.Entities?.FirstOrDefault();
            //Assert.IsTrue(data != null);
            Assert.IsTrue(xrmFakedContext.Data["post"] != null);
            #endregion ASSERT  
        }

        [TestCategory("Function"), TestCategory("Positive")]
        [TestMethod]
        public void DomEngineOpportunity_ConditionType_IsEquals_AttributeTypeIsNotLookup_InvalidLabel()
        {
            #region ARRANGE
            var bingMapKeyConfigInstance = GetConfiguration();

            var xrmFakedContext = new XrmFakedContext { ProxyTypesAssembly = Assembly.Load("Cmc.Engage.Contracts.Tests") };
            var calledId = Guid.NewGuid();
            xrmFakedContext.CallerId = new EntityReference("SystemUser", calledId);
            var timezonedefinition = new Entity("timezonedefinition", Guid.NewGuid())
            {
                ["timezonecode"] = 190,
                ["standardname"] = "India Standard Time"
            };
            var systemUser = new Entity("SystemUser", xrmFakedContext.CallerId.Id)
            {
                ["systemuserid"] = xrmFakedContext.CallerId.Id,
            };

            var systemUser2 = new Entity("SystemUser", Guid.NewGuid());

            var list = GetList(systemUser);
            var domMaster = Getcmc_dommaster2(systemUser, list.ToEntityReference(), systemUser2);
            var domdefinitionexecutionorder = Getdomdefinitionexecutionorder(systemUser, domMaster.Id);
            var opportunity = GetOpportunityEntity(systemUser);

            var domdefinition = Getdomdefinition(domMaster.ToEntityReference(), systemUser);
            var domdefinitionLogic = GetdomdefinitionLogic5(domdefinition.ToEntityReference(), systemUser);

            var contact = new Entity("contact", systemUser.Id)
            {
                ["firstname"] = "Cameron2",
                //["contactid"] = systemUser.ToEntityReference()
            };
            var mockUserSettings = new Entity("usersettings", Guid.NewGuid())
            {
                ["localeid"] = 1033,
                ["systemuserid"] = systemUser.ToEntityReference(),
                ["timezonecode"] = timezonedefinition.GetAttributeValue<int>("timezonecode")
            };
            xrmFakedContext.AddRelationship("user_settings", new XrmFakedRelationship
            {
                RelationshipType = XrmFakedRelationship.enmFakeRelationshipType.OneToMany,
                Entity1LogicalName = "systemuser",
                Entity1Attribute = "systemuserid",
                Entity2LogicalName = "usersettings",
                Entity2Attribute = "systemuserid"
            });
            var assocationListMember = new Entity("listmember", Guid.NewGuid())
            {
                ["entityid"] = contact.Id,
                ["listid"] = list.Id
            };
            xrmFakedContext.AddRelationship("listcontact_association", new XrmFakedRelationship
            {
                IntersectEntity = "listmember",
                RelationshipType = XrmFakedRelationship.enmFakeRelationshipType.ManyToMany,
                Entity1LogicalName = List.EntityLogicalName,
                Entity1Attribute = "listid",
                Entity2LogicalName = Contact.EntityLogicalName,
                Entity2Attribute = "entityid"
            });
            xrmFakedContext.AddRelationship("opportunitycompetitors_association", new XrmFakedRelationship
            {
                IntersectEntity = "opportunitycompetitors",
                RelationshipType = XrmFakedRelationship.enmFakeRelationshipType.ManyToMany,
                Entity1LogicalName = Opportunity.EntityLogicalName,
                Entity1Attribute = "opportunityid",
                Entity2LogicalName = "competitor",
                Entity2Attribute = "competitorid"
            });

            xrmFakedContext.AddRelationship("timezonedefinition", new XrmFakedRelationship
            {
                RelationshipType = XrmFakedRelationship.enmFakeRelationshipType.OneToMany,
                Entity1LogicalName = SystemUser.EntityLogicalName,
                Entity1Attribute = "timezonecode",
                Entity2LogicalName = "timezonedefinition",
                Entity2Attribute = "timezonecode"
            });

            xrmFakedContext.AddRelationship("cmc_contact_opportunity_contactid", new XrmFakedRelationship
            {
                RelationshipType = XrmFakedRelationship.enmFakeRelationshipType.OneToMany,
                Entity1LogicalName = "opportunity",
                Entity1Attribute = "cmc_contactid",
                Entity2LogicalName = "contact",
                Entity2Attribute = "contactid"
            });

            xrmFakedContext.Initialize(new List<Entity>
            {
                contact,
                assocationListMember,
                timezonedefinition,
                mockUserSettings,
                systemUser,
                bingMapKeyConfigInstance,

                domMaster,
                domdefinitionexecutionorder,
                list,
                opportunity,
                domdefinition,
                domdefinitionLogic

            });
            #endregion

            #region ACT
            var mockLogger = new Mock<ILogger>();
            var mockILanguageService = new Mock<ILanguageService>();
            var mockConfigurationService = new ConfigurationService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
            var localTimeFromUtcTimeResponse = new LocalTimeFromUtcTimeResponse
            {
                Results = new ParameterCollection
                {
                    { "LocalTime",DateTime.Now}
                }
            };
            var responses = new ExecuteMultipleResponseItemCollection();
            responses.Add(new ExecuteMultipleResponseItem()
            {
                Fault = null
            });
            var executeMultipleResponse = new ExecuteMultipleResponse
            {
                Results = new ParameterCollection
                {
                    { "Responses",responses}
                }
            };
            var entityMetadata = new EntityMetadata() { LogicalName = "contact", MetadataId = Guid.NewGuid() };

            var userLocalizedLabel = new LocalizedLabel("contact", 1033);
            LocalizedLabel[] labels = { userLocalizedLabel };
            
            var attributeMetadata = new BooleanAttributeMetadata()
            {
                OptionSet = new BooleanOptionSetMetadata()
                {
                    TrueOption = new OptionMetadata()
                    {
                        Label = new Label(userLocalizedLabel, labels),
                        Value = 5
                    },
                    FalseOption = new OptionMetadata()
                    {
                        Label = new Label(userLocalizedLabel, labels),
                        Value = 7
                    }
                },

                LogicalName = "contact",
                SchemaName = "contact",
                DisplayName = new Label("owner", 1033),

            };
            entityMetadata.SetAttributeCollection(new List<AttributeMetadata>()
            {
                new UniqueIdentifierAttributeMetadata("contactid") {LogicalName =   "contactid"},
                new BooleanAttributeMetadata(){

                    LogicalName =   "ownerid",
                    SchemaName = "owner",
                    DisplayName = new Label(userLocalizedLabel,labels),

                },
            });

            entityMetadata.SetAttribute(attributeMetadata);
            var primaryNameAttribute = entityMetadata.GetType().GetProperty("PrimaryNameAttribute");
            if (primaryNameAttribute != null) primaryNameAttribute.SetValue(entityMetadata, "contact");

            QueryExpression contactquery = new QueryExpression()
            {
                EntityName = Contact.EntityLogicalName
            };
            var fetchXmlToQueryExpressionResponse = new FetchXmlToQueryExpressionResponse()
            {
                Results = new ParameterCollection
                        {
                            { "Query", contactquery}
                        }
            };
            var retrieveEntityResponse = new RetrieveEntityResponse()
            {
                Results = new ParameterCollection
                {
                    { "EntityMetadata", entityMetadata }
                }
            };
            var retrieveAttributedata = new RetrieveAttributeResponse()
            {
                Results = new ParameterCollection()
                {
                    { "AttributeMetadata", attributeMetadata }
                }
            };

            //A.CallTo(() => entityMetadata.PrimaryNameAttribute).Returns("firstname");

            A.CallTo(() => xrmFakedContext.GetFakedOrganizationService().Execute(A<RetrieveEntityRequest>.Ignored)).Returns(retrieveEntityResponse);
            A.CallTo(() => xrmFakedContext.GetFakedOrganizationService().Execute(A<RetrieveAttributeRequest>.Ignored)).Returns(retrieveAttributedata);

            //Excute FetchXmlToQueryExpressionRequest if the name contain contact
            A.CallTo(() => xrmFakedContext.GetFakedOrganizationService().Execute(A<FetchXmlToQueryExpressionRequest>.That.Matches(r => r.FetchXml.Contains("name='contact'"))))
                .ReturnsLazily((req) => { return fetchXmlToQueryExpressionResponse; });

            xrmFakedContext.InitializeMetadata(entityMetadata);

            A.CallTo(() => xrmFakedContext.GetFakedOrganizationService().Execute(A<LocalTimeFromUtcTimeRequest>._)).Returns(localTimeFromUtcTimeResponse);
            var mockDomService = new DOMService(mockLogger.Object, mockILanguageService.Object, xrmFakedContext.GetFakedOrganizationService(), mockConfigurationService);
           
            mockDomService.ProcessDomAssignment("opportunity", xrmFakedContext.GetFakedOrganizationService());
            #endregion

            #region ASSERT
            //var fetchXml = $@"<fetch>
            //                    <entity name='post'>
            //                          <all-attributes/>                                               
            //                    </entity>
            //                </fetch>";
            //var data = xrmFakedContext.GetFakedOrganizationService().RetrieveMultipleAll(fetchXml)?.Entities?.FirstOrDefault();
            //Assert.IsTrue(data != null);
            Assert.IsTrue(xrmFakedContext.Data["post"] != null);
            #endregion ASSERT  
        }

        private static Entity GetConfiguration()
        {
            return new cmc_configuration
            {
                Id = Guid.NewGuid(),
                cmc_postdomassignment = true
                
            };
        }
        private static Entity Getcmc_dommaster(Entity systemUser, EntityReference list)
        {
            return new cmc_dommaster
            {
                Id = Guid.NewGuid(),
                cmc_dommastername = "test",
                cmc_runassignmentforentity = new OptionSetValue((int)cmc_runassignmentforentity.Lifecycle),
                statecode = cmc_dommasterState.Active,
                cmc_marketinglistid = list,

            };
        }
        private static Entity Getcmc_dommaster2(Entity systemUser, EntityReference list,Entity systemUser2)
        {
            return new cmc_dommaster
            {
                Id = Guid.NewGuid(),
                cmc_dommastername = "test",
                cmc_runassignmentforentity = new OptionSetValue((int)cmc_runassignmentforentity.Lifecycle),
                statecode = cmc_dommasterState.Active,
                cmc_marketinglistid = list,
                cmc_fallbackuserid = systemUser2.ToEntityReference()

            };
        }
        private static Entity Getdomdefinitionexecutionorder(Entity systemUser, Guid domMasterId)
        {
            return new cmc_domdefinitionexecutionorder
            {
                Id = Guid.NewGuid(),
                statecode = cmc_domdefinitionexecutionorderState.Active,
                OwnerId = systemUser.ToEntityReference(),
                cmc_dommasterid = new EntityReference("cmc_dommaster", domMasterId),
                cmc_attributeschema = "opportunity.cmc_contact_opportunity_contactid.firstname"
            };
        }
        private static Entity GetList(Entity systemUser)
        {
            var FetchXML = $@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='true'>
                            <entity name='contact'><order attribute='fullname' descending='false' /></filter>
                                  <filter type='and'><condition attribute='fullname' operator='like' value='%Cameron%' />
                                     <link-entity name='opportunity' from='cmc_contactid' to='contactid' alias='aa'>
                                          <filter type='and'><condition attribute='opportunityid' operator='not-null' /></filter>
                                     </link-entity>
                            <attribute name='contactid' /><attribute name='fullname' /><attribute name='telephone1' />
                            </entity></fetch>";
            return new List
            {
                Id = Guid.NewGuid(),
                Type = true,
                OwnerId = systemUser.ToEntityReference(),
                Query = FetchXML,
                CreatedFromCode = list_createdfromcode.Contact
            };
        }

        private static Entity GetOpportunityEntity(Entity systemUser)
        {
            return new Opportunity
            {
                Id = Guid.NewGuid(),
                Name = "test",
                StateCode = OpportunityState.Open,
                cmc_domstatus = new OptionSetValue((int)cmc_domstatus.PendingAssignment),
                cmc_contactid = systemUser.ToEntityReference()
            };
        }

        private static Entity Getdomdefinition(EntityReference dommaster, Entity systemUser)
        {
            return new cmc_domdefinition
            {
                Id = Guid.NewGuid(),
                statecode = cmc_domdefinitionState.Active,
                cmc_dommasterid = dommaster,
                cmc_domdefinitionforid = systemUser.ToEntityReference()
            };
        }
        private static Entity GetdomdefinitionLogic(EntityReference domdefinition, Entity systemUser)
        {

            return new cmc_domdefinitionlogic
            {
                Id = Guid.NewGuid(),
                statecode = cmc_domdefinitionlogicState.Active,
                cmc_domdefinitionid = domdefinition,
                cmc_conditiontype = new OptionSetValue((int)cmc_domconditiontype.IsNotNull),
                cmc_attributeschema = "opportunity.cmc_contact_opportunity_contactid.firstname",
                OwnerId = systemUser.ToEntityReference(),
                cmc_value = "stateCode"
            };
        }

        private static Entity GetdomdefinitionLogic1(EntityReference domdefinition, Entity systemUser)
        {

            return new cmc_domdefinitionlogic
            {
                Id = Guid.NewGuid(),
                statecode = cmc_domdefinitionlogicState.Active,
                cmc_domdefinitionid = domdefinition,
                cmc_conditiontype = new OptionSetValue((int)cmc_domconditiontype.BeginsWith),
                cmc_attributeschema = "opportunity.cmc_contact_opportunity_contactid.firstname",
                OwnerId = systemUser.ToEntityReference(),
                cmc_value = "S"
            };
        }
        private static Entity GetdomdefinitionLogic2(EntityReference domdefinition, Entity systemUser)
        {

            return new cmc_domdefinitionlogic
            {
                Id = Guid.NewGuid(),
                statecode = cmc_domdefinitionlogicState.Active,
                cmc_domdefinitionid = domdefinition,
                cmc_conditiontype = new OptionSetValue((int)cmc_domconditiontype.Equals),
                cmc_attributeschema = "opportunity.cmc_contact_opportunity_contactid.firstname",
                OwnerId = systemUser.ToEntityReference(),
                cmc_value = "S"
            };
        }
        private static Entity GetdomdefinitionLogic3(EntityReference domdefinition, Entity systemUser)
        {

            return new cmc_domdefinitionlogic
            {
                Id = Guid.NewGuid(),
                statecode = cmc_domdefinitionlogicState.Active,
                cmc_domdefinitionid = domdefinition,
                cmc_conditiontype = new OptionSetValue((int)cmc_domconditiontype.RecordOwner),
                cmc_attributeschema = "opportunity.cmc_contact_opportunity_contactid.firstname",
                OwnerId = systemUser.ToEntityReference(),
                cmc_value = "S"
            };
        }
        private static Entity GetdomdefinitionLogic4(EntityReference domdefinition, Entity systemUser)
        {

            return new cmc_domdefinitionlogic
            {
                Id = Guid.NewGuid(),
                statecode = cmc_domdefinitionlogicState.Active,
                cmc_domdefinitionid = domdefinition,
                cmc_conditiontype = new OptionSetValue((int)cmc_domconditiontype.Range),
                cmc_attributeschema = "opportunity.cmc_contact_opportunity_contactid.firstname",
                OwnerId = systemUser.ToEntityReference(),
                cmc_minimum = DateTime.Now.ToLongDateString(),
                cmc_maximum= DateTime.Now.AddDays(5).ToLongDateString(),
                cmc_value = "0.5"
            };
        }
        private static Entity GetdomdefinitionLogic5(EntityReference domdefinition, Entity systemUser)
        {

            return new cmc_domdefinitionlogic
            {
                Id = Guid.NewGuid(),
                statecode = cmc_domdefinitionlogicState.Active,
                cmc_domdefinitionid = domdefinition,
                cmc_conditiontype = new OptionSetValue((int)cmc_domconditiontype.Equals),
                cmc_attributeschema = "opportunity.cmc_contact_opportunity_contactid.firstname",
                OwnerId = systemUser.ToEntityReference(),
                cmc_value = "systemuser"
            };
        }
    }
}
