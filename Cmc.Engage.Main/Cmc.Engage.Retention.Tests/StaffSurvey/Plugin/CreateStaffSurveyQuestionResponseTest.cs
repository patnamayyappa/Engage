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
    public class CreateStaffSurveyQuestionResponseTest: XrmUnitTestBase
    {
        [TestCategory("Plugin"), TestCategory("Positive")]
        [TestMethod]
        public void CreateStaffSurveyQuestionResponsePlugin_GetStaffSurveyResponse_UpdateStaffsurvey()
        {
            #region ARRANGE
            var survey = PrepareStaffSurvey();
            var staffsurveyresponse = PrepareStaffSurveyResponse(survey.Id);
            var staffsurveyquestion = PrepareStaffSurveyQuestion();
            var staffsurveyquestionresponse = PrepareStaffSurveyQuestionResponse(staffsurveyresponse.Id, staffsurveyquestion.Id);
            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>
            {
                survey,
                staffsurveyresponse,
                staffsurveyquestion,
                staffsurveyquestionresponse
            });
            var mockServiceProvider = InitializeMockService(xrmFakedContext, null, Operation.cmc_CreateUpdateStaffSurveyQuestionResponses);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);

            #endregion

            #region ACT
            var mockLogger = new Mock<ILogger>();
            var mockILanguageService = new Mock<ILanguageService>();
            var mockConfigurationService = new ConfigurationService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());

            var entityCollectionSurveyData = new EntityCollection();
            entityCollectionSurveyData.Entities.Add(staffsurveyquestionresponse);

            AddInputParameters(mockServiceProvider, "Target", survey.ToEntityReference());
            AddInputParameters(mockServiceProvider, "surveyData", entityCollectionSurveyData);
            AddInputParameters(mockServiceProvider, "isSubmitted", 1);
            var staffSurvey = new StaffSurveyService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService(), mockConfigurationService, mockILanguageService.Object);
            staffSurvey.CreateStaffSurveyQuestionResponses(mockExecutionContext.Object);
            #endregion

            #region ASSERT

            var resultData = new Entity("cmc_staffsurvey");
            xrmFakedContext.Data["cmc_staffsurvey"].TryGetValue(survey.Id, out resultData);
            var dataissubmit = (bool)resultData.Attributes["cmc_issubmit"];
            Assert.AreEqual(true,dataissubmit);

            #endregion
        }
        [TestCategory("Plugin"), TestCategory("Positive")]
        [TestMethod]
        public void CreateStaffSurveyQuestionResponsePlugin_GetStaffSurveyResponse_Createstaffsurveyquestionresponse()
        {
            #region ARRANGE
            var survey = PrepareStaffSurvey();
            var staffsurveyresponse = PrepareStaffSurveyResponse(survey.Id);
            var staffsurveyquestion = PrepareStaffSurveyQuestion();
            var dummyStaffSurveyResponse = PrepareStaffSurveyResponse(survey.Id);
            var staffsurveyquestionresponse = PrepareStaffSurveyQuestionResponse(dummyStaffSurveyResponse.Id, staffsurveyquestion.Id);

            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>
            {
                survey,
                staffsurveyresponse,
                staffsurveyquestion,
                //staffsurveyquestionresponse
            });
            var mockServiceProvider = InitializeMockService(xrmFakedContext, null, Operation.cmc_CreateUpdateStaffSurveyQuestionResponses);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);

            #endregion

            #region ACT

            var mockLogger = new Mock<ILogger>();
            var mockILanguageService = new Mock<ILanguageService>();
            var mockConfigurationService = new ConfigurationService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());

            var entityCollectionSurveyData = new EntityCollection();
            entityCollectionSurveyData.Entities.Add(staffsurveyquestionresponse);

            AddInputParameters(mockServiceProvider, "Target", survey.ToEntityReference());
            AddInputParameters(mockServiceProvider, "surveyData", entityCollectionSurveyData);
            AddInputParameters(mockServiceProvider, "isSubmitted", 1);
            var staffSurvey = new StaffSurveyService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService(), mockConfigurationService, mockILanguageService.Object);
            staffSurvey.CreateStaffSurveyQuestionResponses(mockExecutionContext.Object);

            #endregion

            #region ASSERT

            var values=xrmFakedContext.Data["cmc_staffsurveyquestionresponse"].Values;
            Assert.IsNotNull(values);
            #endregion
        }

        private cmc_staffsurvey PrepareStaffSurvey()
        {
            return new cmc_staffsurvey()
            {
                Id = Guid.NewGuid()
            };
        }
        private cmc_staffsurveyresponse PrepareStaffSurveyResponse(Guid surveyId)
        {
            var staffsurveyresponse=new cmc_staffsurveyresponse()
            {
                Id = Guid.NewGuid(),
                cmc_staffsurveyId = new EntityReference("cmc_staffsurvey", surveyId)
            };
            return staffsurveyresponse;
        }
        private cmc_staffsurveyquestion PrepareStaffSurveyQuestion()
        {
            var staffsurveyquestion = new cmc_staffsurveyquestion()
            {
                Id = Guid.NewGuid()
            };
            return staffsurveyquestion;
        }
        private cmc_staffsurveyquestionresponse PrepareStaffSurveyQuestionResponse(Guid staffsurveyresponseId,Guid staffsurveyquestionId)
        {
            var staffsurveyquestionresponse = new cmc_staffsurveyquestionresponse()
            {
                Id = Guid.NewGuid(),
                cmc_staffsurveyresponseid = new EntityReference("cmc_staffsurveyresponseid", staffsurveyresponseId),
                cmc_staffsurveyquestionid = new EntityReference("cmc_staffsurveyquestionid", staffsurveyquestionId),
                cmc_response = "Test Response",
            };
            return staffsurveyquestionresponse;
        }
    }
}
