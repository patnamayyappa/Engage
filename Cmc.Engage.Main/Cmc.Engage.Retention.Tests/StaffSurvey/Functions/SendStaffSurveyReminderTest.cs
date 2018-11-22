using System;
using System.Collections.Generic;
using Cmc.Core.Xrm.ServerExtension.Logging;
using Cmc.Engage.Common;
using Cmc.Engage.Common.Plugins.Tests.Utilities;
using Cmc.Engage.Common.Utilities.Constants;
using Cmc.Engage.Models;
using FakeXrmEasy;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Moq;

namespace Cmc.Engage.Retention.Tests.StaffSurvey.Functions
{
    [TestClass]
    public class SendStaffSurveyReminderTest : XrmUnitTestBase
    {
        private const string WorkflowName = "Staff Survey - Send Email";
        [TestCategory("Function"), TestCategory("Positive")]
        [TestMethod]
        public void SendStaffSurveyReminder_ForStatusCode_New()
        {
            #region ARRANGE

            var configurationList = GetConfigurationList();
            var staffSurveyList = GetStaffSurveyList();
            var workFlowDetails = GetWorkFlowDetails();
            var entityList = new List<Entity>();
            entityList.AddRange(configurationList);
            entityList.AddRange(staffSurveyList);
            entityList.Add(workFlowDetails);
            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(entityList);

            #endregion

            #region ACT

            var mockLogger = new Mock<ILogger>();
            var mockILanguageService = new Mock<ILanguageService>();
            var mockConfigurationService =
                new ConfigurationService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
            var staffSurveyService = new StaffSurveyService(mockLogger.Object,
                xrmFakedContext.GetFakedOrganizationService(), mockConfigurationService, mockILanguageService.Object);
            staffSurveyService.SendStaffSurveyReminderEmail();

            #endregion

            #region ASSERT

            // To do this Assert part.

            #endregion ASSERT  
        }

        [TestCategory("Function"), TestCategory("Negative")]
        [TestMethod]
        public void SendStaffSurveyReminder_ForStatusCode_ConfigurationDetails_IsNull()
        {
            #region ARRANGE

            var configurationList = GetConfigurationList1();
            var staffSurveyList = GetStaffSurveyList();
            var workFlowDetails = GetWorkFlowDetails();
            var entityList = new List<Entity>();
            entityList.AddRange(configurationList);
            entityList.AddRange(staffSurveyList);
            entityList.Add(workFlowDetails);
            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(entityList);

            #endregion

            #region ACT

            var mockLogger = new Mock<ILogger>();
            var mockILanguageService = new Mock<ILanguageService>();
            var mockConfigurationService =
                new ConfigurationService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
            var staffSurveyService = new StaffSurveyService(mockLogger.Object,
                xrmFakedContext.GetFakedOrganizationService(), mockConfigurationService, mockILanguageService.Object);
            staffSurveyService.SendStaffSurveyReminderEmail();

            #endregion

            #region ASSERT

            // To do this Assert part.

            #endregion ASSERT  
        }

        private static IEnumerable<Entity> GetConfigurationList()
        {
            return new List<Entity>
            {
                new cmc_configuration
                {
                    Id = Guid.NewGuid(),
                    cmc_staffsurveyemailworkflow = new EntityReference("cmc_staffsurveyemailworkflow", Guid.NewGuid())
                    {
                        Name = WorkflowName
                    },
                    cmc_staffsurveysendreminderemailnumberofdays = 3                
                }
            };
        }
        private static IEnumerable<Entity> GetConfigurationList1()
        {
            return new List<Entity>
            {
                new cmc_configuration
                {
                    Id = Guid.NewGuid(),
                    
                }
            };
        }


        private static IEnumerable<Entity> GetStaffSurveyList()
        {
            return new List<Entity>
            {
                new cmc_staffsurvey
                {
                    cmc_staffsurveyId = Guid.NewGuid(),
                    statuscode = cmc_staffsurvey_statuscode.New,
                    cmc_startdate = DateTime.UtcNow.Date.AddDays(-1),
                    cmc_staffsurveyname = "Test",
                    cmc_duedate = DateTime.UtcNow
                },
                new cmc_staffsurvey
                {
                    cmc_staffsurveyId = Guid.NewGuid(),
                    statuscode = cmc_staffsurvey_statuscode.New,
                    cmc_startdate = DateTime.UtcNow.Date,
                    cmc_staffsurveyname = "Test2",
                    cmc_duedate = DateTime.UtcNow
                }
            };
        }

        private static Entity GetWorkFlowDetails()
        {
            return new Workflow
            {
                WorkflowId = Guid.NewGuid(),
                Name = WorkflowName,
                StatusCode = workflow_statuscode.Activated,
                Type = workflow_type.Definition                
            };
        }
    }
}