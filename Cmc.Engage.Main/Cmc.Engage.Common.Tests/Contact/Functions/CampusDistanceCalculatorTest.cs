using System;
using System.Collections.Generic;
using Cmc.Core.Xrm.ServerExtension.Logging;
using Cmc.Engage.Common.Plugins.Tests.Utilities;
using Cmc.Engage.Models;
using FakeXrmEasy;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Moq;

namespace Cmc.Engage.Common.Tests.Contact.Functions
{
    [TestClass]
    public class CampusDistanceCalculatorTest : XrmUnitTestBase
    {
        [TestMethod]
        [TestCategory("Function"), TestCategory("Positive")]
        public void CampusDistanceCalculator_UpdateDistance_ForDifferent_LatitudeAndLongitude()
        {
            #region ARRANGE

            var currentAssociatedAccount = GetCurrentAssociatedAccount();
            var contactInstance = PrepareContactInstance(currentAssociatedAccount);


            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>
            {
                currentAssociatedAccount,
                contactInstance
            });

            #endregion

            #region ACT

            var mockLogger = new Mock<ILogger>();
            var mockILanguageService = new Mock<ILanguageService>();
            var mockConfigurationService =
                new ConfigurationService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
            var mockContactService = new ContactService(mockLogger.Object, null,
                xrmFakedContext.GetFakedOrganizationService(), mockConfigurationService,mockILanguageService.Object);
            mockContactService.CampusDistanceCalculatorLogic();

            #endregion

            #region ASSERT

            xrmFakedContext.Data["contact"].TryGetValue(contactInstance.Id, out var data);
            var updatedMilesFromCampus = data?.Attributes["cmc_milesfromcampus"].ToString();
            Assert.IsNotNull(updatedMilesFromCampus);
            Assert.IsTrue(updatedMilesFromCampus != "0");

            #endregion ASSERT  
        }
        [TestCategory("Function"), TestCategory("Positive")]
        [TestMethod]
        public void CampusDistanceCalculator_UpdateDistanceToZero_SameDifferent_LatitudeAndLongitude()
        {
            #region ARRANGE

            var currentAssociatedAccount = GetCurrentAssociatedAccountWithSameLatitudeAndLongitude();
            var contactInstance = PrepareContactInstance(currentAssociatedAccount);


            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>
            {
                currentAssociatedAccount,
                contactInstance
            });

            #endregion

            #region ACT

            var mockLogger = new Mock<ILogger>();
            var mockILanguageService = new Mock<ILanguageService>();
            var mockConfigurationService =
                new ConfigurationService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
            var mockContactService = new ContactService(mockLogger.Object, null,
                xrmFakedContext.GetFakedOrganizationService(), mockConfigurationService, mockILanguageService.Object);
            mockContactService.CampusDistanceCalculatorLogic();

            #endregion

            #region ASSERT

            xrmFakedContext.Data["contact"].TryGetValue(contactInstance.Id, out var data);
            var updatedMilesFromCampus = data?.Attributes["cmc_milesfromcampus"].ToString();
            Assert.IsTrue(updatedMilesFromCampus == "0");

            #endregion ASSERT  
        }

        private static Entity GetCurrentAssociatedAccount()
        {
            return new Account
            {
                AccountId = Guid.NewGuid(),
                Address1_Latitude = 14.978600,
                Address1_Longitude = 97.661100
            };
        }

        private static Entity GetCurrentAssociatedAccountWithSameLatitudeAndLongitude()
        {
            return new Account
            {
                AccountId = Guid.NewGuid(),
                Address1_Latitude = 12.978600,
                Address1_Longitude = 87.661100
            };
        }

        private static Entity PrepareContactInstance(Entity currentaccountInstance)
        {
            var data = new Models.Contact
            {
                ContactId = Guid.NewGuid(),
                ParentCustomerId = currentaccountInstance.ToEntityReference(),
                Address1_Latitude = 12.978600,
                Address1_Longitude = 87.661100
            };
            return data;
        }
    }
}