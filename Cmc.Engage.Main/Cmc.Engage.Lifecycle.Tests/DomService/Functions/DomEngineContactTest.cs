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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;
using Moq;

namespace Cmc.Engage.Lifecycle.Tests.DomService.Functions
{
    [TestClass]
    public class DomEngineContactTest : XrmUnitTestBase
    {
        [TestCategory("Function"), TestCategory("Positive")]
        [TestMethod]
        public void DomEngineContact()
        {
            #region ARRANGE

            var bingMapKeyConfigInstance = GetConfiguration();
            var xrmFakedContext =
                new XrmFakedContext
                {
                    ProxyTypesAssembly = Assembly.Load("Cmc.Engage.Contracts.Tests")
                };
            var calledId = Guid.NewGuid();
            xrmFakedContext.CallerId = new EntityReference("SystemUser", calledId);
            var timezonedefinition = new Entity("timezonedefinition", Guid.NewGuid())
            {
                ["timezonecode"] = 190,
                ["standardname"] = "India Standard Time"
            };
            var systemUser = new Entity("SystemUser", xrmFakedContext.CallerId.Id)
            {
                ["systemuserid"] = xrmFakedContext.CallerId.Id
            };


            var list = GetList(systemUser);
            var domMaster = Getcmc_dommaster(list.ToEntityReference());
            var domdefinitionexecutionorder = Getdomdefinitionexecutionorder(systemUser, domMaster.Id);
            var contact = GetContactEntity();

            var domdefinition = Getdomdefinition(domMaster.ToEntityReference(), systemUser);
            var domdefinitionLogic = GetdomdefinitionLogic(domdefinition.ToEntityReference(), systemUser);


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
            xrmFakedContext.AddRelationship("listlead_association", new XrmFakedRelationship
            {
                IntersectEntity = "listmember",
                RelationshipType = XrmFakedRelationship.enmFakeRelationshipType.ManyToMany,
                Entity1LogicalName = List.EntityLogicalName,
                Entity1Attribute = "listid",
                Entity2LogicalName = Contact.EntityLogicalName,
                Entity2Attribute = "entityid"
            });
            xrmFakedContext.AddRelationship("listcontact_association", new XrmFakedRelationship
            {
                IntersectEntity = "listmember",
                RelationshipType = XrmFakedRelationship.enmFakeRelationshipType.ManyToMany,
                Entity1LogicalName = List.EntityLogicalName,
                Entity1Attribute = "listid",
                Entity2LogicalName = Contact.EntityLogicalName,
                Entity2Attribute = "entityid"
            });

            xrmFakedContext.AddRelationship("timezonedefinition", new XrmFakedRelationship
            {
                RelationshipType = XrmFakedRelationship.enmFakeRelationshipType.OneToMany,
                Entity1LogicalName = SystemUser.EntityLogicalName,
                Entity1Attribute = "timezonecode",
                Entity2LogicalName = "timezonedefinition",
                Entity2Attribute = "timezonecode"
            });

            xrmFakedContext.AddRelationship("contact_customer_accounts", new XrmFakedRelationship
            {
                RelationshipType = XrmFakedRelationship.enmFakeRelationshipType.OneToMany,
                Entity1LogicalName = "contact",
                Entity1Attribute = "parentcustomerid",
                Entity2LogicalName = "account",
                Entity2Attribute = "accountid"
            });


            xrmFakedContext.Initialize(new List<Entity>
            {
                assocationListMember,
                timezonedefinition,
                mockUserSettings,
                systemUser,
                bingMapKeyConfigInstance,
                domMaster,
                domdefinitionexecutionorder,
                list,
                contact,
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
                    {"LocalTime", DateTime.Now}
                }
            };


            var entityMetadata = new EntityMetadata {LogicalName = "contact"};
            entityMetadata.SetAttributeCollection(new List<AttributeMetadata>
            {
                new UniqueIdentifierAttributeMetadata("contactid") {LogicalName = "contactid"},
                new LookupAttributeMetadata
                {
                    LogicalName = "firstname",
                    SchemaName = "firstname",
                    DisplayName = new Label("systemuser Lookup", 1033),
                    Targets = new[] {"firstname"}
                }
            });


            xrmFakedContext.InitializeMetadata(entityMetadata);
            A.CallTo(() => xrmFakedContext.GetFakedOrganizationService().Execute(A<LocalTimeFromUtcTimeRequest>._))
                .Returns(localTimeFromUtcTimeResponse);


            var mockDomService = new DOMService(mockLogger.Object, mockILanguageService.Object,
                xrmFakedContext.GetFakedOrganizationService(), mockConfigurationService);

            mockDomService.ProcessDomAssignment("contact", xrmFakedContext.GetFakedOrganizationService());

            #endregion

            #region ASSERT

            Assert.IsTrue(xrmFakedContext.Data["post"] != null);

            #endregion ASSERT  
        }

        [TestCategory("Function"), TestCategory("Negative")]
        [TestMethod]
        public void DomEngineContactForNegativeScenario()
        {
            #region ARRANGE
            var xrmFakedContext = new XrmFakedContext();
            #endregion

            #region ACT
            var mockLogger = new Mock<ILogger>();
            var mockILanguageService = new Mock<ILanguageService>();
            var mockConfigurationService = new ConfigurationService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
            #endregion

            #region ASSERT
            Assert.ThrowsException<ArgumentNullException>(() => new DOMService(null, mockILanguageService.Object, xrmFakedContext.GetFakedOrganizationService(), mockConfigurationService));
            Assert.ThrowsException<ArgumentException>(() => new DOMService(mockLogger.Object, null, xrmFakedContext.GetFakedOrganizationService(), mockConfigurationService));          

            #endregion ASSERT  

        }

        #region DATA PREPARATION
        private static Entity GetConfiguration()
        {
            return new cmc_configuration
            {
                Id = Guid.NewGuid(),
                cmc_postdomassignment = true                
            };
        }

        private static Entity Getcmc_dommaster( EntityReference list)
        {
            return new cmc_dommaster
            {
                Id = Guid.NewGuid(),
                cmc_dommastername = "test",
                cmc_runassignmentforentity = new OptionSetValue((int) cmc_runassignmentforentity.Contact),
                statecode = cmc_dommasterState.Active,
                cmc_marketinglistid = list
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
                cmc_attributeschema = "account.contact_customer_accounts.firstname"
            };
        }

        private static Entity GetList(Entity systemUser)
        {
            return new List
            {
                Id = Guid.NewGuid(),
                Type = true,
                OwnerId = systemUser.ToEntityReference(),
                CreatedFromCode = list_createdfromcode.Account
            };
        }

        private static Entity GetContactEntity()
        {
            return new Contact
            {
                Id = Guid.NewGuid(),
                FirstName = "test",
                StateCode = ContactState.Active,
                cmc_domstatus = new OptionSetValue((int) cmc_domstatus.PendingAssignment)
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
                cmc_conditiontype = new OptionSetValue((int) cmc_domconditiontype.IsNotNull),
                cmc_attributeschema = "account.contact_customer_accounts.firstname",
                OwnerId = systemUser.ToEntityReference(),
                cmc_value = "stateCode"
            };
        }
        #endregion
    }
}