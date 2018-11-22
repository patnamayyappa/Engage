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
    public class SetAddressStateorCountryTest : XrmUnitTestBase
    {
        [TestCategory("Plugin"), TestCategory("Positive")]
        [TestMethod]
        public void SetContactStateorCountry_SetValue()
        {
            #region Arrange

            var addressInstance = AddressInstance();
            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>()
            {
                addressInstance
            });

            #endregion Arrange
            var mockServiceProvider = InitializeMockService(xrmFakedContext, addressInstance, Operation.Create);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);

            #region Act
            
            var mockLogger = new Mock<ILogger>();
            var mockILanguageService = new Mock<ILanguageService>();
            var mockBingMapService = InitializeBingMapMockService();
            var mockConfigurationService = new ConfigurationService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
            var addressService = new AddressService(mockLogger.Object, mockBingMapService.Object, xrmFakedContext.GetFakedOrganizationService(), mockILanguageService.Object, mockConfigurationService);
            addressService.SetAddressStateorCountry(mockExecutionContext.Object);
            //xrmFakedContext.GetFakedOrganizationService().Update(new Entity("CustomerAddress", addressInstance.Id));
            #endregion Act

            #region Assert
            var address = xrmFakedContext.GetFakedOrganizationService()
                .Retrieve("customeraddress", addressInstance.Id, new ColumnSet(true));

            Assert.IsNotNull(address.Attributes["cmc_country"]);
            #endregion Assert
        }

        [TestCategory("Plugin"), TestCategory("Negative")]
        [TestMethod]
        public void SetContactStateorCountry_NullValue()
        {
            #region Arrange

            var addressInstance = new CustomerAddress(){Id = Guid.NewGuid(),};
            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>()
            {
                addressInstance
            });

            #endregion Arrange
            var mockServiceProvider = InitializeMockService(xrmFakedContext, addressInstance, Operation.Create);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);

            #region Act

            var mockLogger = new Mock<ILogger>();
            var mockILanguageService = new Mock<ILanguageService>();
            var mockBingMapService = InitializeBingMapMockService();
            var mockConfigurationService = new ConfigurationService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
            var addressService = new AddressService(mockLogger.Object, mockBingMapService.Object, xrmFakedContext.GetFakedOrganizationService(), mockILanguageService.Object, mockConfigurationService);
            addressService.SetAddressStateorCountry(mockExecutionContext.Object);
            //xrmFakedContext.GetFakedOrganizationService().Update(new Entity("CustomerAddress", addressInstance.Id));
            #endregion Act

            #region Assert
            var address = xrmFakedContext.GetFakedOrganizationService()
                .Retrieve("customeraddress", addressInstance.Id, new ColumnSet(true));

            Assert.IsNull(address.GetAttributeValue<string>("Country"));

            #endregion Assert
        }

        private CustomerAddress AddressInstance()
        {
            return new CustomerAddress()
            {
                Id = Guid.NewGuid(),
                cmc_country = cmc_customeraddress_cmc_country.Afghanistan,
                cmc_stateprovince = cmc_customeraddress_cmc_stateprovince.Alabama,
                Country = ""

            };
        }
    }
}
