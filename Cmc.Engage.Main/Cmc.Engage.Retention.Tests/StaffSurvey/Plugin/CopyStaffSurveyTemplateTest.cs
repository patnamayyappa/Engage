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
    public class CopyStaffSurveyTemplateTest : XrmUnitTestBase
    {
        [TestCategory("Plugin"), TestCategory("Positive")]
        [TestMethod]
        public void CopyStaffSurveyTemplateService_CreateStaffSurveyTemplate()
        {
            #region Arrange

            var staffSurveyTemplate = CreateStaffSurveyTemplate(true);
            var staffSurveyTemplatecopy = CreateStaffSurveyTemplate(false);
            var staffSurveyQuestion = CreateStaffSurveyQuestion(staffSurveyTemplate.ToEntityReference());

            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>()
            {
                staffSurveyTemplate,
                staffSurveyQuestion,
                staffSurveyTemplatecopy
            });

            xrmFakedContext.AddRelationship("cmc_staffsurveytemplate_cmc_staffsurveyqu", new XrmFakedRelationship
            {
                RelationshipType = XrmFakedRelationship.enmFakeRelationshipType.OneToMany,
                Entity1LogicalName = cmc_staffsurveytemplate.EntityLogicalName,
                Entity1Attribute = "cmc_staffsurveytemplateid",
                Entity2LogicalName = cmc_staffsurveyquestion.EntityLogicalName,
                Entity2Attribute = "cmc_staffsurveyquestionid"
            });
            #endregion Arrange

            #region ACT          

            var mockLogger = new Mock<ILogger>();
            var mockILanguageService = new Mock<ILanguageService>();

            var mockServiceProvider = InitializeMockService(xrmFakedContext, null, Operation.cmc_CopyStaffSurveyTemplate);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            var mockConfigurationService = new ConfigurationService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
            var mockPluginExecutionContext = (IPluginExecutionContext)mockServiceProvider.Object.GetService(typeof(IPluginExecutionContext));

            var staffSurveyService = new StaffSurveyService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService(), mockConfigurationService, mockILanguageService.Object);
            

            AddInputParameters(mockServiceProvider, "Target", staffSurveyTemplate.ToEntityReference());
            staffSurveyService.CopyStaffSurveyTemplate(mockExecutionContext.Object);
            var staffSurveyTemplateId = Guid.Parse(mockPluginExecutionContext.OutputParameters["copysurveyTemplateResponse"].ToString());

            mockPluginExecutionContext.InputParameters.Clear();

            AddInputParameters(mockServiceProvider, "Target", staffSurveyTemplatecopy.ToEntityReference());
            staffSurveyService.CopyStaffSurveyTemplate(mockExecutionContext.Object);
            var staffSurveyTemplateIdCopy = Guid.Parse(mockPluginExecutionContext.OutputParameters["copysurveyTemplateResponse"].ToString());


            #endregion ACT

            #region Assert

            /*This is the Custom Action plugin ussed to creat copy of Staff Survey Template
             *which returns newly created Entity Id */

            var data = xrmFakedContext.Data["cmc_staffsurveytemplate"].ContainsKey(staffSurveyTemplateId)  && xrmFakedContext.Data["cmc_staffsurveytemplate"].ContainsKey(staffSurveyTemplateIdCopy);
            Assert.IsTrue(data);

            #endregion Assert
        }

        private Entity CreateStaffSurveyTemplate(bool isCopy)
        {
            return new cmc_staffsurveytemplate
            {
                Id = Guid.NewGuid(),
                cmc_staffsurveytemplatename = "Test Template",
                cmc_description = "Test Description",
                cmc_startdatecalculationtype = isCopy ? cmc_staffsurveytemplate_cmc_startdatecalculationtype.Static : cmc_staffsurveytemplate_cmc_startdatecalculationtype.Calculated,
                cmc_startdatestatic = DateTime.Now,
                cmc_startdatenumberofdays = 1,
                cmc_startdatedaysdirection = cmc_staffsurveytemplate_cmc_startdatedaysdirection.After,
                cmc_startdatecalculationfield = cmc_staffsurveytemplate_cmc_startdatecalculationfield.AssignmentDate,
                cmc_duedatecalculationtype = isCopy ? cmc_staffsurveytemplate_cmc_duedatecalculationtype.Static : cmc_staffsurveytemplate_cmc_duedatecalculationtype.Calculated,
                cmc_duedatestatic = DateTime.Now,
                cmc_duedatenumberofdays = 2,
                cmc_duedatedaysdirection = cmc_staffsurveytemplate_cmc_duedatedaysdirection.After,
                cmc_duedatecalculationfield = cmc_staffsurveytemplate_cmc_duedatecalculationfield.AssignmentDate

            };
        }
        private Entity CreateStaffSurveyQuestion(EntityReference staffSurveyTemplate)
        {
            return new cmc_staffsurveyquestion
            {
                Id = Guid.NewGuid(),
                cmc_staffsurveyquestionname = "Test Question",
                cmc_QuestionType = cmc_staffsurveyquestion_cmc_questiontype.TextField,
                cmc_questionorder = 1,
                cmc_staffsurveytemplateid = new EntityReference("cmc_staffsurveytemplate", staffSurveyTemplate.Id)

            };
        }
    }

}
