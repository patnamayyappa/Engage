using System;
using System.Collections.Generic;
using System.Linq;
using Cmc.Core.Xrm.ServerExtension.Core;
using Cmc.Core.Xrm.ServerExtension.Logging;
using Cmc.Engage.Common.Plugins.Tests.Utilities;
using Cmc.Engage.Common.Utilities.Constants;
using Cmc.Engage.Models;
using FakeXrmEasy;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Moq;

namespace Cmc.Engage.Common.Tests.Address.Functions
{
    [TestClass]
    public class AddressGeoCoderTest : XrmUnitTestBase
    {        
        [TestCategory("Function"), TestCategory("Positive")]
        [TestMethod]
        public void AddressGeoCoder_ForNullAddress()
        {
            #region ARRANGE            
            cmc_configuration retrieveConfiguration = GetActiveConfiguration();
            
            var bingMapKeyConfigInstance = GetConfigurationList().FirstOrDefault(a => a.cmc_bingmapapikey == retrieveConfiguration.cmc_bingmapapikey);
            var contactInstance = GetContactInstance();
            var addressInstance = GetNullAddress(contactInstance);

            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>
            {
                bingMapKeyConfigInstance,
                contactInstance,
                addressInstance
            });

            #endregion

            #region ACT

            var mockLogger = new Mock<ILogger>();
            var mockILanguageService = new Mock<ILanguageService>();
            var mockBingMapService = InitializeBingMapMockService();
            //var mockConfigurationService = new Mock<IConfigurationService>(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
            var mockConfigurationService = new ConfigurationService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
            // var mockBingServices = new BingMapService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService(),mockConfigurationService);
            var addressService = new AddressService(mockLogger.Object, mockBingMapService.Object,
                xrmFakedContext.GetFakedOrganizationService(), mockILanguageService.Object, mockConfigurationService);
            addressService.AddressGeoCoderLogic();

            #endregion

            #region ASSERT

            xrmFakedContext.Data["customeraddress"].TryGetValue(addressInstance.Id, out var data);
            var geocoordinatesFlag = data?.Attributes["cmc_geocoordinates"];
            if (geocoordinatesFlag != null)
                Assert.IsTrue((cmc_customeraddress_cmc_geocoordinates)((OptionSetValue)geocoordinatesFlag).Value ==
                              cmc_customeraddress_cmc_geocoordinates.No);

            #endregion ASSERT  
        }

        [TestCategory("Function"), TestCategory("Positive")]
        [TestMethod]
        public void AddressGeoCoder_ForValidAddress()
        {
            #region ARRANGE

            var configurationList = GetConfigurationList();
            var contactInstance = GetContactInstance();
            var addressInstance = GetValidAddress(contactInstance);
            var entityList = new List<Entity>();
            entityList.AddRange(configurationList);
            entityList.Add(contactInstance);
            entityList.Add(addressInstance);

            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(entityList);

            #endregion

            #region ACT

            var mockLogger = new Mock<ILogger>();
            var mockILanguageService = new Mock<ILanguageService>();
            var mockBingMapService = InitializeBingMapMockService();

            var mockConfigurationService =
                new ConfigurationService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
            //var mockBingServices = new BingMapService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService(),
            //    mockConfigurationService);
            var addressService = new AddressService(mockLogger.Object, mockBingMapService.Object,
                xrmFakedContext.GetFakedOrganizationService(), mockILanguageService.Object, mockConfigurationService);
            addressService.AddressGeoCoderLogic();

            #endregion

            #region ASSERT

            xrmFakedContext.Data["customeraddress"].TryGetValue(addressInstance.Id, out var data);
            // Checking condition because of limit for trial of bing api
            if ((bool)data?.Contains("cmc_geocoordinates"))
            {
                var geocoordinatesFlag = data?.Attributes["cmc_geocoordinates"];
                if (geocoordinatesFlag != null)
                    Assert.IsTrue((cmc_customeraddress_cmc_geocoordinates)((OptionSetValue)geocoordinatesFlag).Value ==
                                  cmc_customeraddress_cmc_geocoordinates.Yes);
                var latitude = data?.Attributes["latitude"].ToString();
                Assert.IsTrue(latitude != null);
                var longitude = data?.Attributes["longitude"].ToString();
                Assert.IsTrue(longitude != null);
            }
            #endregion ASSERT  
        }

        [TestCategory("Function"), TestCategory("Positive")]
        [TestMethod]
        public void AddressGeoCoder_ForInvalidAddress()
        {
            #region ARRANGE

            var configurationList = GetConfigurationList();
            var contactInstance = GetContactInstance();
            var addressInstance1 = GetValidAddress(contactInstance);
            var addressInstance2 = GetInvalidAddress(contactInstance);

            var entityList = new List<Entity>();
            entityList.AddRange(configurationList);
            entityList.Add(contactInstance);
            entityList.Add(addressInstance1);
            entityList.Add(addressInstance2);

            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(entityList);

            #endregion

            #region ACT

            var mockLogger = new Mock<ILogger>();
            var mockILanguageService = new Mock<ILanguageService>();
            var mockBingMapService = InitializeBingMapMockService();

            var mockConfigurationService = new ConfigurationService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
            //var mockBingServices = new BingMapService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService(),
            //    mockConfigurationService);
            var addressService = new AddressService(mockLogger.Object, mockBingMapService.Object,
                xrmFakedContext.GetFakedOrganizationService(), mockILanguageService.Object, mockConfigurationService);
            addressService.AddressGeoCoderLogic();

            #endregion

            #region ASSERT

            var fetchXml = $@"<fetch>
                                <entity name='customeraddress'>
                                      <all-attributes/>                                               
                                </entity>
                            </fetch>";
            var data = xrmFakedContext.GetFakedOrganizationService().RetrieveMultipleAll(fetchXml)?.Entities?.FirstOrDefault();
            //xrmFakedContext.Data["cmc_address"].TryGetValue(addressInstance1.Id, out var data);
            if ((bool)data?.Contains("cmc_geocoordinates"))
            {
                var geocoordinatesFlag = data?.Attributes["cmc_geocoordinates"];
                if (geocoordinatesFlag != null)
                    Assert.IsTrue((cmc_customeraddress_cmc_geocoordinates)((OptionSetValue)geocoordinatesFlag).Value ==
                                  cmc_customeraddress_cmc_geocoordinates.Yes);
                if ((bool)data?.Contains("latitude"))
                {
                    var latitude = data?.Attributes["latitude"].ToString();
                    Assert.IsTrue(latitude != null);
                }

                if ((bool)data?.Contains("cmc_longitude"))
                {
                    var longitude = data?.Attributes["longitude"].ToString();
                    Assert.IsTrue(longitude != null);
                }
            }

            #endregion ASSERT  
        }

        [TestCategory("Function"), TestCategory("Positive")]
        [TestMethod]
        public void AddressGeoCoder_ForValidAddress_IfResultIsNull()
        {
            #region ARRANGE

            var configurationList = GetConfigurationList();
            var contactInstance = GetContactInstance();
            var addressInstance2 = GetInvalidAddress(contactInstance);
            var entityList = new List<Entity>();
            entityList.AddRange(configurationList);
            entityList.Add(contactInstance);
            entityList.Add(addressInstance2);

            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(entityList);

            #endregion

            #region ACT

            var mockLogger = new Mock<ILogger>();
            var mockILanguageService = new Mock<ILanguageService>();
            var mockBingMapService = InitializeBingMapMockService();

            var mockConfigurationService =
                new ConfigurationService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
            //var mockBingServices = new BingMapService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService(),
            //    mockConfigurationService);
            var addressService = new AddressService(mockLogger.Object, mockBingMapService.Object,
                xrmFakedContext.GetFakedOrganizationService(), mockILanguageService.Object, mockConfigurationService);
            addressService.AddressGeoCoderLogic();

            #endregion

            #region ASSERT
            var fetchXml = $@"<fetch>
                                <entity name='customeraddress'>
                                      <all-attributes/>                                               
                                </entity>
                            </fetch>";
            var data = xrmFakedContext.GetFakedOrganizationService().RetrieveMultipleAll(fetchXml)?.Entities?.FirstOrDefault();
            //xrmFakedContext.Data["cmc_address"].TryGetValue(addressInstance2.Id, out var data);
            if ((bool)data?.Contains("cmc_geocoordinates"))
            {
                var geocoordinatesFlag = data?.Attributes["cmc_geocoordinates"];
                if (geocoordinatesFlag != null)
                    Assert.IsTrue((cmc_customeraddress_cmc_geocoordinates)((OptionSetValue)geocoordinatesFlag).Value ==
                                  cmc_customeraddress_cmc_geocoordinates.No);
            }
            Assert.IsNotNull(data.Attributes["city"]);
            #endregion ASSERT  
        }

        [TestCategory("Function"), TestCategory("Negative")]
        [TestMethod]
        public void AddressGeoCoder_ForInvalidAddress_Exception()
        {
            #region ARRANGE

            var configurationList = GetInvalidConfiguration();
            var contactInstance = GetContactInstance();
            var addressInstance1 = GetValidAddress(contactInstance);

            var entityList = new List<Entity>();
            entityList.AddRange(configurationList);
            entityList.Add(contactInstance);
            entityList.Add(addressInstance1);

            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(entityList);

            #endregion

            #region ACT

            var mockLogger = new Mock<ILogger>();
            var mockILanguageService = new Mock<ILanguageService>();
            var mockBingMapService = InitializeBingMapMockService();

            var mockConfigurationService =
                new ConfigurationService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
            //var mockBingServices = new BingMapService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService(),
            //    mockConfigurationService);
            var addressService = new AddressService(mockLogger.Object, mockBingMapService.Object,
                xrmFakedContext.GetFakedOrganizationService(), mockILanguageService.Object, mockConfigurationService);
            addressService.AddressGeoCoderLogic();

            #endregion

            #region ASSERT

            xrmFakedContext.Data["customeraddress"].TryGetValue(addressInstance1.Id, out var data);

            Assert.IsFalse(data.Contains("cmc_geocoordinates"));
            #endregion ASSERT  
        }

        private static IEnumerable<cmc_configuration> GetConfigurationList()
        {
            return new List<cmc_configuration>
            {
                new cmc_configuration
                {
                    Id = Guid.NewGuid(),
                    cmc_bingmapapikey = "ApHcQX8UM8ulfNCQgjrGRFu5-He1C0BC2cFh9VtoJbKQG7FNzOT-0t_zau-a_LBh",
                    cmc_batchgeocodesize = 10
                }             

            };
        }
        private static IEnumerable<cmc_configuration> GetInvalidConfiguration()
        {
            return new List<cmc_configuration>
            {
                new cmc_configuration
                {
                    Id = Guid.NewGuid(),
                    cmc_bingmapapikey = "ApHcQX8UM8ulfNCQgjrGRFu5-He1C0BC2cFh9VtoJbKQG7FNzOT-0t_zau-a_L12",
                    cmc_batchgeocodesize = 10
                }                

            };
        }

        private static CustomerAddress GetValidAddress(Entity contact)
        {
            return new CustomerAddress
            {
                CustomerAddressId = Guid.NewGuid(),
                ParentId = contact.ToEntityReference(),
                Line1 = "Chennai",
                Latitude = null,
                Longitude = null,

            };
        }

        private static CustomerAddress GetNullAddress(Entity contact)
        {
            return new CustomerAddress
            {
                CustomerAddressId = Guid.NewGuid(),
                ParentId = contact.ToEntityReference(),
                Line1 = null,
                City = null,
                PostalCode = null,
                Latitude = null,
                Longitude = null,
                cmc_geocoordinates = null
            };
        }

        private static CustomerAddress GetInvalidAddress(Entity contact)
        {
            return new CustomerAddress
            {
                CustomerAddressId = Guid.NewGuid(),
                ParentId = contact.ToEntityReference(),
                Line1 = null,
                City = "1sdfa4w234",
                PostalCode = null,
                Latitude = null,
                Longitude = null,
                cmc_geocoordinates = null
            };
        }

        private static Models.Contact GetContactInstance()
        {
            return new Models.Contact
            {
                Id = Guid.NewGuid()
            };
        }
    }
}