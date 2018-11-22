using System;
using System.Collections.Generic;
using Cmc.Core.Xrm.ServerExtension.Logging;
using Cmc.Engage.Common.Plugins.Tests.Utilities;
using Cmc.Engage.Models;
using FakeXrmEasy;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Moq;

namespace Cmc.Engage.Common.Tests.Address.Plugin
{
    [TestClass]
    public class GeocodeAddressTest : XrmUnitTestBase
    {
        [TestCategory("Plugin")]
        [TestCategory("Positive")]
        [TestMethod]
        public void GeocodeAddress_CreateAddress()
        {
            #region ARRANGE

            var bingMapKeyConfigInstance = PrepareBingMapKeyConfigInstance();
            var contactInstance = PrepareContactInstance();
            var addressInstance = AddressInstance(contactInstance);
            var xrmFakedContext = new XrmFakedContext();

            xrmFakedContext.Initialize(new List<Entity>
            {
                contactInstance,
                addressInstance,
                bingMapKeyConfigInstance
            });


            var mockServiceProvider = InitializeMockService(xrmFakedContext, addressInstance, Operation.Create);

            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);

            #endregion

            #region ACT

            var mockLogger = new Mock<ILogger>();

            var mockILanguageService = new Mock<ILanguageService>();


            var mockBingMapService = InitializeBingMapMockService();

            var mockConfigurationService =
                new ConfigurationService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());

            var contactService = new AddressService(mockLogger.Object, mockBingMapService.Object,
                xrmFakedContext.GetFakedOrganizationService(), mockILanguageService.Object, mockConfigurationService);
            contactService.GeocodeAddress(mockExecutionContext.Object);

            #endregion


            #region ASSERT

            var address = xrmFakedContext.GetFakedOrganizationService()
                .Retrieve("customeraddress", addressInstance.Id, new ColumnSet(true));

            Assert.AreNotSame(address.Attributes["latitude"], "0");

            #endregion
        }

        [TestCategory("Plugin")]
        [TestCategory("Positive")]
        [TestMethod]
        public void GeocodeAddress_UpdateAddress()
        {
            #region ARRANGE

            var bingMapKeyConfigInstance = PrepareBingMapKeyConfigInstance();
            var contactInstance = PrepareContactInstance();
            var successNetworkInstance = PrepareSuccessNetwork();
            var previousAddressInstance = AddressInstance(contactInstance);

            var postAddressInstance = PostAddressInstance(contactInstance);

            var preImageEntity = PrepareImage(previousAddressInstance);
            var postImageEntity = PrepareImage(postAddressInstance);

            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>
            {
                bingMapKeyConfigInstance,
                successNetworkInstance,
                contactInstance,
                previousAddressInstance,
                postAddressInstance
            });


            var mockServiceProvider = InitializeMockService(xrmFakedContext, previousAddressInstance, Operation.Update);

            AddPreEntityImage(mockServiceProvider, "Target", preImageEntity);
            AddPostEntityImage(mockServiceProvider, "PostImage", postImageEntity);

            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);

            #endregion

            #region ACT

            var mockLogger = new Mock<ILogger>();

            var mockILanguageService = new Mock<ILanguageService>();


            var mockBingMapService = InitializeBingMapMockService();

            var mockConfigurationService =
                new ConfigurationService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());


            var contactService = new AddressService(mockLogger.Object, mockBingMapService.Object,
                xrmFakedContext.GetFakedOrganizationService(), mockILanguageService.Object, mockConfigurationService);


            contactService.GeocodeAddress(mockExecutionContext.Object);

            #endregion

            #region ASSERT

            var address = xrmFakedContext.GetFakedOrganizationService()
                .Retrieve("customeraddress", previousAddressInstance.Id, new ColumnSet(true));

            Assert.AreNotSame(address.Attributes["latitude"], "0");

            #endregion
        }

        [TestCategory("Plugin")]
        [TestCategory("Negative")]
        [TestMethod]
        public void GeocodeAddress_CreateAddress_BingMapKeyIsNull()
        {
            #region ARRANGE

            var bingMapKeyConfigInstance = PrepareBingMapKeyConfigInstance();
            bingMapKeyConfigInstance["cmc_configurationname"] = "";
            bingMapKeyConfigInstance["cmc_value"] = "";


            var contactInstance = PrepareContactInstance();
            var addressInstance = AddressInstance(contactInstance);

            var xrmFakedContext = new XrmFakedContext();


            xrmFakedContext.Initialize(new List<Entity>
            {
                bingMapKeyConfigInstance,
                contactInstance,
                addressInstance
            });


            var mockServiceProvider = InitializeMockService(xrmFakedContext, addressInstance, Operation.Create);

            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);

            #endregion

            #region ACT

            var mockLogger = new Mock<ILogger>();

            var mockILanguageService = new Mock<ILanguageService>();


            var mockBingMapService = InitializeBingMapMockService();

            var mockConfigurationService =
                new ConfigurationService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());


            var contactService = new AddressService(mockLogger.Object, mockBingMapService.Object,
                xrmFakedContext.GetFakedOrganizationService(), mockILanguageService.Object, mockConfigurationService);
            contactService.GeocodeAddress(mockExecutionContext.Object);

            #endregion


            #region ASSERT

            var address = xrmFakedContext.GetFakedOrganizationService()
                .Retrieve("cmc_configuration", bingMapKeyConfigInstance.Id, new ColumnSet(true));

            Assert.AreEqual(address.Attributes["cmc_value"], "");

            #endregion
        }

        [TestCategory("Plugin")]
        [TestCategory("Negative")]
        [TestMethod]
        public void GeocodeAddress_CreateAddress_CmcAddressIsNull()
        {
            #region ARRANGE

            var contactInstance = PrepareContactInstance();
            var addressInstance = AddressInstance1();

            var xrmFakedContext = new XrmFakedContext();


            xrmFakedContext.Initialize(new List<Entity>
            {
                contactInstance,
                addressInstance
            });


            var mockServiceProvider = InitializeMockService(xrmFakedContext, addressInstance, Operation.Create);

            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);

            #endregion

            #region ACT

            var mockLogger = new Mock<ILogger>();

            var mockILanguageService = new Mock<ILanguageService>();

            var mockBingMapService = InitializeBingMapMockService();

            var mockConfigurationService =
                new ConfigurationService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());


            var contactService = new AddressService(mockLogger.Object, mockBingMapService.Object,
                xrmFakedContext.GetFakedOrganizationService(), mockILanguageService.Object, mockConfigurationService);
            contactService.GeocodeAddress(mockExecutionContext.Object);

            #endregion


            #region ASSERT

            var address = xrmFakedContext.GetFakedOrganizationService()
                .Retrieve("customeraddress", addressInstance.Id, new ColumnSet(true));

            Assert.IsFalse(address.Attributes.Keys.Contains("parentid"));

            #endregion
        }

        #region Data Preparation

        private Entity PrepareBingMapKeyConfigInstance()
        {
            var bingMapKeyConfigInstance = new Entity("cmc_configuration", Guid.NewGuid())
            {
                ["cmc_bingmapapikey"] = "ApHcQX8UM8ulfNCQgjrGRFu5-He1C0BC2cFh9VtoJbKQG7FNzOT-0t_zau-a_LBh"
            };
            return bingMapKeyConfigInstance;
        }

        private Entity PostAddressInstance(Entity contact)
        {
            return new CustomerAddress
            {
                ParentId = contact.ToEntityReference(),
                County = "India",
                StateOrProvince = "Odisha",
                Line1 = "TEst Street",
                City = "Puri",
                PostalCode = "560037",
                Latitude = 62.13,
                Longitude = 45.21,
                CustomerAddressId = Guid.NewGuid()
            };
        }

        private Entity AddressInstance(Entity contact)
        {
            return new CustomerAddress
            {
                ParentId = contact.ToEntityReference(),
                County = "India",
                StateOrProvince = "Odisha",
                Line1 = "TEst Street",
                City = "Puri",
                PostalCode = "560037",
                Latitude = 62.13,
                Longitude = 45.21,
                CustomerAddressId = Guid.NewGuid()
            };
        }

        private Entity AddressInstance1()
        {
            return new CustomerAddress
            {
                County = "India",
                StateOrProvince = "Odisha",
                Line1 = "TEst Street",
                City = "Puri",
                PostalCode = "560037",
                Latitude = 62.13,
                Longitude = 45.21,
                CustomerAddressId = Guid.NewGuid()
            };
        }


        private Entity PrepareImage(Entity addressEntity)
        {
            return new CustomerAddress
            {
                ParentId = addressEntity.GetAttributeValue<EntityReference>("contact"),
                County = "India",
                StateOrProvince = "Odisha",
                Line1 = "TEst Street",
                City = "Puri",
                PostalCode = "560037",
                Latitude = 62.13,
                Longitude = 45.21,
                CustomerAddressId = Guid.NewGuid()
            };
        }

        private Entity PrepareContactInstance()
        {
            var contact = new Entity("contact", Guid.NewGuid());
            return contact;
        }

        private Entity PrepareSuccessNetwork()
        {
            return new cmc_successnetwork
            {
                cmc_successnetworkId = Guid.NewGuid()
            };
        }

        #endregion
    }
}