using System;
using System.Collections.Generic;
using System.Reflection;
using Cmc.Core.Xrm.ServerExtension.Logging;
using Cmc.Engage.Common.Plugins.Tests.Utilities;
using Cmc.Engage.Models.Tests;
using FakeItEasy;
using FakeXrmEasy;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Moq;

namespace Cmc.Engage.Common.Tests.Language.Plugin
{
    [TestClass]
    public class RetrieveMultiLingualValuesTest : XrmUnitTestBase
    {
        [TestCategory("Plugin"), TestCategory("Positive")]
        [TestMethod]
        public void RetrieveMultiLingualValues_Test()
        {
            #region ARRANGE
            var xrmFakedContext = new XrmFakedContext();
            var calledId = Guid.NewGuid();
            xrmFakedContext.CallerId = new EntityReference("systemuser", calledId);

            var systemUser = new Entity("SystemUser", xrmFakedContext.CallerId.Id)
            {
                ["systemuserid"] = xrmFakedContext.CallerId.Id,
            };

            var userSettings = GetUserSettings(systemUser);
            var langvalue = GetLanguagevalue();

            xrmFakedContext.Initialize(new List<Entity>()
            {
                systemUser,
                userSettings,
                langvalue
            });

            var mockServiceProvider = InitializeMockService(xrmFakedContext, userSettings, Operation.cmc_RetrieveMultiLingualValues);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            var stringInput = "['Ribbon_Loading']";
            AddInputParameters(mockServiceProvider, "Keys", stringInput);

            #endregion ARRANGE

            #region ACT

            var mockLogger = new Mock<ILogger>();
            var mockLanguageService = new LanguageService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
            mockLanguageService.RetrieveMultiLingualValues(mockExecutionContext.Object);
            #endregion ACT

            #region ASSERT

            var mockPluginExecutionContext = (IPluginExecutionContext)mockServiceProvider.Object.GetService(typeof(IPluginExecutionContext));
            var result = mockPluginExecutionContext.OutputParameters["Output"];
            Assert.IsNotNull(result);

            #endregion ASSERT

        }

        [TestCategory("Plugin"), TestCategory("Positive")]
        [TestMethod]
        public void RetrieveMultiLingualValuesTest_LanguageCodeIsNull()
        {
            #region ARRANGE
            var systemuser = GetSystemUser();            
            var userSettings = GetUserSettings(systemuser);
            
            var xrmFakedContext = new XrmFakedContext();
            
            xrmFakedContext.Initialize(new List<Entity>()
            {
                userSettings,
                systemuser
            });

            var mockServiceProvider = InitializeMockService(xrmFakedContext, systemuser, Operation.cmc_RetrieveMultiLingualValues);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            var stringInput = "['" + systemuser.Id + "']";
            AddInputParameters(mockServiceProvider, "Keys", stringInput);
            
            #endregion ARRANGE

            #region ACT
            
            var mockLogger = new Mock<ILogger>();
            var mockLanguageService = new LanguageService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
            mockLanguageService.RetrieveMultiLingualValues(mockExecutionContext.Object);
            #endregion ACT

            #region ASSERT

            var mockPluginExecutionContext = (IPluginExecutionContext)mockServiceProvider.Object.GetService(typeof(IPluginExecutionContext));
            var result = mockPluginExecutionContext.OutputParameters["Output"];
            Assert.IsNotNull(result);

            #endregion ASSERT
        }

        
        private SystemUser GetSystemUser()
        {
            return new SystemUser()
            {
                SystemUserId = Guid.NewGuid()
            };
        }

        private UserSettings GetUserSettings(Entity systemuser)
        {
            return new UserSettings()
            {
                SystemUserId = systemuser.Id,  
                UILanguageId = 1033,                             
            };            
        }

        private cmc_languagevalue GetLanguagevalue()
        {
            return new cmc_languagevalue()
            {
                cmc_languagevalueId = Guid.NewGuid(),
                cmc_keyname = "Ribbon_Loading",
                cmc_value = "Loading",
                cmc_languagecode = 1033
            };
        }
    }
}
