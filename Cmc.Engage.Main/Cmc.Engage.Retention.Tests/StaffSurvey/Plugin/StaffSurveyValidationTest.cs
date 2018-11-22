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
    public class StaffSurveyValidationTest : XrmUnitTestBase
    {
        [TestCategory("Plugin"), TestCategory("Positive")]
        [TestMethod]
        public void StaffSurveyValidation_ValidateSurveyInstance()
        {
            #region ARRANGE

            var academicperiod = PrepareAcademicPeriod(false);
            var staffcourse = PrepareStaffCourse(academicperiod.ToEntityReference(),true);
            var staffsurveytemplate = PrepareStaffSurveyTemplate();
            var staffsurvey = PrepareStaffSurvey(staffcourse, staffsurveytemplate.Id);

            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>
            {
                academicperiod,
                staffsurvey,
                staffsurveytemplate,
                staffcourse
            });

            var mockServiceProvider = InitializeMockService(xrmFakedContext, staffsurvey, Operation.Create);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            var mockPluginExecutionContext = (IPluginExecutionContext)mockServiceProvider.Object.GetService(typeof(IPluginExecutionContext));

            #endregion

            #region ACT

            var mockLogger = new Mock<ILogger>();
            var mockILanguageService = new Mock<ILanguageService>();
            var mockConfigurationService = new ConfigurationService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
            var staffSurveyService = new StaffSurveyService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService(), mockConfigurationService,mockILanguageService.Object);
            staffSurveyService.ValidateSurveyInstance(mockExecutionContext.Object);

            #endregion

            #region ASSERT

            var resultdata = new Entity("cmc_staffsurvey");
            xrmFakedContext.Data["cmc_staffsurvey"].TryGetValue(staffsurvey.Id, out resultdata);

            var data = resultdata.Attributes["cmc_staffsurveytemplateid"];
                Assert.IsNotNull(data);

            #endregion
        }

        [TestCategory("Plugin"), TestCategory("Positive")]
        [TestMethod]
        public void StaffSurveyValidation_ValidateSurveyInstance_NullDate()
        {
            #region ARRANGE

            var academicperiod = PrepareAcademicPeriod(true);
            var staffcourse = PrepareStaffCourse(academicperiod.ToEntityReference(),true);
            var staffsurveytemplate = PrepareStaffSurveyTemplate();
            var staffsurvey = PrepareStaffSurvey(staffcourse, staffsurveytemplate.Id);

            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>
            {
                academicperiod,
                staffsurvey,
                staffsurveytemplate,
                staffcourse
            });

            var mockServiceProvider = InitializeMockService(xrmFakedContext, staffsurvey, Operation.Create);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            var mockPluginExecutionContext = (IPluginExecutionContext)mockServiceProvider.Object.GetService(typeof(IPluginExecutionContext));

            #endregion

            #region ACT

            var mockLogger = new Mock<ILogger>();
            var mockILanguageService = new Mock<ILanguageService>();
            var mockConfigurationService = new ConfigurationService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
            var staffSurveyService = new StaffSurveyService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService(), mockConfigurationService,mockILanguageService.Object);
            
            #endregion

            #region ASSERT
            
            Assert.ThrowsException<InvalidPluginExecutionException>(()=> staffSurveyService.ValidateSurveyInstance(mockExecutionContext.Object));

            #endregion
        }
        [TestCategory("Plugin"), TestCategory("Negative")]
        [TestMethod]
        public void StaffSurveyValidation_ValidateSurveyInstance_InactiveStaffCourse()
        {
            #region ARRANGE

            var academicperiod = PrepareAcademicPeriod(false);
            var staffcourse = PrepareStaffCourse(academicperiod.ToEntityReference(),false);
            var staffsurveytemplate = PrepareStaffSurveyTemplate();
            var staffsurvey = PrepareStaffSurvey(staffcourse, staffsurveytemplate.Id);

            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>
            {
                academicperiod,
                staffsurvey,
                staffsurveytemplate,
                staffcourse
            });

            var mockServiceProvider = InitializeMockService(xrmFakedContext, staffsurvey, Operation.Create);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);

            #endregion

            #region ACT

            var mockLogger = new Mock<ILogger>();
            var mockILanguageService = new Mock<ILanguageService>();
            var mockConfigurationService = new ConfigurationService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
            var staffSurveyService = new StaffSurveyService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService(), mockConfigurationService,mockILanguageService.Object);

            #endregion

            #region ASSERT

            Assert.ThrowsException<InvalidPluginExecutionException>(() => staffSurveyService.ValidateSurveyInstance(mockExecutionContext.Object));

            #endregion
        }

        [TestCategory("Plugin"), TestCategory("Negative")]
        [TestMethod]
        public void StaffSurveyValidation_ValidateSurveyInstance_StartDateLessThanToday()
        {
            #region ARRANGE

            var academicperiod = PrepareAcademicPeriod(false);
            var staffcourse = PrepareStaffCourse(academicperiod.ToEntityReference(), true);
            var staffsurveytemplate = PrepareStaffSurveyTemplate_InvalidDate();
            var staffsurvey = PrepareStaffSurvey(staffcourse, staffsurveytemplate.Id);

            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>
            {
                academicperiod,
                staffsurvey,
                staffsurveytemplate,
                staffcourse
            });

            var mockServiceProvider = InitializeMockService(xrmFakedContext, staffsurvey, Operation.Create);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);

            #endregion

            #region ACT

            var mockLogger = new Mock<ILogger>();
            var mockILanguageService = new Mock<ILanguageService>();
            var mockConfigurationService = new ConfigurationService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
            var staffSurveyService = new StaffSurveyService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService(), mockConfigurationService,mockILanguageService.Object);

            #endregion

            #region ASSERT

            Assert.ThrowsException<InvalidPluginExecutionException>(() => staffSurveyService.ValidateSurveyInstance(mockExecutionContext.Object));

            #endregion
        }

        [TestCategory("Plugin"), TestCategory("Negative")]
        [TestMethod]
        public void StaffSurveyValidation_ValidateSurveyInstance_DuplicateSurvey()
        {
            #region ARRANGE

            var academicperiod = PrepareAcademicPeriod(false);
            var staffcourse = PrepareStaffCourse(academicperiod.ToEntityReference(), true);
            var staffsurveytemplate = PrepareStaffSurveyTemplate();
            var staffsurvey = PrepareStaffSurvey(staffcourse, staffsurveytemplate.Id);
            var staffsurvey2 = PrepareStaffSurvey2(staffcourse, staffsurveytemplate.Id);

            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>
            {
                academicperiod,
                staffsurvey,
                staffsurveytemplate,
                staffsurvey2,
                staffcourse
            });

            var mockServiceProvider = InitializeMockService(xrmFakedContext, staffsurvey, Operation.Create);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);

            #endregion

            #region ACT

            var mockLogger = new Mock<ILogger>();
            var mockILanguageService = new Mock<ILanguageService>();

            var mockConfigurationService = new ConfigurationService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
            var staffSurveyService = new StaffSurveyService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService(), mockConfigurationService, mockILanguageService.Object);

            #endregion

            #region ASSERT

            Assert.ThrowsException<InvalidPluginExecutionException>(() => staffSurveyService.ValidateSurveyInstance(mockExecutionContext.Object));

            #endregion
        }

        private Entity PrepareStaffSurvey(Entity course, Guid template)
        {
            var staff = new cmc_staffsurvey()
            {
                Id = Guid.NewGuid(),
                cmc_coursesectionid = course.ToEntityReference(),
                cmc_staffsurveytemplateid = new EntityReference("cmc_staffsurveytemplate",template)
            };
            return staff;
        }
        private Entity PrepareStaffSurvey2(Entity course, Guid template)
        {
            var staff = new cmc_staffsurvey()
            {
                Id = Guid.NewGuid(),
                cmc_coursesectionid =course.ToEntityReference(),
                cmc_staffsurveytemplateid = new EntityReference("cmc_staffsurveytemplate", template),
                statuscode = cmc_staffsurvey_statuscode.InProgress,
                cmc_startdate = DateTime.Now.AddDays(6),
                cmc_duedate = DateTime.Now.AddDays(7)
            };
            return staff;
        }

        private Entity PrepareAcademicPeriod(bool isnull)
        {
            var academic = new mshied_academicperiod()
            {
                Id = Guid.NewGuid(),
                mshied_StartDate = isnull ? (DateTime?)null : DateTime.Now,
                mshied_EndDate = isnull ? (DateTime?)null : DateTime.Now.AddDays(60)                
            };           
            return academic;
        }
        private Entity PrepareStaffCourse(EntityReference academic,bool isActive)
        {
            var course = new mshied_coursesection()
            {
                Id = Guid.NewGuid(),
                mshied_AcademicPeriodId = academic,
                mshied_InstructorId = new EntityReference("mshied_InstructorId", Guid.NewGuid()),
                statecode = isActive ? mshied_coursesectionState.Active : mshied_coursesectionState.Inactive
            };
            return course;
        }

        private Entity PrepareStaffSurveyTemplate()
        {
            var temp = new cmc_staffsurveytemplate()
            {
                Id = Guid.NewGuid(),
                cmc_staffsurveytemplatename = "Test name",
                cmc_description = "Description",                
                cmc_duedatecalculationfield = cmc_staffsurveytemplate_cmc_duedatecalculationfield.EndDateofAcademicPeriod,
                cmc_duedatedaysdirection = cmc_staffsurveytemplate_cmc_duedatedaysdirection.Before,
                cmc_duedatecalculationtype = cmc_staffsurveytemplate_cmc_duedatecalculationtype.Calculated,
                cmc_startdatestatic = DateTime.Now,
                cmc_duedatenumberofdays = 5,
                cmc_duedatestatic = DateTime.Now,
                cmc_startdatecalculationfield = cmc_staffsurveytemplate_cmc_startdatecalculationfield.AssignmentDate,
                cmc_startdatecalculationtype = cmc_staffsurveytemplate_cmc_startdatecalculationtype.Calculated,
                cmc_startdatedaysdirection = cmc_staffsurveytemplate_cmc_startdatedaysdirection.After,
                cmc_startdatenumberofdays = 3
                
            };
            return temp;
        }

        private Entity PrepareStaffSurveyTemplate_InvalidDate()
        {
            var temp = new cmc_staffsurveytemplate()
            {
                Id = Guid.NewGuid(),
                cmc_staffsurveytemplatename = "Test name",
                cmc_description = "Description",
                cmc_startdatecalculationtype = cmc_staffsurveytemplate_cmc_startdatecalculationtype.Static,
                cmc_startdatestatic = DateTime.Now.AddDays(-5),
                cmc_duedatecalculationtype = cmc_staffsurveytemplate_cmc_duedatecalculationtype.Static,
                cmc_duedatestatic = DateTime.Now,

            };
            return temp;
        }
    }
}
