using System;
using System.Collections.Generic;
using Cmc.Core.Xrm.ServerExtension.Logging;
using Cmc.Engage.Common.Plugins.Tests.Utilities;
using FakeXrmEasy;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Moq;
using Cmc.Engage.Models.Tests;

namespace Cmc.Engage.Common.Tests.WebSerivce
{
    [TestClass]
    public class PortalRetrieveMultiLingualValuesTest : XrmUnitTestBase
    {

        [TestCategory("Web Service"), TestCategory("Positive")]
        [TestMethod]
        public void PortalRetrieveMultiLingualValues_ForDoWork()
        {
            #region ARRANGE
            var adx_Website = new Entity("adx_Website") { Id = Guid.NewGuid() };
            var languageEntity = CreatePortalLanguageEntity();

            var websiteEntity = WebsiteLanguageEntity(languageEntity, adx_Website);
            var contactInstance = PreparingContactEntity();

            var languageInstance = preparingLanguageValue();
            var systemuser = PreparingsystemUser();
            var usersetting = PreparingUserSetiing(systemuser);

            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>()
                {
                   languageEntity,
                   contactInstance,
                   websiteEntity,
                   systemuser,
                   usersetting,
                   languageInstance
                });

            var mockServiceProvider = InitializeMockService(xrmFakedContext, systemuser, Operation.RetrieveMultiple);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);

            var stringInput = "{'ContactId': '" + contactInstance.Id + "' ,'WebsiteId':'" + adx_Website.Id + "', 'Keys':'Ribbon_Loading'}";
            #endregion

            #region ACT

            //Mock the ILogger
            var mockLogger = new Mock<ILogger>();

            var getPortalUserLanguageCode = new GetPortalUserLanguageCode(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
            var mockILanguageService = new LanguageService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
            var portalRetrieveMultiLingualValues = new PortalRetrieveMultiLingualValues(mockLogger.Object, mockILanguageService, getPortalUserLanguageCode, xrmFakedContext.GetFakedOrganizationService());
            var data = portalRetrieveMultiLingualValues.DoWork(mockExecutionContext.Object, stringInput);

            #endregion


            #region ASSERT
            Assert.IsNotNull(data);
            #endregion

        }


        [TestCategory("Web Service"), TestCategory("Negative")]
        [TestMethod]

        public void PortalRetrieveMultiLingualValuesForNegativeScenario()
        {
            #region ARRANGE
            var xrmFakedContext = new XrmFakedContext();
            #endregion

            #region ACT
            var mockLogger = new Mock<ILogger>();
            var mockLanguageService = new Mock<ILanguageService>();
            var getPortalUserLanguageCode = new GetPortalUserLanguageCode(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
            #endregion

            #region ASSERT
            Assert.ThrowsException<ArgumentNullException>(() => new PortalRetrieveMultiLingualValues(null, mockLanguageService.Object, getPortalUserLanguageCode, xrmFakedContext.GetFakedOrganizationService()));
            Assert.ThrowsException<ArgumentException>(() => new PortalRetrieveMultiLingualValues(mockLogger.Object, mockLanguageService.Object, getPortalUserLanguageCode, null));
            Assert.ThrowsException<ArgumentNullException>(() => new PortalRetrieveMultiLingualValues(mockLogger.Object, null, getPortalUserLanguageCode, xrmFakedContext.GetFakedOrganizationService()));
            Assert.ThrowsException<ArgumentNullException>(() => new PortalRetrieveMultiLingualValues(mockLogger.Object, mockLanguageService.Object, null, xrmFakedContext.GetFakedOrganizationService()));
            #endregion
        }


        #region DATA PREPARING
        private cmc_languagevalue preparingLanguageValue()
        {
            var languagevalue = new cmc_languagevalue()
            {
                cmc_languagevalueId = Guid.NewGuid(),
                cmc_languagecode = 1033,
                cmc_keyname = "Ribbon_Loading",
                cmc_value = "Loading"
            };
            return languagevalue;
        }
        private adx_websitelanguage WebsiteLanguageEntity(Entity portalLanguage, Entity adx_Website)
        {
            return new adx_websitelanguage()
            {
                Id = Guid.NewGuid(),
                adx_WebsiteId = adx_Website.ToEntityReference(),
                adx_PortalLanguageId = portalLanguage.ToEntityReference(),
            };
        }
        private Models.Contact PreparingContactEntity()
        {
            var contact = new Models.Contact()
            {
                ContactId = Guid.NewGuid()
            };
            return contact;
        }

        private UserSettings PreparingUserSetiing(Entity userInstance)
        {
            var userSettingInstance = new UserSettings()
            {
                SystemUserId = Guid.NewGuid(),
                UILanguageId = 1033
            };
            return userSettingInstance;
        }

        private SystemUser PreparingsystemUser()
        {
            var systemuserInstance = new SystemUser()
            {
                SystemUserId = Guid.NewGuid()
            };
            return systemuserInstance;
        }

        private adx_portallanguage CreatePortalLanguageEntity()
        {
            return new adx_portallanguage()
            {
                Id = Guid.NewGuid(),
                adx_lcid = 1033
            };
        }

        #endregion

    }
}
