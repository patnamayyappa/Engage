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
    public class StaffSurveyTemplateTest : XrmUnitTestBase
    {
        [TestCategory("Plugin"), TestCategory("Positive")]
        [TestMethod]
        public void StaffSurveyTemplate_SaveStaffSurveyTemplate()
        {
            #region ARRANGE
            var createstaffsurveytemp = StaffSurveyTemplate();
            var staffsurveytemplate = new EntityReference("cmc_staffsurveytemplate", createstaffsurveytemp.Id);
            var createquestions = Questions(staffsurveytemplate);
            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>
            {
                createstaffsurveytemp,
                createquestions
            });
            var mockServiceProvider = InitializeMockService(xrmFakedContext, null, Operation.cmc_SurveyTemplateCreateQuestions);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
           

            var questions = new EntityCollection();
            questions.Entities.Add(createquestions);

            AddInputParameters(mockServiceProvider, "Target", staffsurveytemplate);
            AddInputParameters(mockServiceProvider, "Questions", questions);

            #endregion

            #region ACT

            var mockLogger = new Mock<ILogger>();
            var mockILanguageService = new Mock<ILanguageService>();
            var mockConfigurationService = new ConfigurationService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
            var staffSurveyService = new StaffSurveyService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService(), mockConfigurationService,mockILanguageService.Object);
            staffSurveyService.SaveStaffSurveyTemplate(mockExecutionContext.Object);

            #endregion

            #region ASSERT
            
            var resultData = new Entity("cmc_staffsurveyquestion");
            xrmFakedContext.Data["cmc_staffsurveyquestion"].TryGetValue(createquestions.Id, out resultData);

            var data = resultData.Attributes["cmc_staffsurveyquestionname"];
            Assert.IsNotNull(data);

            #endregion
        }

        private Entity StaffSurveyTemplate()
        {
            var staff = new cmc_staffsurveytemplate()
            {
                Id = Guid.NewGuid(),
                //cmc_staffsurveytemplatename = "Test staff survey name"               
            };
            return staff;
        }

        private Entity Questions(EntityReference staffsurveytemplate)
        {
            var ques = new cmc_staffsurveyquestion()
            {
                cmc_staffsurveyquestionname = "Test question name",
                cmc_choice = "Test choice",
                cmc_staffsurveyquestionId = Guid.NewGuid(),
                cmc_QuestionType = cmc_staffsurveyquestion_cmc_questiontype.CheckBox,
                cmc_questionorder = 1,
                cmc_staffsurveytemplateid = staffsurveytemplate
            };
            return ques;
        }
    }
}
