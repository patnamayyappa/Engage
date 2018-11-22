using System;
using System.Collections.Generic;
using Cmc.Core.Xrm.ServerExtension.Logging;
using Cmc.Engage.Common;
using Cmc.Engage.Common.Plugins.Tests.Utilities;
using Cmc.Engage.Models;
using FakeXrmEasy;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Moq;

namespace Cmc.Engage.Retention.Tests.StaffSurvey.Plugin
{
    [TestClass]
    public class UpdateStaffSurveyStatusTest : XrmUnitTestBase
    {
        [TestCategory("Plugin"), TestCategory("Positive")]
        [TestMethod]
        public void UpdateStaffSurveyStatusService_UpdateStatus()
        {
            #region ARRANGE
            var staffSurvey = CreateStaffSurvey(true);
            var inActiveStaffSurvey = CreateStaffSurvey(false);
            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>()
            {
                staffSurvey,
                inActiveStaffSurvey
            });
            #endregion

            #region ACT          

            var mockLogger = new Mock<ILogger>();
            var mockILanguageService = new Mock<ILanguageService>();

            var mockServiceProvider = InitializeMockService(xrmFakedContext, staffSurvey, Operation.Update);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            var mockConfigurationService = new ConfigurationService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
            var staffSurveyService = new StaffSurveyService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService(), mockConfigurationService,mockILanguageService.Object);
            var mockPluginExecutionContext = (IPluginExecutionContext)mockServiceProvider.Object.GetService(typeof(IPluginExecutionContext));

            AddPostEntityImage(mockServiceProvider, "PostImage", staffSurvey);
            staffSurveyService.UpdateStaffSurveyCompletedCancellationDate(mockExecutionContext.Object);

            mockPluginExecutionContext.PostEntityImages.Clear();

            AddPostEntityImage(mockServiceProvider, "PostImage", inActiveStaffSurvey);
            staffSurveyService.UpdateStaffSurveyCompletedCancellationDate(mockExecutionContext.Object);

            #endregion Act

            #region ASSERT

            /*This plugin will be called post update action 
            if Survey is made active then it should clear Completed or Cancelled date .*/

            xrmFakedContext.Data["cmc_staffsurvey"].TryGetValue(staffSurvey.Id, out staffSurvey);
            var activeSurveyComment = staffSurvey.Attributes["cmc_cancellationcomment"];
            Assert.IsNull(activeSurveyComment);

            xrmFakedContext.Data["cmc_staffsurvey"].TryGetValue(inActiveStaffSurvey.Id, out inActiveStaffSurvey);
            var inActiveSurveyComment = inActiveStaffSurvey.Attributes["cmc_cancellationcomment"];
            Assert.IsNotNull(inActiveSurveyComment);

            #endregion ASSERT
        }
        
        private Entity CreateStaffSurvey(bool isActive)
        {
            return new cmc_staffsurvey
            {
                Id = Guid.NewGuid(),
                statecode = isActive ? cmc_staffsurveyState.Active : cmc_staffsurveyState.Inactive,
                cmc_cancellationcomment = "Test Comment",
                cmc_completedcancelleddate = DateTime.Now
            };
        }
    }
}
