using System;
using System.Collections.Generic;
using Cmc.Core.Xrm.ServerExtension.Logging;
using Cmc.Engage.Common.Plugins.Tests.Utilities;
using Cmc.Engage.Models;
using FakeXrmEasy;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Moq;

namespace Cmc.Engage.Common.Tests.Address.Plugin
{
    [TestClass]
    public class CheckAndValidatePrimaryAddressTest : XrmUnitTestBase
    {
        [TestCategory("Plugin")]
        [TestCategory("Positive")]
        [TestMethod]
        public void CreatePrimaryAddress()
        {
            #region ARRANGE

            var contact = PreparingContact();
            var newAddress = PreparingNewAddress(contact.Id);
            var existAddress = PreparingExistAddress(contact.Id);

            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>
            {
                contact,
                newAddress,
                existAddress
            });

            var mockServiceProvider = InitializeMockService(xrmFakedContext, newAddress, Operation.Create);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);

            #endregion

            #region ACT

            //Mock the ILogger
            var mockLogger = new Mock<ILogger>();
            var mockILanguageService = new Mock<ILanguageService>();


            var mockBingMapService = InitializeBingMapMockService();

            var mockConfigurationService =
                new ConfigurationService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());


            var mockAddressService = new AddressService(mockLogger.Object, mockBingMapService.Object,
                xrmFakedContext.GetFakedOrganizationService(), mockILanguageService.Object, mockConfigurationService);
            mockAddressService.CheckAndValidatePrimaryAddress(mockExecutionContext.Object);

            #endregion

            #region ASSERT

            xrmFakedContext.Data["customeraddress"].TryGetValue(newAddress.Id, out var resultData);

            var data = resultData.Attributes["addressnumber"].ToString();
            Assert.AreEqual("1", data);

            #endregion
        }

        [TestCategory("Plugin")]
        [TestCategory("Negative")]
        [TestMethod]
        public void CreateAddressWithoutPrimary()
        {
            #region ARRANGE

            var contact = PreparingContact();
            var newAddress = PreparingNewAddressWithNoPrimary(contact.Id);


            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>
            {
                contact,
                newAddress
            });

            var mockServiceProvider = InitializeMockService(xrmFakedContext, newAddress, Operation.Create);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);

            #endregion

            #region ACT

            var mockLogger = new Mock<ILogger>();
            var mockILanguageService = new Mock<ILanguageService>();


            var mockBingMapService = InitializeBingMapMockService();

            var mockConfigurationService =
                new ConfigurationService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());


            var mockAddressService = new AddressService(mockLogger.Object, mockBingMapService.Object,
                xrmFakedContext.GetFakedOrganizationService(), mockILanguageService.Object, mockConfigurationService);
            mockAddressService.CheckAndValidatePrimaryAddress(mockExecutionContext.Object);

            #endregion

            #region ASSERT

            xrmFakedContext.Data["customeraddress"].TryGetValue(newAddress.Id, out var resultData);

            var data = resultData.Attributes["addressnumber"].ToString();
            Assert.AreNotEqual("1", data);

            #endregion
        }


        [TestCategory("Plugin")]
        [TestCategory("Positive")]
        [TestMethod]
        public void UpdateAddressToPrimary()
        {
            #region ARRANGE

            var contact = PreparingContact();
            var newAddress = PreparingNewAddress(contact.Id);
            var existAddress = PreparingExistAddress(contact.Id);
            var postImage = PreparePostImage(contact, newAddress);

            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>
            {
                contact,
                newAddress,
                existAddress,
                postImage
            });


            var mockServiceProvider = InitializeMockService(xrmFakedContext, newAddress, Operation.Update);

            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            AddPostEntityImage(mockServiceProvider, "Target", postImage);

            #endregion

            #region ACT

            var mockLogger = new Mock<ILogger>();
            var mockILanguageService = new Mock<ILanguageService>();

            var mockBingMapService = InitializeBingMapMockService();

            var mockConfigurationService =
                new ConfigurationService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());


            var mockAddressService = new AddressService(mockLogger.Object, mockBingMapService.Object,
                xrmFakedContext.GetFakedOrganizationService(), mockILanguageService.Object, mockConfigurationService);
            mockAddressService.CheckAndValidatePrimaryAddress(mockExecutionContext.Object);

            #endregion

            #region ASSERT

            xrmFakedContext.Data["customeraddress"].TryGetValue(newAddress.Id, out var resultData);

            var data = resultData.Attributes["addressnumber"].ToString();
            Assert.AreEqual("1", data);

            #endregion
        }

        [TestCategory("Plugin")]
        [TestCategory("Negative")]
        [TestMethod]
        public void UpdateNonPrimaryAddress()
        {
            #region ARRANGE

            var contact = PreparingContact();
            var newAddress = PreparingNewAddressWithNoPrimary(contact.Id);


            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>
            {
                contact,
                newAddress
            });


            var mockServiceProvider = InitializeMockService(xrmFakedContext, newAddress, Operation.Update);

            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);

            #endregion

            #region ACT

            var mockLogger = new Mock<ILogger>();
            var mockILanguageService = new Mock<ILanguageService>();

            var mockBingMapService = InitializeBingMapMockService();

            var mockConfigurationService =
                new ConfigurationService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());


            var mockAddressService = new AddressService(mockLogger.Object, mockBingMapService.Object,
                xrmFakedContext.GetFakedOrganizationService(), mockILanguageService.Object, mockConfigurationService);
            mockAddressService.CheckAndValidatePrimaryAddress(mockExecutionContext.Object);

            #endregion

            #region ASSERT

            xrmFakedContext.Data["customeraddress"].TryGetValue(newAddress.Id, out var resultData);

            var data = resultData.Attributes["addressnumber"].ToString();
            Assert.AreNotEqual("1", data);

            #endregion
        }

        [TestCategory("Plugin")]
        [TestCategory("Negative")]
        [TestMethod]
        public void CreateAddressWithoutParentId()
        {
            #region ARRANGE

            var contact = PreparingContact();
            var newAddress = PreparingNewAddressWithoutParentId();


            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>
            {
                contact,
                newAddress
            });

            var mockServiceProvider = InitializeMockService(xrmFakedContext, newAddress, Operation.Create);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);

            #endregion

            #region ACT

            var mockLogger = new Mock<ILogger>();
            var mockILanguageService = new Mock<ILanguageService>();


            var mockBingMapService = InitializeBingMapMockService();

            var mockConfigurationService =
                new ConfigurationService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());


            var mockAddressService = new AddressService(mockLogger.Object, mockBingMapService.Object,
                xrmFakedContext.GetFakedOrganizationService(), mockILanguageService.Object, mockConfigurationService);
            mockAddressService.CheckAndValidatePrimaryAddress(mockExecutionContext.Object);

            #endregion

            #region ASSERT

            xrmFakedContext.Data["customeraddress"].TryGetValue(newAddress.Id, out var resultData);

            var data = resultData.Attributes["addressnumber"].ToString();
            Assert.AreNotEqual("1", data);

            #endregion
        }

        [TestCategory("Plugin")]
        [TestCategory("Negative")]
        [TestMethod]
        public void UpdateAddressWithoutParentId()
        {
            #region ARRANGE

            var contact = PreparingContact();
            var newAddress = PreparingNewAddress(contact.Id);
            var postImage = PreparePostImageWithoutParentId(newAddress);

            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>
            {
                contact,
                newAddress,
                postImage
            });


            var mockServiceProvider = InitializeMockService(xrmFakedContext, newAddress, Operation.Update);

            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            AddPostEntityImage(mockServiceProvider, "Target", postImage);

            #endregion

            #region ACT

            var mockLogger = new Mock<ILogger>();
            var mockILanguageService = new Mock<ILanguageService>();

            var mockBingMapService = InitializeBingMapMockService();

            var mockConfigurationService =
                new ConfigurationService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());


            var mockAddressService = new AddressService(mockLogger.Object, mockBingMapService.Object,
                xrmFakedContext.GetFakedOrganizationService(), mockILanguageService.Object, mockConfigurationService);
            mockAddressService.CheckAndValidatePrimaryAddress(mockExecutionContext.Object);

            #endregion

            #region ASSERT

            xrmFakedContext.Data["customeraddress"].TryGetValue(newAddress.Id, out var resultData);

            var data = resultData.Attributes["addressnumber"].ToString();
            Assert.AreNotEqual("1", data);

            #endregion
        }

        #region DATA_Preparation

        private Entity PreparingContact()
        {
            return new Models.Contact
            {
                ContactId = Guid.NewGuid(),
                FirstName = "Ankur k"
            };
        }

        private Entity PreparingNewAddress(Guid contactInstance)
        {
            return new CustomerAddress
            {
                ParentId = new EntityReference("parentid", contactInstance),
                Line2 = "India",
                Line1 = "Chenai",
                mshied_MailType = mshied_customeraddress_mshied_mailtype.Primary,
                CustomerAddressId = Guid.NewGuid(),
                AddressNumber = 2
            };
        }

        private Entity PreparingExistAddress(Guid contactInstance)
        {
            return new CustomerAddress
            {
                ParentId = new EntityReference("parentid", contactInstance),
                Line2 = "India",
                Line1 = "Bangalore",
                mshied_MailType = null,
                CustomerAddressId = Guid.NewGuid(),
                AddressNumber = 1
            };
        }

        private Entity PreparePostImage(Entity contactInstance, Entity addressInstance)
        {
            return new CustomerAddress
            {
                CustomerAddressId = addressInstance.Id,
                AddressNumber = 2,
                ParentId = contactInstance.ToEntityReference()
            };
        }

        private Entity PreparePostImageWithoutParentId(Entity addressInstance)
        {
            return new CustomerAddress
            {
                CustomerAddressId = addressInstance.Id,
                AddressNumber = 2
            };
        }

        private Entity PreparingNewAddressWithNoPrimary(Guid contactInstance)
        {
            return new CustomerAddress
            {
                ParentId = new EntityReference("parentid", contactInstance),
                Line2 = "India",
                Line1 = "Chenai",
                mshied_MailType = null,
                CustomerAddressId = Guid.NewGuid(),
                AddressNumber = 3
            };
        }

        private Entity PreparingNewAddressWithoutParentId()
        {
            return new CustomerAddress
            {
                Line2 = "India",
                Line1 = "Chenai",
                mshied_MailType = mshied_customeraddress_mshied_mailtype.Primary,
                CustomerAddressId = Guid.NewGuid(),
                AddressNumber = 2
            };
        }

        #endregion
    }
}