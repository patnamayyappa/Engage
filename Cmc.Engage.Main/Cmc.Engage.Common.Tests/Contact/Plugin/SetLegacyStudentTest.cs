using System;
using System.Collections.Generic;
using Cmc.Core.Xrm.ServerExtension.Logging;
using Cmc.Engage.Common.Plugins.Tests.Utilities;
using Cmc.Engage.Models;
using FakeXrmEasy;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Moq;

namespace Cmc.Engage.Common.Tests.Contact.Plugin
{
    [TestClass]
    public class SetLegacyStudentTest : XrmUnitTestBase
    {
        [TestCategory("Plugin"), TestCategory("Positive")]
        [TestMethod]
        public void SetLegacyStudent_CreateConnection()
        {
            #region ARRANGE

            var accountInstance1 = PrepareAccountInstance();
            cmc_configuration retrieveConfiguration = GetActiveConfiguration();
            var contactInstance1 = PreparingContactInstance(accountInstance1.Id);
            var contactInstance2 = PreparingContactInstance2(accountInstance1.Id);
            var childRoleInstance = PrepareChildRole();
            var parentRoleInstance = PrepareParentRole();
            var connectionInstance = PrepareConnectionInstance(contactInstance1, contactInstance2, parentRoleInstance, childRoleInstance, accountInstance1);
            var alumniInstance = PrepareAlumniInstanceforContact(contactInstance2, accountInstance1);
            var configuration = retrieveConfiguration;//PrepareConfiguration();
            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>()
            {
                accountInstance1,
                connectionInstance,
                contactInstance1,
                contactInstance2,
                parentRoleInstance,
                childRoleInstance,
                alumniInstance,
                configuration
            });
            //Initialize the Mock Service
            var mockServiceProvider = InitializeMockService(xrmFakedContext, connectionInstance, Operation.Create);

            //Fetch the Mock Execution Context
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            #endregion

            #region ACT
            var connectionEntityCollection = new EntityCollection();
            connectionEntityCollection.Entities.Add(contactInstance1);
            connectionEntityCollection.Entities.Add(contactInstance2);

            var mockLogger = new Mock<ILogger>();
            var mockILanguageService = new Mock<ILanguageService>();
            var mockBingMapService = InitializeBingMapMockService();

            var mockConfigurationService = new ConfigurationService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
            //Mock the IBingMap
            //var mockBingMapService.Object = new BingMapService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService(), mockConfigurationService);
            var contactService = new ContactService(mockLogger.Object, mockBingMapService.Object, xrmFakedContext.GetFakedOrganizationService(), mockConfigurationService, mockILanguageService.Object);

            contactService.SetLegacyStudent(mockExecutionContext.Object);
            #endregion

            #region ASSERT

          

            var cmc_legacy = xrmFakedContext.GetFakedOrganizationService()
                            .Retrieve("contact", contactInstance1.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet(true))
                            .GetAttributeValue<bool>("mshied_legacy");

            Assert.IsTrue(cmc_legacy);

            #endregion
        }
        
        [TestCategory("Plugin"), TestCategory("Positive")]
        [TestMethod]
        public void SetLegacyStudent_UpdateConnection_Family()
        {
            #region ARRANGE

            var accountInstance1 = PrepareAccountInstance();
            cmc_configuration retrieveConfiguration = GetActiveConfiguration();
            var contactInstance1 = PreparingContactInstance(accountInstance1.Id, true);
            var contactInstance2 = PreparingContactInstance2(accountInstance1.Id, true);
            var childRoleInstance = PrepareChildRole();
            var parentRoleInstance = PrepareParentRole();
            var connectionInstance = PrepareConnectionInstance(contactInstance1, contactInstance2, parentRoleInstance, childRoleInstance, accountInstance1);
            connectionInstance.StateCode = ConnectionState.Inactive;
            var preconnectionInstance = PrepareConnectionInstance(contactInstance1, contactInstance2, parentRoleInstance, childRoleInstance, accountInstance1);
            var alumniInstance = PrepareAlumniInstanceforContact(contactInstance2, accountInstance1);
            var configuration = retrieveConfiguration;//PrepareConfiguration();

            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>()
            {
                accountInstance1,
                connectionInstance,
                preconnectionInstance,
                contactInstance1,
                contactInstance2,
                parentRoleInstance,
                childRoleInstance,
                alumniInstance,
                configuration
            });

            //Initialize the Mock Service
            var mockServiceProvider = InitializeMockService(xrmFakedContext, connectionInstance, Operation.Update);
            AddPreEntityImage(mockServiceProvider, "Target", preconnectionInstance);
            AddPostEntityImage(mockServiceProvider, "Target", connectionInstance);

            //Fetch the Mock Execution Context
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            #endregion

            #region ACT
            var connectionEntityCollection = new EntityCollection();
            connectionEntityCollection.Entities.Add(contactInstance1);
            connectionEntityCollection.Entities.Add(contactInstance2);

            var mockLogger = new Mock<ILogger>();
            var mockILanguageService = new Mock<ILanguageService>();
            var mockConfigurationService = new ConfigurationService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
            //Mock the IBingMap
            var mockBingMapService = InitializeBingMapMockService();

            //var mockBingServices = new BingMapService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService(), mockConfigurationService);
            var contactService = new ContactService(mockLogger.Object, mockBingMapService.Object, xrmFakedContext.GetFakedOrganizationService(), mockConfigurationService, mockILanguageService.Object);

            contactService.SetLegacyStudent(mockExecutionContext.Object);
            #endregion

            #region ASSERT

            var cmc_legacy = xrmFakedContext.GetFakedOrganizationService()
                            .Retrieve("contact", contactInstance1.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet(true))
                            .GetAttributeValue<bool>("cmc_legacy");

            Assert.IsFalse(cmc_legacy);

            #endregion
        }
        
        [TestCategory("Plugin"), TestCategory("Positive")]
        [TestMethod]
        public void SetLegacyStudent_DeleteConnection()
        {
            #region ARRANGE

            var accountInstance1 = PrepareAccountInstance();
            cmc_configuration retrieveConfiguration = GetActiveConfiguration();
            var contactInstance1 = PreparingContactInstance(accountInstance1.Id, true);
            var contactInstance2 = PreparingContactInstance2(accountInstance1.Id, true);
            var childRoleInstance = PrepareChildRole();
            var parentRoleInstance = PrepareParentRole();
            var connectionInstance = PrepareConnectionInstance(contactInstance1, contactInstance2, parentRoleInstance, childRoleInstance, accountInstance1);
            connectionInstance.StateCode = ConnectionState.Inactive;
            var preconnectionInstance = PrepareConnectionInstance(contactInstance1, contactInstance2, parentRoleInstance, childRoleInstance, accountInstance1);
            var alumniInstance = PrepareAlumniInstanceforContact(contactInstance2, accountInstance1);
            var configuration = retrieveConfiguration;//PrepareConfiguration();

            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>()
            {
                accountInstance1,
                connectionInstance,
                preconnectionInstance,
                contactInstance1,
                contactInstance2,
                parentRoleInstance,
                childRoleInstance,
                alumniInstance,
                configuration
            });
            //Initialize the Mock Service
            var mockServiceProvider = InitializeMockService(xrmFakedContext, connectionInstance, Operation.Delete);
            AddPreEntityImage(mockServiceProvider, "Target", preconnectionInstance);
            //Fetch the Mock Execution Context
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            #endregion

            #region ACT
            var connectionEntityCollection = new EntityCollection();
            connectionEntityCollection.Entities.Add(contactInstance1);
            connectionEntityCollection.Entities.Add(contactInstance2);

            var mockLogger = new Mock<ILogger>();
            var mockILanguageService = new Mock<ILanguageService>();
            var mockConfigurationService = new ConfigurationService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
            //Mock the IBingMap
            var mockBingMapService = InitializeBingMapMockService();

            //var mockBingServices = new BingMapService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService(), mockConfigurationService);
            var contactService = new ContactService(mockLogger.Object, mockBingMapService.Object, xrmFakedContext.GetFakedOrganizationService(), mockConfigurationService, mockILanguageService.Object);

            contactService.SetLegacyStudent(mockExecutionContext.Object);
            #endregion

            #region ASSERT

            var cmc_legacy = xrmFakedContext.GetFakedOrganizationService()
                            .Retrieve("contact", contactInstance1.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet(true))
                            .GetAttributeValue<bool>("cmc_legacy");

            Assert.IsFalse(cmc_legacy);

            #endregion
        }
                
        [TestMethod]
        [TestCategory("Plugin"), TestCategory("Positive")]
        public void SetLegacyStudent_UpdateConnection_AccountToContact()
        {
            #region ARRANGE

            var accountInstance1 = PrepareAccountInstance();
            cmc_configuration retrieveConfiguration = GetActiveConfiguration();
            var contactInstance1 = PreparingContactInstance(accountInstance1.Id, true);
            var contactInstance2 = PreparingContactInstance2(accountInstance1.Id, true);
            var connectionRole1 = PrepareChildRole();
            var connectionRole2 = PrepareParentRole();
            var postConnectionInstance = PrepareConnectionInstance(contactInstance1, contactInstance2, connectionRole1, connectionRole2, accountInstance1);
            postConnectionInstance.StateCode = ConnectionState.Active;
            var preConnectionInstance = PrepareConnectionInstanceAtoC(contactInstance1, accountInstance1, connectionRole1, connectionRole2);
            var configuration = retrieveConfiguration;//PrepareConfiguration();


            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>()
            {

                postConnectionInstance,
                preConnectionInstance,
                accountInstance1,
                contactInstance1,
                contactInstance2,
                configuration,
                connectionRole1,
                connectionRole2
            });


            //Initialize the Mock Service
            var mockServiceProvider = InitializeMockService(xrmFakedContext, preConnectionInstance, Operation.Update);
            AddPreEntityImage(mockServiceProvider, "Target", preConnectionInstance);
            AddPostEntityImage(mockServiceProvider, "Target", postConnectionInstance);
            //Fetch the Mock Execution Context
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            #endregion

            #region ACT
            var connectionEntityCollection = new EntityCollection();
            connectionEntityCollection.Entities.Add(contactInstance1);
            connectionEntityCollection.Entities.Add(contactInstance2);

            var mockLogger = new Mock<ILogger>();
            var mockILanguageService = new Mock<ILanguageService>();
            var mockConfigurationService = new ConfigurationService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
            //Mock the IBingMap
            var mockBingMapService = InitializeBingMapMockService();

            //var mockBingServices = new BingMapService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService(), mockConfigurationService);
            var contactService = new ContactService(mockLogger.Object, mockBingMapService.Object, xrmFakedContext.GetFakedOrganizationService(), mockConfigurationService,mockILanguageService.Object);

            contactService.SetLegacyStudent(mockExecutionContext.Object);
            #endregion

            #region ASSERT

            var cmc_legacy = xrmFakedContext.GetFakedOrganizationService()
                            .Retrieve("contact", contactInstance1.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet(true))
                            .GetAttributeValue<bool>("mshied_legacy");

            Assert.IsTrue(cmc_legacy);

            #endregion
        }
        
        [TestMethod]
        [TestCategory("Plugin"), TestCategory("Positive")]
        public void SetLegacyStudent_UpdateConnection_ContactToAccountNonAlumni()
        {
            #region ARRANGE

            var accountInstance1 = PrepareAccountInstance();
            cmc_configuration retrieveConfiguration = GetActiveConfiguration();
            var contactInstance1 = PreparingContactInstance(accountInstance1.Id, true);
            var contactInstance2 = PreparingContactInstance2(accountInstance1.Id, true);
            var connectionRole1 = PrepareChildRole();
            var connectionRole2 = PrepareParentRole();

            var preConnectionInstance = PrepareConnectionInstanceCtoA(contactInstance1, contactInstance2, connectionRole1, connectionRole2);
            var postConnectionInstance = PrepareConnectionInstanceCtoC(contactInstance1, contactInstance2, connectionRole1, connectionRole2, accountInstance1);
            postConnectionInstance.StateCode = ConnectionState.Active;
            preConnectionInstance.Record1RoleId = (new ConnectionRole() { Id = new Guid() }).ToEntityReference();
            preConnectionInstance.Record2RoleId = (new ConnectionRole() { Id = new Guid() }).ToEntityReference();
            var configuration = retrieveConfiguration;//PrepareConfiguration();


            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>()
            {

                postConnectionInstance,
                preConnectionInstance,
                accountInstance1,
                contactInstance1,
                contactInstance2,
                configuration,
                connectionRole1,
                connectionRole2
            });


            //Initialize the Mock Service
            var mockServiceProvider = InitializeMockService(xrmFakedContext, preConnectionInstance, Operation.Update);
            AddPreEntityImage(mockServiceProvider, "Target", preConnectionInstance);
            AddPostEntityImage(mockServiceProvider, "Target", postConnectionInstance);
            //Fetch the Mock Execution Context
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            #endregion

            #region ACT
            var connectionEntityCollection = new EntityCollection();
            connectionEntityCollection.Entities.Add(contactInstance1);
            connectionEntityCollection.Entities.Add(contactInstance2);

            var mockLogger = new Mock<ILogger>();
            var mockILanguageService = new Mock<ILanguageService>();
            var mockConfigurationService = new ConfigurationService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
            //Mock the IBingMap
            var mockBingMapService = InitializeBingMapMockService();

            //var mockBingServices = new BingMapService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService(), mockConfigurationService);
            var contactService = new ContactService(mockLogger.Object, mockBingMapService.Object, xrmFakedContext.GetFakedOrganizationService(), mockConfigurationService, mockILanguageService.Object);

            contactService.SetLegacyStudent(mockExecutionContext.Object);
            #endregion

            #region ASSERT

            var cmc_legacy = xrmFakedContext.GetFakedOrganizationService()
                            .Retrieve("contact", contactInstance1.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet(true))
                            .GetAttributeValue<bool>("mshied_legacy");

            Assert.IsTrue(cmc_legacy);

            #endregion
        }
        
        [TestMethod]
        [TestCategory("Plugin"), TestCategory("Positive")]
        public void SetLegacyStudent_UpdateConnection_ContactToAccountAlumni()
        {
            #region ARRANGE

            var accountInstance1 = PrepareAccountInstance();
            cmc_configuration retrieveConfiguration = GetActiveConfiguration();
            var contactInstance1 = PreparingContactInstance(accountInstance1.Id, true);
            var contactInstance2 = PreparingContactInstance2(accountInstance1.Id, true);
            var connectionRole1 = PrepareChildRole();
            var connectionRole2 = PrepareParentRole();
            var preConnectionInstance = PrepareConnectionInstanceCtoA(contactInstance1, contactInstance2, connectionRole1, connectionRole2);
            var postConnectionInstance = PrepareConnectionInstanceCtoC(contactInstance1, contactInstance2, connectionRole1, connectionRole2, accountInstance1);
            postConnectionInstance.StateCode = ConnectionState.Active;
            postConnectionInstance.Record1RoleId = (new ConnectionRole() { Id = new Guid() }).ToEntityReference();
            postConnectionInstance.Record2RoleId = (new ConnectionRole() { Id = new Guid() }).ToEntityReference();
            var configuration = retrieveConfiguration;//PrepareConfiguration();


            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>()
            {

                postConnectionInstance,
                preConnectionInstance,
                accountInstance1,
                contactInstance1,
                contactInstance2,
                configuration,
                connectionRole1,
                connectionRole2
            });


            //Initialize the Mock Service
            var mockServiceProvider = InitializeMockService(xrmFakedContext, preConnectionInstance, Operation.Update);
            AddPreEntityImage(mockServiceProvider, "Target", preConnectionInstance);
            AddPostEntityImage(mockServiceProvider, "Target", postConnectionInstance);
            //Fetch the Mock Execution Context
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            #endregion

            #region ACT
            var connectionEntityCollection = new EntityCollection();
            connectionEntityCollection.Entities.Add(contactInstance1);
            connectionEntityCollection.Entities.Add(contactInstance2);

            var mockLogger = new Mock<ILogger>();
            var mockILanguageService = new Mock<ILanguageService>();
            var mockConfigurationService = new ConfigurationService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
            //Mock the IBingMap
            var mockBingMapService = InitializeBingMapMockService();
            //var mockBingServices = new BingMapService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService(), mockConfigurationService);
            var contactService = new ContactService(mockLogger.Object, mockBingMapService.Object, xrmFakedContext.GetFakedOrganizationService(), mockConfigurationService, mockILanguageService.Object);

            contactService.SetLegacyStudent(mockExecutionContext.Object);
            #endregion

            #region ASSERT

            var cmc_legacy = xrmFakedContext.GetFakedOrganizationService()
                            .Retrieve("contact", contactInstance1.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet(true))
                            .GetAttributeValue<bool>("cmc_legacy");

            Assert.IsFalse(cmc_legacy);

            #endregion
        }

        [TestMethod]
        [TestCategory("Plugin"), TestCategory("Positive")]
        public void SetLegacyStudent_UpdateConnection_ContactToAccountNonFamily()
        {
            #region ARRANGE

            var accountInstance1 = PrepareAccountInstance();
            cmc_configuration retrieveConfiguration = GetActiveConfiguration();
            var contactInstance1 = PreparingContactInstance(accountInstance1.Id, true);
            var contactInstance2 = PreparingContactInstance2(accountInstance1.Id, true);
            var connectionRole1 = PrepareChildRole();
            var connectionRole2 = PrepareParentRole();

            var preConnectionInstance = PrepareConnectionInstanceCtoA(contactInstance1, contactInstance2, connectionRole1, connectionRole2);
            var postConnectionInstance = PrepareConnectionInstanceCtoC(contactInstance1, contactInstance2, connectionRole1, connectionRole2, accountInstance1);
            postConnectionInstance.StateCode = ConnectionState.Active;
            postConnectionInstance.Record1RoleId = (new ConnectionRole() { Id = Guid.NewGuid() }).ToEntityReference();
            postConnectionInstance.Record2RoleId = (new ConnectionRole() { Id = Guid.NewGuid() }).ToEntityReference();
            preConnectionInstance.Record1RoleId = (new ConnectionRole() { Id = Guid.NewGuid() }).ToEntityReference();
            preConnectionInstance.Record2RoleId = (new ConnectionRole() { Id = Guid.NewGuid() }).ToEntityReference();
            var configuration = retrieveConfiguration;//PrepareConfiguration();


            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>()
            {

                postConnectionInstance,
                preConnectionInstance,
                accountInstance1,
                contactInstance1,
                contactInstance2,
                configuration,
                connectionRole1,
                connectionRole2
            });


            //Initialize the Mock Service
            var mockServiceProvider = InitializeMockService(xrmFakedContext, preConnectionInstance, Operation.Update);
            AddPreEntityImage(mockServiceProvider, "Target", preConnectionInstance);
            AddPostEntityImage(mockServiceProvider, "Target", postConnectionInstance);
            //Fetch the Mock Execution Context
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            #endregion

            #region ACT
            var connectionEntityCollection = new EntityCollection();
            connectionEntityCollection.Entities.Add(contactInstance1);
            connectionEntityCollection.Entities.Add(contactInstance2);

            var mockLogger = new Mock<ILogger>();
            var mockILanguageService = new Mock<ILanguageService>();
            var mockConfigurationService = new ConfigurationService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
            //Mock the IBingMap
            var mockBingMapService = InitializeBingMapMockService();

            //var mockBingServices = new BingMapService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService(), mockConfigurationService);
            var contactService = new ContactService(mockLogger.Object, mockBingMapService.Object, xrmFakedContext.GetFakedOrganizationService(), mockConfigurationService, mockILanguageService.Object);

            contactService.SetLegacyStudent(mockExecutionContext.Object);
            #endregion

            #region ASSERT

            var cmc_legacy = xrmFakedContext.GetFakedOrganizationService()
                            .Retrieve("contact", contactInstance1.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet(true))
                            .GetAttributeValue<bool>("mshied_legacy");

            Assert.IsTrue(cmc_legacy);

            #endregion
        }


        [TestCategory("Plugin"), TestCategory("Negative")]
        [TestMethod]
        public void SetLegacyStudent_CreateConnection_WhenCheckLegacyDisabledIs_True()  
        {
            #region ARRANGE

            var accountInstance1 = PrepareAccountInstance();
            
            var contactInstance1 = PreparingContactInstance(accountInstance1.Id);
            var contactInstance2 = PreparingContactInstance2(accountInstance1.Id);
            var childRoleInstance = PrepareChildRole();
            var parentRoleInstance = PrepareParentRole();
            var connectionInstance = PrepareConnectionInstance(contactInstance1, contactInstance2, parentRoleInstance, childRoleInstance, accountInstance1);
            var alumniInstance = PrepareAlumniInstanceforContact(contactInstance2, accountInstance1);
            var configuration = PrepareConfigurationHavingValueTrue();
            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>()
            {
                accountInstance1,
                connectionInstance,
                contactInstance1,
                contactInstance2,
                parentRoleInstance,
                childRoleInstance,
                alumniInstance,
                configuration
            });
            //Initialize the Mock Service
            var mockServiceProvider = InitializeMockService(xrmFakedContext, connectionInstance, Operation.Create);

            //Fetch the Mock Execution Context
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            #endregion

            #region ACT
            var connectionEntityCollection = new EntityCollection();
            connectionEntityCollection.Entities.Add(contactInstance1);
            connectionEntityCollection.Entities.Add(contactInstance2);

            var mockLogger = new Mock<ILogger>();
            var mockILanguageService = new Mock<ILanguageService>();

            var mockConfigurationService = new ConfigurationService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
            //Mock the IBingMap
            var mockBingMapService = InitializeBingMapMockService();

            //var mockBingServices = new BingMapService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService(), mockConfigurationService);
            var contactService = new ContactService(mockLogger.Object, mockBingMapService.Object, xrmFakedContext.GetFakedOrganizationService(), mockConfigurationService, mockILanguageService.Object);

            contactService.SetLegacyStudent(mockExecutionContext.Object);
            #endregion

            #region ASSERT

            var cmc_legacy = xrmFakedContext.GetFakedOrganizationService()
                            .Retrieve("contact", contactInstance1.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet(true))
                            .GetAttributeValue<bool>("cmc_legacy");

            Assert.IsFalse(cmc_legacy);

            #endregion
        }


        [TestCategory("Plugin"), TestCategory("Negative")]
        [TestMethod]
        public void SetLegacyStudent_DeleteConnection_WhenCheckLegacyDisabledIs_True() 
        {
            #region ARRANGE

            var accountInstance1 = PrepareAccountInstance();

            var contactInstance1 = PreparingContactInstance(accountInstance1.Id, true);
            var contactInstance2 = PreparingContactInstance2(accountInstance1.Id, true);
            var childRoleInstance = PrepareChildRole();
            var parentRoleInstance = PrepareParentRole();
            var connectionInstance = PrepareConnectionInstance(contactInstance1, contactInstance2, parentRoleInstance, childRoleInstance, accountInstance1);
            connectionInstance.StateCode = ConnectionState.Inactive;
            var preconnectionInstance = PrepareConnectionInstance(contactInstance1, contactInstance2, parentRoleInstance, childRoleInstance, accountInstance1);
            var alumniInstance = PrepareAlumniInstanceforContact(contactInstance2, accountInstance1);
            var configuration = PrepareConfigurationHavingValueTrue();

            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>()
            {
                accountInstance1,
                connectionInstance,
                preconnectionInstance,
                contactInstance1,
                contactInstance2,
                parentRoleInstance,
                childRoleInstance,
                alumniInstance,
                configuration
            });
            //Initialize the Mock Service
            var mockServiceProvider = InitializeMockService(xrmFakedContext, connectionInstance, Operation.Delete);
            AddPreEntityImage(mockServiceProvider, "Target", preconnectionInstance);
            //Fetch the Mock Execution Context
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            #endregion

            #region ACT
            var connectionEntityCollection = new EntityCollection();
            connectionEntityCollection.Entities.Add(contactInstance1);
            connectionEntityCollection.Entities.Add(contactInstance2);

            var mockLogger = new Mock<ILogger>();
            var mockILanguageService = new Mock<ILanguageService>();
            var mockConfigurationService = new ConfigurationService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
            //Mock the IBingMap
            var mockBingMapService = InitializeBingMapMockService();

            //var mockBingServices = new BingMapService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService(), mockConfigurationService);
            var contactService = new ContactService(mockLogger.Object, mockBingMapService.Object, xrmFakedContext.GetFakedOrganizationService(), mockConfigurationService, mockILanguageService.Object);

            contactService.SetLegacyStudent(mockExecutionContext.Object);
            #endregion

            #region ASSERT

            var cmc_legacy = xrmFakedContext.GetFakedOrganizationService()
                            .Retrieve("contact", contactInstance1.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet(true))
                            .GetAttributeValue<bool>("mshied_legacy");

            Assert.IsTrue(cmc_legacy);

            #endregion
        }


        [TestCategory("Plugin"), TestCategory("Negative")]
        [TestMethod]
        public void SetLegacyStudent_DeleteConnection_WhenConnectionStateIsInactive()  
        {
            #region ARRANGE

            var accountInstance1 = PrepareAccountInstance();
            cmc_configuration retrieveConfiguration = GetActiveConfiguration();
            var contactInstance1 = PreparingContactInstance(accountInstance1.Id, true);
            var contactInstance2 = PreparingContactInstance2(accountInstance1.Id, true);
            var childRoleInstance = PrepareChildRole();
            var parentRoleInstance = PrepareParentRole();
            var connectionInstance = PrepareConnectionInstanceHavingInactiveState(contactInstance1, contactInstance2, parentRoleInstance, childRoleInstance, accountInstance1);
            connectionInstance.StateCode = ConnectionState.Inactive;
            var preconnectionInstance = PrepareConnectionInstanceHavingInactiveState(contactInstance1, contactInstance2, parentRoleInstance, childRoleInstance, accountInstance1);
            var alumniInstance = PrepareAlumniInstanceforContact(contactInstance2, accountInstance1);
            var configuration = retrieveConfiguration;//PrepareConfiguration();

            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>()
            {
                accountInstance1,
                connectionInstance,
                preconnectionInstance,
                contactInstance1,
                contactInstance2,
                parentRoleInstance,
                childRoleInstance,
                alumniInstance,
                configuration
            });
            //Initialize the Mock Service
            var mockServiceProvider = InitializeMockService(xrmFakedContext, connectionInstance, Operation.Delete);
            AddPreEntityImage(mockServiceProvider, "Target", preconnectionInstance);
            //Fetch the Mock Execution Context
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            #endregion

            #region ACT
            var connectionEntityCollection = new EntityCollection();
            connectionEntityCollection.Entities.Add(contactInstance1);
            connectionEntityCollection.Entities.Add(contactInstance2);

            var mockLogger = new Mock<ILogger>();
            var mockILanguageService = new Mock<ILanguageService>();
            var mockConfigurationService = new ConfigurationService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
            //Mock the IBingMap
            var mockBingMapService = InitializeBingMapMockService();

            //var mockBingServices = new BingMapService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService(), mockConfigurationService);
            var contactService = new ContactService(mockLogger.Object, mockBingMapService.Object, xrmFakedContext.GetFakedOrganizationService(), mockConfigurationService, mockILanguageService.Object);

            contactService.SetLegacyStudent(mockExecutionContext.Object);
            #endregion

            #region ASSERT

            var cmc_legacy = xrmFakedContext.GetFakedOrganizationService()
                            .Retrieve("contact", contactInstance1.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet(true))
                            .GetAttributeValue<bool>("mshied_legacy");

            Assert.IsTrue(cmc_legacy);

            #endregion
        }


        [TestCategory("Plugin"), TestCategory("Negative")]
        [TestMethod]
        public void SetLegacyStudent_UpdateConnection_Family_IsLegacyCheckDisabledIsTrue()  
        {
            #region ARRANGE

            var accountInstance1 = PrepareAccountInstance();

            var contactInstance1 = PreparingContactInstance(accountInstance1.Id, true);
            var contactInstance2 = PreparingContactInstance2(accountInstance1.Id, true);
            var childRoleInstance = PrepareChildRole();
            var parentRoleInstance = PrepareParentRole();
            var connectionInstance = PrepareConnectionInstance(contactInstance1, contactInstance2, parentRoleInstance, childRoleInstance, accountInstance1);
            connectionInstance.StateCode = ConnectionState.Inactive;
            var preconnectionInstance = PrepareConnectionInstance(contactInstance1, contactInstance2, parentRoleInstance, childRoleInstance, accountInstance1);
            var alumniInstance = PrepareAlumniInstanceforContact(contactInstance2, accountInstance1);
            var configuration = PrepareConfigurationHavingValueTrue();

            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>()
            {
                accountInstance1,
                connectionInstance,
                preconnectionInstance,
                contactInstance1,
                contactInstance2,
                parentRoleInstance,
                childRoleInstance,
                alumniInstance,
                configuration
            });

            //Initialize the Mock Service
            var mockServiceProvider = InitializeMockService(xrmFakedContext, connectionInstance, Operation.Update);
            AddPreEntityImage(mockServiceProvider, "Target", preconnectionInstance);
            AddPostEntityImage(mockServiceProvider, "Target", connectionInstance);

            //Fetch the Mock Execution Context
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            #endregion

            #region ACT
            var connectionEntityCollection = new EntityCollection();
            connectionEntityCollection.Entities.Add(contactInstance1);
            connectionEntityCollection.Entities.Add(contactInstance2);

            var mockLogger = new Mock<ILogger>();
            var mockILanguageService = new Mock<ILanguageService>();
            var mockConfigurationService = new ConfigurationService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
            //Mock the IBingMap
            var mockBingMapService = InitializeBingMapMockService();

            //var mockBingServices = new BingMapService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService(), mockConfigurationService);
            var contactService = new ContactService(mockLogger.Object, mockBingMapService.Object, xrmFakedContext.GetFakedOrganizationService(), mockConfigurationService, mockILanguageService.Object);

            contactService.SetLegacyStudent(mockExecutionContext.Object);
            #endregion

            #region ASSERT

            var cmc_legacy = xrmFakedContext.GetFakedOrganizationService()
                            .Retrieve("contact", contactInstance1.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet(true))
                            .GetAttributeValue<bool>("mshied_legacy");

            Assert.IsTrue(cmc_legacy);

            #endregion
        }


        [TestCategory("Plugin"), TestCategory("Negative")]
        [TestMethod]
        public void SetLegacyStudent_UpdateConnection_Family_WhenContactAsLegacyHaveNotBeenMet()  
        {
            #region ARRANGE

            var accountInstance1 = PrepareAccountInstance();
            cmc_configuration retrieveConfiguration = GetActiveConfiguration();
            var contactInstance1 = PreparingContactInstance(accountInstance1.Id, true);
            var contactInstance2 = PreparingContactInstance2(accountInstance1.Id, true);
            var childRoleInstance = PrepareChildRole();
            var parentRoleInstance = PrepareParentRole();
            var connectionInstance = PrepareConnectionInstance(contactInstance1, contactInstance2, parentRoleInstance, childRoleInstance, accountInstance1);
            connectionInstance.StateCode = ConnectionState.Inactive;
            var preconnectionInstance = PrepareConnectionInstance(contactInstance1, contactInstance2, parentRoleInstance, childRoleInstance, accountInstance1);
            var alumniInstance = PrepareAlumniInstanceforContact(contactInstance2, accountInstance1);
            var configuration = retrieveConfiguration;//PrepareConfiguration();

            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>()
            {
                accountInstance1,
                connectionInstance,
                preconnectionInstance,
                contactInstance1,
                contactInstance2,
                parentRoleInstance,
                childRoleInstance,
                alumniInstance,
                configuration
            });

            //Initialize the Mock Service
            var mockServiceProvider = InitializeMockService(xrmFakedContext, connectionInstance, Operation.Update);
            AddPreEntityImage(mockServiceProvider, "Target", preconnectionInstance);
            AddPostEntityImage(mockServiceProvider, "Target", preconnectionInstance);

            //Fetch the Mock Execution Context
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            #endregion

            #region ACT
            var connectionEntityCollection = new EntityCollection();
            connectionEntityCollection.Entities.Add(contactInstance1);
            connectionEntityCollection.Entities.Add(contactInstance2);

            var mockLogger = new Mock<ILogger>();
            var mockILanguageService = new Mock<ILanguageService>();
            var mockConfigurationService = new ConfigurationService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
            //Mock the IBingMap
            var mockBingMapService = InitializeBingMapMockService();

            //var mockBingServices = new BingMapService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService(), mockConfigurationService);
            var contactService = new ContactService(mockLogger.Object, mockBingMapService.Object, xrmFakedContext.GetFakedOrganizationService(), mockConfigurationService, mockILanguageService.Object);

            contactService.SetLegacyStudent(mockExecutionContext.Object);
            #endregion

            #region ASSERT

            var cmc_legacy = xrmFakedContext.GetFakedOrganizationService()
                            .Retrieve("contact", contactInstance1.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet(true))
                            .GetAttributeValue<bool>("mshied_legacy");

            Assert.IsTrue(cmc_legacy);

            #endregion
        }


        [TestCategory("Plugin"), TestCategory("Negative")]
        [TestMethod]
        public void SetLegacyStudent_UpdateConnection_Family_WhenPreConnectionIsInactiveAndUpdateConnnectionIsActive()  
        {
            #region ARRANGE

            var accountInstance1 = PrepareAccountInstance();
            cmc_configuration retrieveConfiguration = GetActiveConfiguration();
            var contactInstance1 = PreparingContactInstance(accountInstance1.Id, true);
            var contactInstance2 = PreparingContactInstance2(accountInstance1.Id, true);
            var childRoleInstance = PrepareChildRole();
            var parentRoleInstance = PrepareParentRole();
            var connectionInstance = PrepareConnectionInstance(contactInstance1, contactInstance2, parentRoleInstance, childRoleInstance, accountInstance1);

            var preconnectionInstance = PreConnectionInstanceInactive(contactInstance1, contactInstance2, parentRoleInstance, childRoleInstance, accountInstance1);
            var alumniInstance = PrepareAlumniInstanceforContact(contactInstance2, accountInstance1);
            var configuration = retrieveConfiguration;//PrepareConfiguration();

            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>()
            {
                accountInstance1,
                connectionInstance,
                preconnectionInstance,
                contactInstance1,
                contactInstance2,
                parentRoleInstance,
                childRoleInstance,
                alumniInstance,
                configuration
            });

            //Initialize the Mock Service
            var mockServiceProvider = InitializeMockService(xrmFakedContext, connectionInstance, Operation.Update);
            AddPreEntityImage(mockServiceProvider, "Target", preconnectionInstance);
            AddPostEntityImage(mockServiceProvider, "Target", connectionInstance);

            //Fetch the Mock Execution Context
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            #endregion

            #region ACT
            var connectionEntityCollection = new EntityCollection();
            connectionEntityCollection.Entities.Add(contactInstance1);
            connectionEntityCollection.Entities.Add(contactInstance2);

            var mockLogger = new Mock<ILogger>();
            var mockILanguageService = new Mock<ILanguageService>();
            var mockConfigurationService = new ConfigurationService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
            //Mock the IBingMap
            var mockBingMapService = InitializeBingMapMockService();

            //var mockBingServices = new BingMapService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService(), mockConfigurationService);
            var contactService = new ContactService(mockLogger.Object, mockBingMapService.Object, xrmFakedContext.GetFakedOrganizationService(), mockConfigurationService, mockILanguageService.Object);

            contactService.SetLegacyStudent(mockExecutionContext.Object);
            #endregion

            #region ASSERT

            var cmc_legacy = xrmFakedContext.GetFakedOrganizationService()
                            .Retrieve("contact", contactInstance1.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet(true))
                            .GetAttributeValue<bool>("mshied_legacy");

            Assert.IsTrue(cmc_legacy);

            #endregion
        }


        [TestCategory("Plugin"), TestCategory("Negative")]
        [TestMethod]
        public void SetLegacyStudent_UpdateConnection_Family_WhenPreConnectionIsInactiveAndUpdateConnnectionIsInactive()
        {
            #region ARRANGE

            var accountInstance1 = PrepareAccountInstance();
            cmc_configuration retrieveConfiguration = GetActiveConfiguration();
            var contactInstance1 = PreparingContactInstance(accountInstance1.Id, true);
            var contactInstance2 = PreparingContactInstance2(accountInstance1.Id, true);
            var childRoleInstance = PrepareChildRole();
            var parentRoleInstance = PrepareParentRole();
            var connectionInstance = PreConnectionInstanceInactive(contactInstance1, contactInstance2, parentRoleInstance, childRoleInstance, accountInstance1);

            var preconnectionInstance = PreConnectionInstanceInactive(contactInstance2, contactInstance1, parentRoleInstance, childRoleInstance, accountInstance1);
            var alumniInstance = PrepareAlumniInstanceforContact(contactInstance2, accountInstance1);
            var configuration = retrieveConfiguration;//PrepareConfiguration();

            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>()
            {
                accountInstance1,
                connectionInstance,
                preconnectionInstance,
                contactInstance1,
                contactInstance2,
                parentRoleInstance,
                childRoleInstance,
                alumniInstance,
                configuration
            });

            //Initialize the Mock Service
            var mockServiceProvider = InitializeMockService(xrmFakedContext, connectionInstance, Operation.Update);
            AddPreEntityImage(mockServiceProvider, "Target", preconnectionInstance);
            AddPostEntityImage(mockServiceProvider, "Target", connectionInstance);

            //Fetch the Mock Execution Context
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            #endregion

            #region ACT
            var connectionEntityCollection = new EntityCollection();
            connectionEntityCollection.Entities.Add(contactInstance1);
            connectionEntityCollection.Entities.Add(contactInstance2);

            var mockLogger = new Mock<ILogger>();
            var mockILanguageService = new Mock<ILanguageService>();
            var mockConfigurationService = new ConfigurationService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
            //Mock the IBingMap
            var mockBingMapService = InitializeBingMapMockService();

            //var mockBingServices = new BingMapService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService(), mockConfigurationService);
            var contactService = new ContactService(mockLogger.Object, mockBingMapService.Object, xrmFakedContext.GetFakedOrganizationService(), mockConfigurationService, mockILanguageService.Object);

            contactService.SetLegacyStudent(mockExecutionContext.Object);
            #endregion

            #region ASSERT

            var cmc_legacy = xrmFakedContext.GetFakedOrganizationService()
                            .Retrieve("contact", contactInstance1.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet(true))
                            .GetAttributeValue<bool>("mshied_legacy");

            Assert.IsTrue(cmc_legacy);

            #endregion
        }


        [TestCategory("Plugin"), TestCategory("Positive")]  
        [TestMethod]
        public void SetLegacyStudent_UpdateConnection_IsFamilyRoleToNonFamilyRole()
        {
            #region ARRANGE

            var accountInstance1 = PrepareAccountInstance();
            cmc_configuration retrieveConfiguration = GetActiveConfiguration();
            var contactInstance1 = PreparingContactInstance(accountInstance1.Id, true);
            var contactInstance2 = PreparingContactInstance2(accountInstance1.Id, true);
            var childRoleInstance = PrepareChildRole();
            var parentRoleInstance = PrepareParentRole();
            var connectionInstance = PrepareConnectionInstance(contactInstance1, contactInstance2, parentRoleInstance, childRoleInstance, accountInstance1);

            var preconnectionInstance = PostPrepareConnectionInstanceWhenIsFamilyRoleToNonFamilyRole(contactInstance1, contactInstance2, parentRoleInstance, childRoleInstance, accountInstance1);
            var alumniInstance = PrepareAlumniInstanceforContact(contactInstance2, accountInstance1, preconnectionInstance);
            var configuration = retrieveConfiguration;//PrepareConfiguration();

            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>()
            {
                accountInstance1,
                connectionInstance,
                preconnectionInstance,
                contactInstance1,
                contactInstance2,
                parentRoleInstance,
                childRoleInstance,
                alumniInstance,
                configuration
            });

            //Initialize the Mock Service
            var mockServiceProvider = InitializeMockService(xrmFakedContext, connectionInstance, Operation.Update);
            AddPreEntityImage(mockServiceProvider, "Target", connectionInstance);
            AddPostEntityImage(mockServiceProvider, "Target", preconnectionInstance);

            //Fetch the Mock Execution Context
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            #endregion

            #region ACT
            var connectionEntityCollection = new EntityCollection();
            connectionEntityCollection.Entities.Add(contactInstance1);
            connectionEntityCollection.Entities.Add(contactInstance2);

            var mockLogger = new Mock<ILogger>();
            var mockILanguageService = new Mock<ILanguageService>();
            var mockConfigurationService = new ConfigurationService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
            //Mock the IBingMap
            var mockBingMapService = InitializeBingMapMockService();

            //var mockBingServices = new BingMapService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService(), mockConfigurationService);
            var contactService = new ContactService(mockLogger.Object, mockBingMapService.Object, xrmFakedContext.GetFakedOrganizationService(), mockConfigurationService, mockILanguageService.Object);

            contactService.SetLegacyStudent(mockExecutionContext.Object);
            #endregion

            #region ASSERT

            var cmc_legacy = xrmFakedContext.GetFakedOrganizationService()
                            .Retrieve("contact", contactInstance1.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet(true))
                            .GetAttributeValue<bool>("cmc_legacy");

            Assert.IsFalse(cmc_legacy);

            #endregion
        }

        [TestCategory("Plugin"), TestCategory("Positive")]
        [TestMethod]
        public void SetLegacyStudent_CreateConnection_WhenRecord1andRecord2IsNotContactORAccount() 
        {
            #region ARRANGE

            var accountInstance1 = PrepareAccountInstance();
            cmc_configuration retrieveConfiguration = GetActiveConfiguration();
            var contactInstance1 = PreparingContactInstance(accountInstance1.Id);
            var contactInstance2 = PreparingContactInstance2(accountInstance1.Id);
            var childRoleInstance = PrepareChildRole(contactInstance2.Id);
            var parentRoleInstance = PrepareParentRole(contactInstance1.Id);
            var connectionInstance = PrepareConnectionInstanceAtoCHavingNoContactandAccount(contactInstance1, accountInstance1, parentRoleInstance, childRoleInstance);//PrepareConnectionInstance(contactInstance1, contactInstance2, parentRoleInstance, childRoleInstance, accountInstance1);
            var alumniInstance = PrepareAlumniInstanceforContact(contactInstance2, accountInstance1);
            var configuration = retrieveConfiguration;//PrepareConfiguration();
            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>()
            {
                accountInstance1,
                connectionInstance,
                contactInstance1,
                contactInstance2,
                parentRoleInstance,
                childRoleInstance,
                alumniInstance,
                configuration
            });
            //Initialize the Mock Service
            var mockServiceProvider = InitializeMockService(xrmFakedContext, connectionInstance, Operation.Create);

            //Fetch the Mock Execution Context
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            #endregion

            #region ACT
            var connectionEntityCollection = new EntityCollection();
            connectionEntityCollection.Entities.Add(contactInstance1);
            connectionEntityCollection.Entities.Add(contactInstance2);

            var mockLogger = new Mock<ILogger>();
            var mockILanguageService = new Mock<ILanguageService>();
            var mockConfigurationService = new ConfigurationService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
            //Mock the IBingMap
            var mockBingMapService = InitializeBingMapMockService();

            //var mockBingServices = new BingMapService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService(), mockConfigurationService);
            var contactService = new ContactService(mockLogger.Object, mockBingMapService.Object, xrmFakedContext.GetFakedOrganizationService(), mockConfigurationService, mockILanguageService.Object);

            contactService.SetLegacyStudent(mockExecutionContext.Object);
            #endregion

            #region ASSERT

            var cmc_legacy = xrmFakedContext.GetFakedOrganizationService()
                            .Retrieve("contact", contactInstance1.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet(true))
                            .GetAttributeValue<bool>("cmc_legacy");

            Assert.IsFalse(cmc_legacy);

            #endregion
        }


        [TestCategory("Plugin"), TestCategory("Positive")]
        [TestMethod]
        public void SetLegacyStudent_CreateConnection_WhenBothContactHas_Alumni_roles()  
        {
            #region ARRANGE

            var accountInstance1 = PrepareAccountInstance();
            cmc_configuration retrieveConfiguration = GetActiveConfiguration();
            var contactInstance1 = PreparingContactInstance(accountInstance1.Id,true);
            var contactInstance2 = PreparingContactInstance2(accountInstance1.Id, true);
            var childRoleInstance = PrepareChildRole(contactInstance2.Id);
            var parentRoleInstance = PrepareParentRole(contactInstance1.Id);
            var connectionInstance = PrepareConnectionInstanceAtoC_(contactInstance1, accountInstance1,parentRoleInstance,childRoleInstance);
            var connectionInstance2 = PrepareConnectionInstanceAtoC__(contactInstance1, accountInstance1, parentRoleInstance, childRoleInstance);
            var alumniInstance = PrepareAlumniInstanceforContact(contactInstance2, accountInstance1);
            var configuration = retrieveConfiguration;//PrepareConfiguration();
            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>()
            {
                accountInstance1,
                connectionInstance,connectionInstance2,
                contactInstance1,
                contactInstance2,
                parentRoleInstance,
                childRoleInstance,
                alumniInstance,
                configuration
            });
            //Initialize the Mock Service
            var mockServiceProvider = InitializeMockService(xrmFakedContext, connectionInstance, Operation.Create);

            //Fetch the Mock Execution Context
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            #endregion

            #region ACT
            var connectionEntityCollection = new EntityCollection();
            connectionEntityCollection.Entities.Add(contactInstance1);
            connectionEntityCollection.Entities.Add(contactInstance2);

            var mockLogger = new Mock<ILogger>();
            var mockILanguageService = new Mock<ILanguageService>();
            var mockConfigurationService = new ConfigurationService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
            //Mock the IBingMap
            var mockBingMapService = InitializeBingMapMockService();

            //var mockBingServices = new BingMapService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService(), mockConfigurationService);
            var contactService = new ContactService(mockLogger.Object, mockBingMapService.Object, xrmFakedContext.GetFakedOrganizationService(), mockConfigurationService, mockILanguageService.Object);

            contactService.SetLegacyStudent(mockExecutionContext.Object);
            #endregion

            #region ASSERT

            var cmc_legacy = xrmFakedContext.GetFakedOrganizationService()
                            .Retrieve("contact", contactInstance1.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet(true))
                            .GetAttributeValue<bool>("mshied_legacy");

            Assert.IsTrue(cmc_legacy);

            #endregion
        }


        [TestCategory("Plugin"), TestCategory("Negative")]
        [TestMethod]
        public void SetLegacyStudent_CreateConnectionWhenConnectionIsNotAFamilyConnection()  
        {
            #region ARRANGE

            var accountInstance1 = PrepareAccountInstance();
            cmc_configuration retrieveConfiguration = GetActiveConfiguration();
            var contactInstance1 = PreparingContactInstance(accountInstance1.Id);
            var contactInstance2 = PreparingContactInstance2(accountInstance1.Id);
            var childRoleInstance = PrepareChildRole(contactInstance2.Id);
            var parentRoleInstance = PrepareParentRole(contactInstance1.Id);
            var connectionInstance = PrepareConnectionInstanceCtoCHavingNoFamilyConnection(contactInstance1, contactInstance2, parentRoleInstance, childRoleInstance,accountInstance1);
            var connectionInstance2 = PrepareConnectionInstanceAtoC__(contactInstance1, accountInstance1, parentRoleInstance, childRoleInstance);
            var alumniInstance = PrepareAlumniInstanceforContact(contactInstance2, accountInstance1);
            var configuration = retrieveConfiguration;//PrepareConfiguration();
            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>()
            {
                accountInstance1,
                connectionInstance,connectionInstance2,
                contactInstance1,
                contactInstance2,
                parentRoleInstance,
                childRoleInstance,
                alumniInstance,
                configuration
            });
            //Initialize the Mock Service
            var mockServiceProvider = InitializeMockService(xrmFakedContext, connectionInstance, Operation.Create);

            //Fetch the Mock Execution Context
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            #endregion

            #region ACT
            var connectionEntityCollection = new EntityCollection();
            connectionEntityCollection.Entities.Add(contactInstance1);
            connectionEntityCollection.Entities.Add(contactInstance2);

            var mockLogger = new Mock<ILogger>();
            var mockILanguageService = new Mock<ILanguageService>();
            var mockConfigurationService = new ConfigurationService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
            //Mock the IBingMap
            var mockBingMapService = InitializeBingMapMockService();

            //var mockBingServices = new BingMapService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService(), mockConfigurationService);
            var contactService = new ContactService(mockLogger.Object, mockBingMapService.Object, xrmFakedContext.GetFakedOrganizationService(), mockConfigurationService, mockILanguageService.Object);

            contactService.SetLegacyStudent(mockExecutionContext.Object);
            #endregion

            #region ASSERT

            var cmc_legacy = xrmFakedContext.GetFakedOrganizationService()
                            .Retrieve("contact", contactInstance1.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet(true))
                            .GetAttributeValue<bool>("cmc_legacy");

            Assert.IsFalse(cmc_legacy);

            #endregion
        }



        [TestCategory("Plugin"), TestCategory("Negative")]
        [TestMethod]
        public void SetLegacyStudent_CreateConnection_WhenAccountIsNotATypeOfCampus() 
        {
            #region ARRANGE

            var accountInstance1 = PrepareAccountInstanceHavingAccountTypeCollege();
            cmc_configuration retrieveConfiguration = GetActiveConfiguration();
            var contactInstance1 = PreparingContactInstance(accountInstance1.Id);
            var contactInstance2 = PreparingContactInstance2(accountInstance1.Id);
            var childRoleInstance = PrepareChildRole(contactInstance2.Id);
            var parentRoleInstance = PrepareParentRole(contactInstance1.Id);
            var connectionInstance = PrepareConnectionInstanceAtoC_(contactInstance1, accountInstance1, parentRoleInstance, childRoleInstance);
            var connectionInstance2 = PrepareConnectionInstanceAtoC__(contactInstance1, accountInstance1, parentRoleInstance, childRoleInstance);
            var alumniInstance = PrepareAlumniInstanceforContact(contactInstance2, accountInstance1);
            var configuration = retrieveConfiguration;//PrepareConfiguration();
            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>()
            {
                accountInstance1,
                connectionInstance,connectionInstance2,
                contactInstance1,
                contactInstance2,
                parentRoleInstance,
                childRoleInstance,
                alumniInstance,
                configuration
            });
            //Initialize the Mock Service
            var mockServiceProvider = InitializeMockService(xrmFakedContext, connectionInstance, Operation.Create);

            //Fetch the Mock Execution Context
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            #endregion

            #region ACT
            var connectionEntityCollection = new EntityCollection();
            connectionEntityCollection.Entities.Add(contactInstance1);
            connectionEntityCollection.Entities.Add(contactInstance2);

            var mockLogger = new Mock<ILogger>();
            var mockILanguageService = new Mock<ILanguageService>();
            var mockConfigurationService = new ConfigurationService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
            //Mock the IBingMap
            var mockBingMapService = InitializeBingMapMockService();

            //var mockBingServices = new BingMapService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService(), mockConfigurationService);
            var contactService = new ContactService(mockLogger.Object, mockBingMapService.Object, xrmFakedContext.GetFakedOrganizationService(), mockConfigurationService, mockILanguageService.Object);

            contactService.SetLegacyStudent(mockExecutionContext.Object);
            #endregion

            #region ASSERT

            var cmc_legacy = xrmFakedContext.GetFakedOrganizationService()
                            .Retrieve("contact", contactInstance1.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet(true))
                            .GetAttributeValue<bool>("cmc_legacy");

            Assert.IsFalse(cmc_legacy);

            #endregion
        }


        [TestMethod]
        [TestCategory("Plugin"), TestCategory("Positive")]
        public void SetLegacyStudent_UpdateConnection_AccountToContact_WhenContactHasMoreThanOneFamilyConnection() 
        {
            #region ARRANGE

            var accountInstance1 = PrepareAccountInstance();
            cmc_configuration retrieveConfiguration = GetActiveConfiguration();
            var contactInstance1 = PreparingContactInstance(accountInstance1.Id, true);
            var contactInstance2 = PreparingContactInstance2(accountInstance1.Id, true);
            var connectionRole1 = PrepareChildRole();
            var connectionRole2 = PrepareParentRole();
            var postConnectionInstance = PrepareConnectionInstanceforCount(contactInstance1, contactInstance1, connectionRole1, connectionRole2, accountInstance1);
            var preConnectionInstance = PrepareConnectionInstanceAtoCforCount(contactInstance1, accountInstance1, connectionRole1, connectionRole2);
            var configuration = retrieveConfiguration;//PrepareConfiguration();


            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>()
            {
                postConnectionInstance,
                preConnectionInstance,
                accountInstance1,
                contactInstance1,
                contactInstance2,
                configuration,
                connectionRole1,
                connectionRole2
            });


            //Initialize the Mock Service
            var mockServiceProvider = InitializeMockService(xrmFakedContext, preConnectionInstance, Operation.Update);
            AddPreEntityImage(mockServiceProvider, "Target", preConnectionInstance);
            AddPostEntityImage(mockServiceProvider, "Target", postConnectionInstance);
            //Fetch the Mock Execution Context
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            #endregion

            #region ACT
            var connectionEntityCollection = new EntityCollection();
            connectionEntityCollection.Entities.Add(contactInstance1);
            connectionEntityCollection.Entities.Add(contactInstance2);

            var mockLogger = new Mock<ILogger>();
            var mockILanguageService = new Mock<ILanguageService>();
            var mockConfigurationService = new ConfigurationService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
            //Mock the IBingMap
            var mockBingMapService = InitializeBingMapMockService();

            //var mockBingServices = new BingMapService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService(), mockConfigurationService);
            var contactService = new ContactService(mockLogger.Object, mockBingMapService.Object, xrmFakedContext.GetFakedOrganizationService(), mockConfigurationService, mockILanguageService.Object);

            contactService.SetLegacyStudent(mockExecutionContext.Object);
            #endregion

            #region ASSERT

            var cmc_legacy = xrmFakedContext.GetFakedOrganizationService()
                            .Retrieve("contact", contactInstance1.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet(true))
                            .GetAttributeValue<bool>("cmc_legacy");

            Assert.IsFalse(cmc_legacy);

            #endregion
        }

        #region Data Preparation

        private Connection PrepareConnectionInstance(Entity contact1, Entity contact2, Entity parentConnection, Entity ChildConnection, Entity accountInstance)
        {
            var connectionInstance = new Connection()
            {
                ConnectionId = Guid.NewGuid(),
                Record1Id = contact1.ToEntityReference(),
                Record1RoleId = parentConnection.ToEntityReference(),  
                Record2Id = contact2.ToEntityReference(),
                Record2RoleId = ChildConnection.ToEntityReference(),
                contact_connections1 = contact1.ToEntity<Models.Contact>(),
                contact_connections2 = contact2.ToEntity<Models.Contact>(),
                StateCode = ConnectionState.Active,
                account_connections1 = accountInstance.ToEntity<Account>()
            };
            return connectionInstance;
        }

        private Connection PrepareConnectionInstanceforCount(Entity contact1, Entity contact2, Entity parentConnection, Entity ChildConnection, Entity accountInstance)
        {
            var connectionInstance = new Connection()
            {
                ConnectionId = Guid.NewGuid(),
                Record1Id = contact2.ToEntityReference(),
                Record1RoleId = parentConnection.ToEntityReference(),
                Record2Id = contact1.ToEntityReference(),
                Record2RoleId = ChildConnection.ToEntityReference(),
                StateCode = ConnectionState.Active,
               ["record1objecttypecode"] = connection_record1objecttypecode.Contact
            };
            return connectionInstance;
        }

        private Connection PrepareConnectionInstanceHavingInactiveState(Entity contact1, Entity contact2, Entity parentConnection, Entity ChildConnection, Entity accountInstance)
        {
            var connectionInstance = new Connection()
            {
                ConnectionId = Guid.NewGuid(),
                Record1Id = contact1.ToEntityReference(),
                Record1RoleId = parentConnection.ToEntityReference(),
                Record2Id = contact2.ToEntityReference(),
                Record2RoleId = ChildConnection.ToEntityReference(),
                contact_connections1 = contact1.ToEntity<Models.Contact>(),
                contact_connections2 = contact2.ToEntity<Models.Contact>(),
                StateCode = ConnectionState.Inactive,
                account_connections1 = accountInstance.ToEntity<Account>()
            };
            return connectionInstance;
        }

        private Connection PreConnectionInstanceInactive(Entity contact1, Entity contact2, Entity parentConnection, Entity ChildConnection, Entity accountInstance)
        {
            var connectionInstance = new Connection()
            {
                ConnectionId = Guid.NewGuid(),
                Record1Id = contact1.ToEntityReference(),
                Record1RoleId = parentConnection.ToEntityReference(),
                Record2Id = contact2.ToEntityReference(),
                Record2RoleId = ChildConnection.ToEntityReference(),
                contact_connections1 = contact1.ToEntity<Models.Contact>(),
                contact_connections2 = contact2.ToEntity<Models.Contact>(),
                StateCode = ConnectionState.Inactive,
                account_connections1 = accountInstance.ToEntity<Account>()

            };
            return connectionInstance;
        }

        private Connection PostPrepareConnectionInstanceWhenIsFamilyRoleToNonFamilyRole(Entity contact1, Entity contact2, Entity parentConnection, Entity ChildConnection, Entity accountInstance)
        {
            var connectionInstance = new Connection()
            {
                ConnectionId = Guid.NewGuid(),
                Record1Id = contact1.ToEntityReference(),
                Record1RoleId = parentConnection.ToEntityReference(),
                Record2Id = contact2.ToEntityReference(),
                Record2RoleId = (new ConnectionRole() { Id = new Guid("3BB3FEDD-0333-E811-A95F-000D3A11FE33") }).ToEntityReference(),//ChildConnection.ToEntityReference(),
                contact_connections1 = contact1.ToEntity<Models.Contact>(),
                contact_connections2 = contact2.ToEntity<Models.Contact>(),
                StateCode = ConnectionState.Active,
                account_connections1 = accountInstance.ToEntity<Account>()
            };
            return connectionInstance;
        }

        private Connection PrepareAlumniInstanceforContact(Entity Contact, Entity account)
        {

            var connectionInstance = new Connection()
            {
                ConnectionId = Guid.NewGuid(),
                Record1Id = Contact.ToEntityReference(),
                Record1RoleId = (new ConnectionRole() { Id = new Guid("3BB3FEDD-0333-E811-A95F-000D3A11FE32") }).ToEntityReference(),
                Record2Id = account.ToEntityReference(),
                Record2RoleId = (new ConnectionRole() { Id = new Guid("3BB3FEDD-0333-E811-A95F-000D3A11FE32") }).ToEntityReference(),
                StateCode = ConnectionState.Active,
            };
            return connectionInstance;
        }

        private Connection PrepareAlumniInstanceforContact(Entity Contact, Entity account, Connection connection)
        {
            var connectionInstance = new Connection()
            {
                ConnectionId = connection.Id,//Guid.NewGuid(),
                Record1Id = Contact.ToEntityReference(),
                Record1RoleId = (new ConnectionRole() { Id = new Guid("3BB3FEDD-0333-E811-A95F-000D3A11FE32") }).ToEntityReference(),
                Record2Id = account.ToEntityReference(),
                Record2RoleId = (new ConnectionRole() { Id = new Guid("3BB3FEDD-0333-E811-A95F-000D3A11FE32") }).ToEntityReference(),
                StateCode = ConnectionState.Active,
            };
            return connectionInstance;
        }

        private Entity PreparingContactInstance(Guid accountInstance, bool setLegacy = false)
        {
            var contact = new Models.Contact()
            {
                ContactId = Guid.NewGuid(),
                FirstName = "ankkur",
                LastName = "Kushwaha",
                cmc_sourcecampusid = new EntityReference("account", accountInstance),
                AccountRoleCode = contact_accountrolecode.Employee,
                //ParentCustomerId = new EntityReference("account", accountInstance),                
                mshied_Legacy = setLegacy,
            };
            return contact;
        }

        private Entity PreparingContactInstance2(Guid accountInstance, bool setLegacy = false)
        {
            var contact = new Models.Contact()
            {
                ContactId = Guid.NewGuid(),
                FirstName = "Test",
                LastName = "contact",
                cmc_sourcecampusid = new EntityReference("account", accountInstance),
                AccountRoleCode = contact_accountrolecode.Employee,
                //ParentCustomerId = new EntityReference("account", accountInstance),                
                mshied_Legacy = setLegacy,
            };
            return contact;
        }

        private Account PrepareAccountInstance()
        {
            //Creating an Account2 to change the contact campus.
            var accountInstance = new Account()
            {
                Id = Guid.NewGuid(),
                mshied_AccountType = mshied_account_mshied_accounttype.Campus
            };
            return accountInstance;
        }

        private Account PrepareAccountInstanceHavingAccountTypeCollege()
        {
            //Creating an Account2 to change the contact campus.
            var accountInstance = new Account()
            {
                Id = Guid.NewGuid(),
                mshied_AccountType = mshied_account_mshied_accounttype.College
            };
            return accountInstance;
        }


        private ConnectionRole PrepareChildRole()
        {
            var connectionRoleInstance = new ConnectionRole()
            {
                Id = Guid.NewGuid(),
                Name = "Child",
                Category = new OptionSetValue((int)connectionrole_category.Family)
            };
            return connectionRoleInstance;
        }

        private ConnectionRole PrepareChildRole(Guid guid)
        {
            var connectionRoleInstance = new ConnectionRole()
            {
                Id = Guid.NewGuid(),ConnectionRoleId=guid,
                Name = "Child",
                Category = new OptionSetValue((int)connectionrole_category.Family)
            };
            return connectionRoleInstance;
        }

        private ConnectionRole PrepareParentRole()
        {
            var connectionRoleInstance = new ConnectionRole()
            {
                Id = Guid.NewGuid(),
                Name = "Parent",
                Category = new OptionSetValue((int)connectionrole_category.Family)
            };
            return connectionRoleInstance;
        }
        private ConnectionRole PrepareParentRole(Guid contact)
        {
            var connectionRoleInstance = new ConnectionRole()
            {
                Id = Guid.NewGuid(),ConnectionRoleId=contact,
                Name = "Parent",
                Category = new OptionSetValue((int)connectionrole_category.Family)
            };
            return connectionRoleInstance;
        }

        private cmc_configuration PrepareConfigurationHavingValueTrue()
        {
            return new cmc_configuration()
            {
                Id = Guid.NewGuid(),
                cmc_stoplegacycheck = true                
            };
        }

        private Connection PrepareConnectionInstanceAtoC(Entity contact, Entity account, Entity parentConnection, Entity ChildConnection)
        {
            var connectionInstance = new Connection()
            {
                ConnectionId = Guid.NewGuid(),
                Record1Id = account.ToEntityReference(),
                Record1RoleId = (new ConnectionRole() { Id = new Guid("3BB3FEDD-0333-E811-A95F-000D3A11FE32") }).ToEntityReference(),
                Record2Id = contact.ToEntityReference(),
                Record2RoleId = (new ConnectionRole() { Id = new Guid("3BB3FEDD-0333-E811-A95F-000D3A11FE32") }).ToEntityReference(),
                StateCode = ConnectionState.Active
            };
            return connectionInstance;
        }

        private Connection PrepareConnectionInstanceAtoCforCount(Entity contact, Entity account, Entity parentConnection, Entity ChildConnection)
        {
            var connectionInstance = new Connection()
            {
                ConnectionId = Guid.NewGuid(),
                Record1Id = account.ToEntityReference(),
                Record1RoleId = (new ConnectionRole() { Id = new Guid("3BB3FEDD-0333-E811-A95F-000D3A11FE32") }).ToEntityReference(),
                Record2Id = contact.ToEntityReference(),
                Record2RoleId = (new ConnectionRole() { Id = new Guid("3BB3FEDD-0333-E811-A95F-000D3A11FE32") }).ToEntityReference(),
                StateCode = ConnectionState.Active,
                ["record1objecttypecode"] = connection_record1objecttypecode.Contact

            };
            return connectionInstance;
        }

        private Connection PrepareConnectionInstanceAtoCHavingNoContactandAccount(Entity contact, Entity account, Entity parentConnection, Entity ChildConnection)
        {
            var connectionInstance = new Connection()
            {
                ConnectionId = Guid.NewGuid(),
                Record1Id = contact.ToEntityReference(),
                Record1RoleId = parentConnection.ToEntityReference(),
                Record2Id = account.ToEntityReference(),
                Record2RoleId = ChildConnection.ToEntityReference(),
                StateCode = ConnectionState.Active,
                ["record1objecttypecode"] = connection_record1objecttypecode.Contact
            };
            return connectionInstance;
        }
        private Connection PrepareConnectionInstanceAtoC_(Entity contact, Entity account, Entity parentConnection, Entity ChildConnection)
        {
            var connectionInstance = new Connection()
            {
                ConnectionId = Guid.NewGuid(),
                Record1Id =contact.ToEntityReference(), 
                Record1RoleId = (new ConnectionRole() { Id = new Guid("3BB3FEDD-0333-E811-A95F-000D3A11FE32") }).ToEntityReference(),
                Record2Id =account.ToEntityReference(),
                Record2RoleId =(new ConnectionRole() { Id = new Guid("3BB3FEDD-0333-E811-A95F-000D3A11FE32") }).ToEntityReference(),
                StateCode = ConnectionState.Active,
                ["record1objecttypecode"] = connection_record1objecttypecode.Contact
            };
            return connectionInstance;
        }
        private Connection PrepareConnectionInstanceAtoC__(Entity contact, Entity account, Entity parentConnection, Entity ChildConnection)
        {
            var connectionInstance = new Connection()
            {
                ConnectionId = Guid.NewGuid(),
                Record1Id = contact.ToEntityReference(),
                Record1RoleId = parentConnection.ToEntityReference(),
                Record2Id = account.ToEntityReference(),
                Record2RoleId = ChildConnection.ToEntityReference(),
                StateCode = ConnectionState.Active,
            };
            return connectionInstance;
        }
        private Connection PrepareConnectionInstanceCtoA(Entity contact1, Entity contact2, Entity parentConnection, Entity ChildConnection)
        {
            var connectionInstance = new Connection()
            {
                ConnectionId = Guid.NewGuid(),
                Record1Id = contact1.ToEntityReference(),
                Record1RoleId = (new ConnectionRole() { Id = new Guid("3BB3FEDD-0333-E811-A95F-000D3A11FE32") }).ToEntityReference(),
                Record2Id = contact2.ToEntityReference(),
                Record2RoleId = (new ConnectionRole() { Id = new Guid("3BB3FEDD-0333-E811-A95F-000D3A11FE32") }).ToEntityReference(),
                StateCode = ConnectionState.Active,
            };
            return connectionInstance;
        }
        private Connection PrepareConnectionInstanceCtoC(Entity contact1, Entity contact2, Entity parentConnection, Entity ChildConnection, Entity accountInstance)
        {
            var connectionInstance = new Connection()
            {
                ConnectionId = Guid.NewGuid(),
                Record1Id = contact1.ToEntityReference(),
                Record1RoleId = parentConnection.ToEntityReference(),
                Record2Id = contact2.ToEntityReference(),
                Record2RoleId = ChildConnection.ToEntityReference(),
                contact_connections1 = contact1.ToEntity<Models.Contact>(),
                contact_connections2 = contact2.ToEntity<Models.Contact>(),
                StateCode = ConnectionState.Inactive,
                account_connections1 = accountInstance.ToEntity<Account>(),
                ["record1objecttypecode"] = connection_record1objecttypecode.Contact
            };
            return connectionInstance;
        }

        private Connection PrepareConnectionInstanceCtoCHavingNoFamilyConnection(Entity contact1, Entity contact2, Entity parentConnection, Entity ChildConnection, Entity accountInstance)
        {
            var connectionInstance = new Connection()
            {
                ConnectionId = Guid.NewGuid(),
                Record1Id = contact1.ToEntityReference(),
                Record1RoleId =(new ConnectionRole() { Id = new Guid("3BB3FEDD-0333-E811-A95F-000D3A11FE32") }).ToEntityReference(), 
                Record2Id = contact2.ToEntityReference(),
                Record2RoleId = (new ConnectionRole() { Id = new Guid("3BB3FEDD-0333-E811-A95F-000D3A11FE32") }).ToEntityReference(),
                contact_connections1 = contact1.ToEntity<Models.Contact>(),
                contact_connections2 = contact2.ToEntity<Models.Contact>(),
                StateCode = ConnectionState.Inactive,
                account_connections1 = accountInstance.ToEntity<Account>(),
                ["record1objecttypecode"] = connection_record1objecttypecode.Contact
            };
            return connectionInstance;
        }
        #endregion
    }
}
