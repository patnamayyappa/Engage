using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Cmc.Core.Xrm.ServerExtension.Logging;
using Cmc.Engage.Common;
using Cmc.Engage.Common.Plugins.Tests.Utilities;
using Cmc.Engage.Models;
using FakeItEasy;
using FakeXrmEasy;
using FakeXrmEasy.Extensions;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Moq;
using Cmc.Engage.Common.Utilities.Constants;
using Microsoft.Xrm.Sdk.Query;
using Cmc.Engage.Lifecycle;

namespace Cmc.Engage.Lifecycle.Plugins.Test.DomService.Functions
{
    [TestClass]
    public class DomEngineAccountTest : XrmUnitTestBase
    {
        [TestMethod]
        [TestCategory("Function"), TestCategory("Positive")]
        public void DomEngineAccount()
        {
            #region ARRANGE
            var bingMapKeyConfigInstance = GetConfiguration();


            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.ProxyTypesAssembly = Assembly.Load("Cmc.Engage.Contracts.Tests");
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

            var shift = new Entity("cmc_shift", Guid.NewGuid())
            {
                ["name"] = "shift",
            };

            var account = GetAccouEntity();
            var account1 = GetAccouEntity();
            var account2 = GetAccouEntity();
            var account3 = GetAccouEntity();
            var account4 = GetAccouEntity();
            account1["name"] = "Hello";
            account2["name"] = "Hello2";
            account3["name"] = "Hello3";
            account4["name"] = "Hello4";

            var contact = new Entity("contact", Guid.NewGuid())
            {
                ["firstname"] = "Brian",
                ["lastname"] = "34Hello",
                ["emailaddress1"] = "Emailert",
                ["parentcustomerid"] = account.Id,
                ["cmc_domstatus"] = new OptionSetValue((int)cmc_domstatus.PendingAssignment),
                ["cmc_shiftid"] = shift.ToEntityReference()
            };

            var contact1 = new Entity("contact", Guid.NewGuid())
            {
                ["firstname"] = "Hello",
                ["lastname"] = "323Helloasdf",
                ["emailaddress1"] = "Email",
                ["parentcustomerid"] = account1.Id,
                ["cmc_deceased"] = false,
                ["cmc_sourcedate"] = DateTime.Now.ToShortDateString(),
                ["cmc_domstatus"] = new OptionSetValue((int)cmc_domstatus.PendingAssignment),
                ["cmc_shiftid"] = shift.ToEntityReference()
            };

            var contact2 = new Entity("contact", Guid.NewGuid())
            {
                ["firstname"] = "Hello2",
                ["lastname"] = "Hello34324",
                ["emailaddress1"] = "Email23",
                ["parentcustomerid"] = account2.Id,
                ["cmc_deceased"] = true,
                ["cmc_sourcedate"] = DateTime.Now.ToShortDateString(),
                ["cmc_domstatus"] = new OptionSetValue((int)cmc_domstatus.PendingAssignment),
                ["cmc_shiftid"] = shift.ToEntityReference()
            };
            var contact3 = new Entity("contact", Guid.NewGuid())
            {
                ["firstname"] = "Hello23",
                ["lastname"] = "Hello3432334",
                ["emailaddress1"] = "Email2333",
                ["parentcustomerid"] = account3.Id,
                ["cmc_deceased"] = true,
                ["cmc_sourcedate"] = DateTime.Now.ToShortDateString(),
                ["cmc_domstatus"] = new OptionSetValue((int)cmc_domstatus.PendingAssignment),
                ["cmc_shiftid"] = shift.ToEntityReference()
            };

            var contact4 = new Entity("contact", Guid.NewGuid())
            {
                ["firstname"] = "First",
                ["lastname"] = "Last",
                ["emailaddress1"] = "Email123",
                ["parentcustomerid"] = account4.Id,
                ["cmc_domstatus"] = new OptionSetValue((int)cmc_domstatus.PendingAssignment),
                ["cmc_shiftid"] = shift.ToEntityReference()
            };
            
            var list = GetList(systemUser);

            var domMaster = Getcmc_dommaster(systemUser, list.ToEntityReference());
            var domdefinitionexecutionorder = GetdomdefinitionexecutionorderList(systemUser, domMaster.Id);

            var domdefinition = Getdomdefinition(domMaster.ToEntityReference(), systemUser);
            //var domdefinitionLogic = GetdomdefinitionLogic(domdefinition.ToEntityReference(), systemUser);
            var domdefinitionLogic = GetdomdefinitionLogicList(domdefinition.ToEntityReference(), systemUser);

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


            //var assocationListMember = new Entity("listmember", Guid.NewGuid())
            //{
            //    ["entityid"] = account.Id,
            //    ["listid"] = list.Id
            //};
            //var assocationListMember1 = new Entity("listmember", Guid.NewGuid())
            //{
            //    ["entityid"] = account1.Id,
            //    ["listid"] = list.Id
            //};
            xrmFakedContext.AddRelationship("listlead_association", new XrmFakedRelationship
            {
                IntersectEntity = "listmember",
                RelationshipType = XrmFakedRelationship.enmFakeRelationshipType.ManyToMany,
                Entity1LogicalName = List.EntityLogicalName,
                Entity1Attribute = "listid",
                Entity2LogicalName = Contact.EntityLogicalName,
                Entity2Attribute = "entityid"
            });
            xrmFakedContext.AddRelationship("listaccount_association", new XrmFakedRelationship
            {
                IntersectEntity = "listmember",
                RelationshipType = XrmFakedRelationship.enmFakeRelationshipType.ManyToMany,
                Entity1LogicalName = List.EntityLogicalName,
                Entity1Attribute = "listid",
                Entity2LogicalName = Account.EntityLogicalName,
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

            var entityList = new List<Entity>();

            entityList.Add(contact);
            //entityList.Add(assocationListMember);
            //entityList.Add(assocationListMember1);
            entityList.Add(timezonedefinition);
            entityList.Add(mockUserSettings);
            entityList.Add(systemUser);
            entityList.Add(bingMapKeyConfigInstance);
            entityList.Add(domMaster);
            entityList.AddRange(domdefinitionexecutionorder);
            entityList.Add(list);
            entityList.Add(account);
            entityList.Add(account1);
            entityList.Add(account2);
            entityList.Add(account3);
            entityList.Add(account4);
            entityList.Add(domdefinition);
            entityList.AddRange(domdefinitionLogic);


            xrmFakedContext.Initialize(entityList);
            

            var listMembersListRequest = new AddListMembersListRequest() { ListId = list.Id, MemberIds = new List<Guid>() { account.Id, account1.Id, account2.Id, account3.Id, account4.Id }.ToArray() };
            xrmFakedContext.GetFakedOrganizationService().Execute(listMembersListRequest);


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

            var entityMetaDataList = new EntityMetadataCollection();
           
            var contactMetadata = new EntityMetadata() { LogicalName = "contact" };
            contactMetadata.SetAttributeCollection(new List<AttributeMetadata>()
            {
                new UniqueIdentifierAttributeMetadata("contactid") {LogicalName =   "contactid",},
                new StringAttributeMetadata(){
                    LogicalName =   "firstname",
                    SchemaName = "firstname",
                    DisplayName = new Label("firstname",1033),
                },
                new StringAttributeMetadata(){
                    LogicalName = "lastname",
                    SchemaName = "lastname",
                    DisplayName = new Label("lastname",1033),
                },
                new DateTimeAttributeMetadata(){
                    LogicalName = "cmc_sourcedate",
                    SchemaName = "cmc_sourcedate",
                    DisplayName = new Label("cmc_sourcedate",1033),
                },
                new BooleanAttributeMetadata()
                {
                    LogicalName = "cmc_deceased",
                    SchemaName = "cmc_deceased",
                    DisplayName = new Label("cmc_deceased",1033),
                },
                new LookupAttributeMetadata(){
                    LogicalName = "cmc_shiftid",
                    SchemaName = "cmc_shiftid",
                    DisplayName = new Label("cmc_shiftid",1033),
                    Targets = new[] { "cmc_shift" },
                },
                new PicklistAttributeMetadata()
                {
                    LogicalName = "cmc_domstatus",
                    SchemaName = "cmc_domstatus",
                    DisplayName = new Label("cmc_domstatus",1033),
                    OptionSet = new OptionSetMetadata()
                    {
                    Options = { new OptionMetadata(new Label()
                    {
                        UserLocalizedLabel = new LocalizedLabel()
                        {
                            Label = new Label("cmc_domstatus", 175490000).ToString() } }, 175490000)                    
                        }
                    }
                }
            });

            var primaryNameAttribute = contactMetadata.GetType().GetProperty("PrimaryNameAttribute");
            if (primaryNameAttribute != null) primaryNameAttribute.SetValue(contactMetadata, "firstname");

            entityMetaDataList.Add(contactMetadata);

            var shiftMetadata = new EntityMetadata() { LogicalName = "cmc_shift" };
            shiftMetadata.SetAttributeCollection(new List<AttributeMetadata>()
            {
                new UniqueIdentifierAttributeMetadata("cmc_shiftid") {LogicalName =   "cmc_shiftid"},  
            });
           

            var primaryNameAttributeShift = shiftMetadata.GetType().GetProperty("PrimaryNameAttribute");
            if (primaryNameAttributeShift != null) primaryNameAttributeShift.SetValue(shiftMetadata, "cmc_code");

            entityMetaDataList.Add(shiftMetadata);

            xrmFakedContext.InitializeMetadata(entityMetaDataList);
            A.CallTo(() => xrmFakedContext.GetFakedOrganizationService().Execute(A<LocalTimeFromUtcTimeRequest>._)).Returns(localTimeFromUtcTimeResponse);

            var mockDomService = new DOMService(mockLogger.Object, mockILanguageService.Object, xrmFakedContext.GetFakedOrganizationService(), mockConfigurationService);
            xrmFakedContext.GetFakedOrganizationService().Create(new cmc_todo() { cmc_todoId = Guid.NewGuid() });
            mockDomService.ProcessDomAssignment("account", xrmFakedContext.GetFakedOrganizationService());

            #endregion

            #region ASSERT

            Assert.IsTrue(xrmFakedContext.Data["post"] != null);
            #endregion ASSERT  
        }

        [TestMethod]
        [TestCategory("Function"), TestCategory("Negative")]
        public void DomEngineAccountNegative()
        {
            #region ARRANGE
            var bingMapKeyConfigInstance = GetConfiguration();


            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.ProxyTypesAssembly = Assembly.Load("Cmc.Engage.Contracts.Tests");
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
            var account = GetAccouEntity();

            var domdefinition = Getdomdefinition(domMaster.ToEntityReference(), systemUser);
            var domdefinitionLogic = GetdomdefinitionLogic(domdefinition.ToEntityReference(), systemUser);
            var contact = new Entity("contact", Guid.NewGuid())
            {
                ["firstname"] = "Brian",
                ["parentcustomerid"] = account.Id

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
                ["entityid"] = account.Id,
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
            xrmFakedContext.AddRelationship("listaccount_association", new XrmFakedRelationship
            {
                IntersectEntity = "listmember",
                RelationshipType = XrmFakedRelationship.enmFakeRelationshipType.ManyToMany,
                Entity1LogicalName = List.EntityLogicalName,
                Entity1Attribute = "listid",
                Entity2LogicalName = Account.EntityLogicalName,
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
                contact,
                assocationListMember,

                timezonedefinition,
                mockUserSettings,
                systemUser,
                bingMapKeyConfigInstance,
                domMaster,
                domdefinitionexecutionorder,
                list,
                account,
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


            xrmFakedContext.InitializeMetadata(entityMetadata);
            A.CallTo(() => xrmFakedContext.GetFakedOrganizationService().Execute(A<LocalTimeFromUtcTimeRequest>._)).Returns(localTimeFromUtcTimeResponse);

            var mockDomService = new DOMService(mockLogger.Object, mockILanguageService.Object, xrmFakedContext.GetFakedOrganizationService(), mockConfigurationService);
            xrmFakedContext.GetFakedOrganizationService().Create(new cmc_todo() { cmc_todoId = Guid.NewGuid() });
            mockDomService.ProcessDomAssignment("account1", xrmFakedContext.GetFakedOrganizationService());

            #endregion

            #region ASSERT

            //Assert.IsTrue(xrmFakedContext.Data["post"] != null);
            #endregion ASSERT  
        }


        [TestMethod]
        [TestCategory("Function"), TestCategory("Negative")]
        public void DomEngineAccountNegative2()
        {
            #region ARRANGE
            //var bingMapKeyConfigInstance = GetConfiguration();


            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.ProxyTypesAssembly = Assembly.Load("Cmc.Engage.Contracts.Tests");
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
            var account = GetAccouEntity();

            var domdefinition = Getdomdefinition(domMaster.ToEntityReference(), systemUser);
            var domdefinitionLogic = GetdomdefinitionLogic(domdefinition.ToEntityReference(), systemUser);
            var contact = new Entity("contact", Guid.NewGuid())
            {
                ["firstname"] = "Brian",
                ["parentcustomerid"] = account.Id

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
                ["entityid"] = account.Id,
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
            xrmFakedContext.AddRelationship("listaccount_association", new XrmFakedRelationship
            {
                IntersectEntity = "listmember",
                RelationshipType = XrmFakedRelationship.enmFakeRelationshipType.ManyToMany,
                Entity1LogicalName = List.EntityLogicalName,
                Entity1Attribute = "listid",
                Entity2LogicalName = Account.EntityLogicalName,
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
                contact,
                assocationListMember,
                timezonedefinition,
                mockUserSettings,
                systemUser,
                //bingMapKeyConfigInstance,
                domMaster,
                domdefinitionexecutionorder,
                list,
                account,
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


            xrmFakedContext.InitializeMetadata(entityMetadata);
            A.CallTo(() => xrmFakedContext.GetFakedOrganizationService().Execute(A<LocalTimeFromUtcTimeRequest>._)).Returns(localTimeFromUtcTimeResponse);

            var mockDomService = new DOMService(mockLogger.Object, mockILanguageService.Object, xrmFakedContext.GetFakedOrganizationService(), mockConfigurationService);
            xrmFakedContext.GetFakedOrganizationService().Create(new cmc_todo() { cmc_todoId = Guid.NewGuid() });
            try
            {
                mockDomService.ProcessDomAssignment("account", xrmFakedContext.GetFakedOrganizationService());
            }
            catch (Exception ex)
            {

            }

            #endregion

            #region ASSERT

            //Assert.IsTrue(xrmFakedContext.Data["post"] != null);
            #endregion ASSERT  
        }

        [TestMethod]
        [TestCategory("Function"), TestCategory("Posetive")]
        public void DomEngineAccountNegative4()
        {
            #region ARRANGE
            //var bingMapKeyConfigInstance = GetConfiguration();


            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.ProxyTypesAssembly = Assembly.Load("Cmc.Engage.Contracts.Tests");
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
            var account = GetAccouEntity();

            var domdefinition = Getdomdefinition(domMaster.ToEntityReference(), systemUser);
            var domdefinitionLogic = GetdomdefinitionLogic(domdefinition.ToEntityReference(), systemUser);
            var contact = new Entity("contact", Guid.NewGuid())
            {
                ["firstname"] = "Brian",
                ["parentcustomerid"] = account.Id

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
                ["entityid"] = account.Id,
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
            xrmFakedContext.AddRelationship("listaccount_association", new XrmFakedRelationship
            {
                IntersectEntity = "listmember",
                RelationshipType = XrmFakedRelationship.enmFakeRelationshipType.ManyToMany,
                Entity1LogicalName = List.EntityLogicalName,
                Entity1Attribute = "listid",
                Entity2LogicalName = Account.EntityLogicalName,
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
                contact,
                assocationListMember,
                timezonedefinition,
                mockUserSettings,
                systemUser,
                //bingMapKeyConfigInstance,
                domMaster,
                domdefinitionexecutionorder,
                list,
                account,
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


            xrmFakedContext.InitializeMetadata(entityMetadata);
            A.CallTo(() => xrmFakedContext.GetFakedOrganizationService().Execute(A<LocalTimeFromUtcTimeRequest>._)).Returns(localTimeFromUtcTimeResponse);

            var mockDomService = new DOMService(mockLogger.Object, mockILanguageService.Object, xrmFakedContext.GetFakedOrganizationService(), mockConfigurationService);
            xrmFakedContext.GetFakedOrganizationService().Create(new cmc_todo() { cmc_todoId = Guid.NewGuid() });
            try
            {
                mockDomService.ProcessDomAssignment("account", xrmFakedContext.GetFakedOrganizationService());
            }
            catch (Exception ex)
            {

            }

            #endregion

            #region ASSERT

            //Assert.IsTrue(xrmFakedContext.Data["post"] != null);
            #endregion ASSERT  
        }


        private static cmc_configuration GetConfiguration()
        {
            return new cmc_configuration
            {
                Id = Guid.NewGuid(),
                cmc_postdomassignment = true
                //cmc_configurationname = Constants.PostDomAssignmentConfigName,
                //cmc_Value = "True"
            };
        }
        private static Entity Getcmc_dommaster(Entity systemUser, EntityReference list)
        {
            return new cmc_dommaster
            {
                Id = Guid.NewGuid(),
                cmc_dommastername = "test",
                cmc_runassignmentforentity = new OptionSetValue((int)cmc_runassignmentforentity.Account),
                statecode = cmc_dommasterState.Active,
                cmc_marketinglistid = list,
                cmc_fallbackuserid = systemUser.ToEntityReference()
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
                cmc_attributeschema = "account.contact_customer_accounts.firstname",
                cmc_order = 1,

            };
        }
        private static List<Entity> GetdomdefinitionexecutionorderList(Entity systemUser, Guid domMasterId)
        {
            var lstOrder = new List<Entity>(){new cmc_domdefinitionexecutionorder
            {
                Id = Guid.NewGuid(),
                statecode = cmc_domdefinitionexecutionorderState.Active,
                OwnerId = systemUser.ToEntityReference(),
                cmc_dommasterid = new EntityReference("cmc_dommaster", domMasterId),
                cmc_attributeschema = "account.contact_customer_accounts.firstname",
                cmc_order = 1,

            },
            new cmc_domdefinitionexecutionorder
            {
                Id = Guid.NewGuid(),
                statecode = cmc_domdefinitionexecutionorderState.Active,
                OwnerId = systemUser.ToEntityReference(),
                cmc_dommasterid = new EntityReference("cmc_dommaster", domMasterId),
                cmc_attributeschema = "account.contact_customer_accounts.cmc_deceased",
                cmc_order = 2,

            },
            new cmc_domdefinitionexecutionorder
            {
                Id = Guid.NewGuid(),
                statecode = cmc_domdefinitionexecutionorderState.Active,
                OwnerId = systemUser.ToEntityReference(),
                cmc_dommasterid = new EntityReference("cmc_dommaster", domMasterId),
                cmc_attributeschema = "account.contact_customer_accounts.cmc_sourcedate",
                cmc_order = 3,

            },
            new cmc_domdefinitionexecutionorder
            {
                Id = Guid.NewGuid(),
                statecode = cmc_domdefinitionexecutionorderState.Active,
                OwnerId = systemUser.ToEntityReference(),
                cmc_dommasterid = new EntityReference("cmc_dommaster", domMasterId),
                cmc_attributeschema = "account.contact_customer_accounts.cmc_shiftid",
                cmc_order = 4,

            },
            new cmc_domdefinitionexecutionorder
            {
                Id = Guid.NewGuid(),
                statecode = cmc_domdefinitionexecutionorderState.Active,
                OwnerId = systemUser.ToEntityReference(),
                cmc_dommasterid = new EntityReference("cmc_dommaster", domMasterId),
                cmc_attributeschema = "account.contact_customer_accounts.cmc_domstatus",
                cmc_order = 5,

            },
            };

            return lstOrder;
        }
        private static Entity GetList(Entity systemUser)
        {
            return new List
            {
                Id = Guid.NewGuid(),
                Type = true,
                OwnerId = systemUser.ToEntityReference(),
                CreatedFromCode = list_createdfromcode.Account,

            };
        }

        private static Entity GetAccouEntity()
        {
            return new Account
            {
                Id = Guid.NewGuid(),
                Name = "test",
                StateCode = AccountState.Active,
                cmc_domstatus = new OptionSetValue((int)cmc_domstatus.PendingAssignment),

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
                cmc_attributeschema = "account.contact_customer_accounts.firstname",
                OwnerId = systemUser.ToEntityReference(),
                cmc_value = "stateCode"
            };
        }

        private static List<cmc_domdefinitionlogic> GetdomdefinitionLogicList(EntityReference domdefinition, Entity systemUser)
        {

            List<cmc_domdefinitionlogic> listDomdefinitionlogic = new List<cmc_domdefinitionlogic>(){
            new cmc_domdefinitionlogic
            {
                Id = Guid.NewGuid(),
                statecode = cmc_domdefinitionlogicState.Active,
                cmc_domdefinitionid = domdefinition,
                cmc_conditiontype = new OptionSetValue((int)cmc_domconditiontype.IsNotNull),
                cmc_attributeschema = "account.contact_customer_accounts.firstname",
                OwnerId = systemUser.ToEntityReference(),
                cmc_value = "stateCode"
            },
            new cmc_domdefinitionlogic
            {
                Id = Guid.NewGuid(),
                statecode = cmc_domdefinitionlogicState.Active,
                cmc_domdefinitionid = domdefinition,
                cmc_conditiontype = new OptionSetValue((int)cmc_domconditiontype.BeginsWith),
                cmc_attributeschema = "account.contact_customer_accounts.cmc_deceased",
                OwnerId = systemUser.ToEntityReference(),
                cmc_value = "true"
            },
            new cmc_domdefinitionlogic
            {
                Id = Guid.NewGuid(),
                statecode = cmc_domdefinitionlogicState.Active,
                cmc_domdefinitionid = domdefinition,
                cmc_conditiontype = new OptionSetValue((int)cmc_domconditiontype.Equals),
                cmc_attributeschema = "account.contact_customer_accounts.cmc_sourcedate",
                OwnerId = systemUser.ToEntityReference(),
                cmc_value = DateTime.Now.ToShortDateString()
            },
            new cmc_domdefinitionlogic
            {
                Id = Guid.NewGuid(),
                statecode = cmc_domdefinitionlogicState.Active,
                cmc_domdefinitionid = domdefinition,
                cmc_conditiontype = new OptionSetValue((int)cmc_domconditiontype.BeginsWith),
                cmc_attributeschema = "account.contact_customer_accounts.cmc_shiftid",
                OwnerId = systemUser.ToEntityReference(),
                cmc_value = ""
            },
            new cmc_domdefinitionlogic
            {
                Id = Guid.NewGuid(),
                statecode = cmc_domdefinitionlogicState.Active,
                cmc_domdefinitionid = domdefinition,
                cmc_conditiontype = new OptionSetValue((int)cmc_domconditiontype.BeginsWith),
                cmc_attributeschema = "account.contact_customer_accounts.cmc_domstatus",
                OwnerId = systemUser.ToEntityReference(),
                cmc_value = "Microsoft.Xrm.Sdk.Label"
            },
            };

            return listDomdefinitionlogic;

        }
    }
}
