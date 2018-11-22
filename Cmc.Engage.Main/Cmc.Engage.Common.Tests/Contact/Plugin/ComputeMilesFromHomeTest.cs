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
    public class ComputeMilesFromHomeTest : XrmUnitTestBase
    {
        [TestCategory("Plugin"), TestCategory("Positive")]
        [TestMethod]
        public void ComputeMilesFromHome_UpdatetheAccount_CurrentAndPreviousAssociatedAccount_AreNotEqual()
        {
            //Need to mock all the dependency Injections passed to the services constructor
            #region ARRANGE
            //Create 2 accounts, where one account is the initial associated Campus 
            //with the contact and the other is the updated Campus of the Contact
            var previousAssociatedAccount = PrepareAccountInstance1();
            var currentAssociatedAccount = PrepareAccountInstance2();

            var contactInstance = PrepareContactInstance(previousAssociatedAccount.Id);
            var contactPreImage = PrepareContactPreImage(contactInstance, previousAssociatedAccount.Id);
            var contactPostImage = PrepareContactPostImage(contactInstance, currentAssociatedAccount.Id);

            var bingMapKeyConfigInstance = PrepareBingMapKeyConfigInstance();

            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>()
            {
                previousAssociatedAccount,
                currentAssociatedAccount,
                contactInstance,
                bingMapKeyConfigInstance
            });

            //Initialize the Mock Service
            var mockServiceProvider = InitializeMockService(xrmFakedContext, contactPreImage, Operation.Update);

            //Add the Pre Entity images and Post Entity Images            
            AddPreEntityImage(mockServiceProvider, "PreImage", contactPreImage);
            AddPostEntityImage(mockServiceProvider, "PostImage", contactPostImage);

            //Fetch the Mock Execution Context
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            //Mock External Dependecies

            #endregion ARRANGE

            #region ACT
            //Instantiate the Class with the mocked dependencies.
            //Need to mock all the dependency Injections passed to the services constructor
            //Mock the ILogger
            var mockLogger = new Mock<ILogger>();
            var mockILanguageService = new Mock<ILanguageService>();
            //Mock the IBingMap
            var mockBingMapService = InitializeBingMapMockService();

            var mockConfigurationService = new ConfigurationService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
            //var mockBingServices = new BingMapService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService(), mockConfigurationService);
            var contactService = new ContactService(mockLogger.Object, mockBingMapService.Object, xrmFakedContext.GetFakedOrganizationService(), mockConfigurationService, mockILanguageService.Object);
            contactService.ComputeMilesFromHome(mockExecutionContext.Object);
            #endregion ACT

            #region ASSERT
            //Get back the Context Data after business logic is performed
            Entity preContactData = new Entity("contact");
            xrmFakedContext.Data["contact"].TryGetValue(contactInstance.Id, out preContactData);

            //Assert if the business logic performed is correct.
            var milesFromCampus = (preContactData.Attributes["cmc_milesfromcampus"]).ToString();
            var parentCustomerId = (preContactData.Attributes["parentcustomerid"]).ToString();
            Assert.AreNotEqual(parentCustomerId, previousAssociatedAccount.Id);
            #endregion ASSERT      
        }

        private cmc_configuration PrepareBingMapKeyConfigInstance()
        {
            var bingMapKeyConfigInstance = new cmc_configuration()
            {
                Id = Guid.NewGuid(),
                cmc_bingmapapikey = "ApHcQX8UM8ulfNCQgjrGRFu5-He1C0BC2cFh9VtoJbKQG7FNzOT-0t_zau-a_LBh"
                //cmc_configurationname = "BingMapApiKey",
                //cmc_Value = "ApHcQX8UM8ulfNCQgjrGRFu5-He1C0BC2cFh9VtoJbKQG7FNzOT-0t_zau-a_LBh"
            };
            return bingMapKeyConfigInstance;
        }

        private Entity PrepareAccountInstance1()
        {
            var accountInstance1 = new Entity("account", Guid.NewGuid())
            {
                ["address1_latitude"] = 12.978600,
                ["address1_longitude"] = 77.661100,
            };
            return accountInstance1;
        }

        private Entity PrepareAccountInstance2()
        {
            //Creating an Account2 to change the contact campus.
            var accountInstance2 = new Entity("account", Guid.NewGuid())
            {
                ["address1_latitude"] = 12.978600,
                ["address1_longitude"] = 87.661100,
            };
            return accountInstance2;
        }

        private Entity PrepareContactInstance(Guid accountInstance1)
        {
            var contactInstance = new Entity("contact", Guid.NewGuid())
            {
                ["parentcustomerid"] = accountInstance1,
                ["address1_latitude"] = 12.978600,
                ["address1_longitude"] = 87.661100,
            };
            return contactInstance;
        }

        private Models.Contact PrepareContactPreImage(Entity contactInstance, Guid accountInstance1)
        {
            var targetPreImage = new Models.Contact()
            // var asd=new contact()
            {
                ContactId = contactInstance.Id,
                Address1_Latitude = contactInstance.GetAttributeValue<double>("address1_latitude"),
                Address1_Longitude = contactInstance.GetAttributeValue<double>("address1_longitude"),
                cmc_milesfromcampus = null,
                ParentCustomerId = new EntityReference("account", accountInstance1),
            };
            return targetPreImage;
        }

        private Models.Contact PrepareContactPostImage(Entity contactInstance, Guid accountInstance2)
        {
            var targetPostImage = new Models.Contact()
            {
                ContactId = contactInstance.Id,
                Address1_Latitude = contactInstance.GetAttributeValue<double>("address1_latitude"),
                Address1_Longitude = contactInstance.GetAttributeValue<double>("address1_longitude"),
                ParentCustomerId = new EntityReference("account", accountInstance2),
            };
            return targetPostImage;
        }

    }
}
