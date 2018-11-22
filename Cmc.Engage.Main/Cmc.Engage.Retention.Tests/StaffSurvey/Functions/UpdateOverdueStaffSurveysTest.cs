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

namespace Cmc.Engage.Retention.Tests.StaffSurvey.Functions
{
    [TestClass]
    public class UpdateOverdueStaffSurveysTest : XrmUnitTestBase
    {
        [TestCategory("Function"), TestCategory("Positive")]
        [TestMethod]
        public void UpdateOverdueStaffSurveys_ChangeSurveyStatusToOverdue()
        {
            #region Arrange

            var staffSurvey = CreateStaffSurvey();
            var xrmFakedContext = new XrmFakedContext();

            xrmFakedContext.Initialize(new List<Entity>()
                {
                    staffSurvey
                });
            #endregion Arrange

            #region ACT

            var mockLogger = new Mock<ILogger>();
            var mockILanguageService = new Mock<ILanguageService>();
            var mockConfigurationService = new ConfigurationService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
            var staffSurveyService = new StaffSurveyService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService(), mockConfigurationService, mockILanguageService.Object);
            staffSurveyService.UpdateSurveysCrossedDuedate();

            #endregion ACT

            #region ASSERT

            //This Function is used to update Active staffsurvey's statuscode which crossed duedate to Overdue           

            xrmFakedContext.Data["cmc_staffsurvey"].TryGetValue(staffSurvey.Id, out staffSurvey);
            Assert.AreEqual(((OptionSetValue)staffSurvey.Attributes["statuscode"]).Value, (int)cmc_staffsurvey_statuscode.Overdue);

            #endregion ASSERT
        }
        private Entity CreateStaffSurvey()
        {
            return new cmc_staffsurvey
            {
                Id = Guid.NewGuid(),
                cmc_staffsurveyname = "Test Survey",
                statecode = cmc_staffsurveyState.Active,
                statuscode = cmc_staffsurvey_statuscode.New,
                cmc_duedate = DateTime.Now.AddDays(-1)
            };
        }
    }
}
