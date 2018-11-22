using System;
using System.Collections.Generic;
using System.Web.Script.Serialization;
using Cmc.Core.Xrm.ServerExtension.Core;
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
    public class CreateStaffSurveyFromStaffSurveyTemplateTest: XrmUnitTestBase
    {
        [TestCategory("Plugin"), TestCategory("Positive")]
        [TestMethod]
        public void CreateStaffSurveyFromStaffSurveyTemplate_WhenStaffSurveyStartDateAndStaffSurveyDueDate_IsStaticType()
        {
            #region ARRANGE

            var creatingUser = preparingSystemUser();
            var creatingSurveyTemplate = PreparingSurveyTemplate();
            var creatingSurveyTemplateEntity = new EntityReference("cmc_staffsurveytemplate", creatingSurveyTemplate.Id);
            var creatingAcademicPeriod = PreparingAcademicPeriod();
            var creatingStaffCourse = PreparingStaffCourse(creatingAcademicPeriod, creatingUser.ToEntityReference());

            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>
            {
                creatingSurveyTemplate,
                creatingStaffCourse,
                creatingAcademicPeriod
            });

            var entitycollection = new EntityCollection();
            entitycollection.Entities.Add(creatingStaffCourse);

            #endregion

            #region ACT

            var mockServiceProvider = InitializeMockService(xrmFakedContext, null, Operation.Create);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            var mockLogger = new Mock<ILogger>();
            var mockILanguageService = new Mock<ILanguageService>();
            var mockConfigurationService = new ConfigurationService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
            AddInputParameters(mockServiceProvider, "Target", creatingSurveyTemplateEntity);
            AddInputParameters(mockServiceProvider, "Staffs", entitycollection);
            var staffService = new StaffSurveyService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService(), mockConfigurationService, mockILanguageService.Object);
            staffService.CreateStaffSurveyFromTemplate(mockExecutionContext.Object);

            #endregion

            #region ASSERT

            var fetchXml = $@"<fetch>
                                <entity name='cmc_staffsurvey'>
                                      <all-attributes/>                                               
                                </entity>
                            </fetch>";
            var data = xrmFakedContext.GetFakedOrganizationService().RetrieveMultipleAll(fetchXml)?.Entities?.Count > 0;
            //var data = xrmFakedContext.Data["cmc_staffsurvey"].Count > 0;
            Assert.IsTrue(data);

            #endregion

        }

        [TestCategory("Plugin"), TestCategory("Positive")]
        [TestMethod]
        public void CreateStaffSurveyFromStaffSurveyTemplate_WhenStaffSurveyStartDateAndStaffSurveyDueDate_Startdatecalculationfield_IsStartDateofAcademicPeriod()
        {
            #region ARRANGE

            var creatingUser = preparingSystemUser();
            var creatingSurveyTemplate = PreparingSurveyTemplate1();
            var creatingSurveyTemplateEntity = new EntityReference("cmc_staffsurveytemplate", creatingSurveyTemplate.Id);
            var creatingAcademicPeriod = PreparingAcademicPeriod();
            var creatingStaffCourse = PreparingStaffCourse(creatingAcademicPeriod, creatingUser.ToEntityReference());

            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>
            {
                creatingSurveyTemplate,
                creatingStaffCourse,
                creatingAcademicPeriod
            });

            var entitycollection = new EntityCollection();
            entitycollection.Entities.Add(creatingStaffCourse);

            #endregion

            #region ACT

            var mockServiceProvider = InitializeMockService(xrmFakedContext, null, Operation.Create);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            var mockLogger = new Mock<ILogger>();
            var mockILanguageService = new Mock<ILanguageService>();
            var mockConfigurationService = new ConfigurationService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
            AddInputParameters(mockServiceProvider, "Target", creatingSurveyTemplateEntity);
            AddInputParameters(mockServiceProvider, "Staffs", entitycollection);
            var staffService = new StaffSurveyService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService(), mockConfigurationService, mockILanguageService.Object);
            staffService.CreateStaffSurveyFromTemplate(mockExecutionContext.Object);

            #endregion

            #region ASSERT
            var fetchXml = $@"<fetch>
                                <entity name='cmc_staffsurvey'>
                                      <all-attributes/>                                               
                                </entity>
                            </fetch>";
            var data = xrmFakedContext.GetFakedOrganizationService().RetrieveMultipleAll(fetchXml)?.Entities?.Count > 0;
            //var data = xrmFakedContext.Data["cmc_staffsurvey"].Count > 0;
            Assert.IsTrue(data);            

            #endregion

        }

        [TestCategory("Plugin"), TestCategory("Positive")]
        [TestMethod]
        public void CreateStaffSurveyFromStaffSurveyTemplate_WhenStaffSurveyStartDateAndStaffSurveyDueDate_Startdatecalculationfield_IsEndDateofAcademicPeriod()
        {
            #region ARRANGE

            var creatingUser = preparingSystemUser();
            var creatingSurveyTemplate = PreparingSurveyTemplate2();
            var creatingSurveyTemplateEntity = new EntityReference("cmc_staffsurveytemplate", creatingSurveyTemplate.Id);
            var creatingAcademicPeriod = PreparingAcademicPeriod();
            var creatingStaffCourse = PreparingStaffCourse(creatingAcademicPeriod, creatingUser.ToEntityReference());
          
            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>
            {
                creatingSurveyTemplate,
                creatingStaffCourse,
                creatingAcademicPeriod
            });

            var entitycollection = new EntityCollection();
            entitycollection.Entities.Add(creatingStaffCourse);

            #endregion

            #region ACT

            var mockServiceProvider = InitializeMockService(xrmFakedContext, null, Operation.Create);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            var mockLogger = new Mock<ILogger>();
            var mockILanguageService = new Mock<ILanguageService>();
            var mockConfigurationService = new ConfigurationService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
            AddInputParameters(mockServiceProvider, "Target", creatingSurveyTemplateEntity);
            AddInputParameters(mockServiceProvider, "Staffs", entitycollection);
            var staffService = new StaffSurveyService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService(), mockConfigurationService, mockILanguageService.Object);
            staffService.CreateStaffSurveyFromTemplate(mockExecutionContext.Object);

            #endregion

            #region ASSERT

            var fetchXml = $@"<fetch>
                                <entity name='cmc_staffsurvey'>
                                      <all-attributes/>                                               
                                </entity>
                            </fetch>";
            var data = xrmFakedContext.GetFakedOrganizationService().RetrieveMultipleAll(fetchXml)?.Entities?.Count > 0;
            //var data = xrmFakedContext.Data["cmc_staffsurvey"].Count > 0;
            Assert.IsTrue(data);           

            #endregion

        }

        [TestCategory("Plugin"), TestCategory("Negative")]
        [TestMethod]
        public void CreateStaffSurveyFromStaffSurveyTemplate_WhenStaffSurveyStartDateAndStaffSurveyDueDate_EndDateIsGreaterthanDueDate()
        {
            #region ARRANGE

            var creatingUser = preparingSystemUser();
            var creatingSurveyTemplate = PreparingSurveyTemplate3();
            var creatingSurveyTemplateEntity = new EntityReference("cmc_staffsurveytemplate", creatingSurveyTemplate.Id);
            var creatingAcademicPeriod = PreparingAcademicPeriod();
            var creatingStaffCourse = PreparingStaffCourse(creatingAcademicPeriod, creatingUser.ToEntityReference());

            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>
            {
                creatingSurveyTemplate,
                creatingStaffCourse,
                creatingAcademicPeriod
            });

            var entitycollection = new EntityCollection();
            entitycollection.Entities.Add(creatingStaffCourse);

            #endregion

            #region ACT

            var mockServiceProvider = InitializeMockService(xrmFakedContext, null, Operation.Create);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            var mockLogger = new Mock<ILogger>();
            var mockILanguageService = new Mock<ILanguageService>();
            var mockConfigurationService = new ConfigurationService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
            AddInputParameters(mockServiceProvider, "Target", creatingSurveyTemplateEntity);
            AddInputParameters(mockServiceProvider, "Staffs", entitycollection);
            var staffService = new StaffSurveyService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService(), mockConfigurationService, mockILanguageService.Object);
            staffService.CreateStaffSurveyFromTemplate(mockExecutionContext.Object);

            #endregion

            #region ASSERT
            //Negavative secaniro cmc_staffsurvery is not creating
            //var data = xrmFakedContext.Data["cmc_staffsurvey"];
            //Assert.AreEqual(0, xrmFakedContext.Data["cmc_staffsurvey"].Count);

            #endregion

        }

        [TestMethod]
        [TestCategory("Plugin"), TestCategory("Negative")]
        public void CreateStaffSurveyFromStaffSurveyTemplate_WhenStaffSurveyStartDateAndStaffSurveyDueDate_CalculatedType_StartAndDuedatehaveAcademicPeriodStartDateNULL()
        {
            #region ARRANGE
            var creatingUser = preparingSystemUser();
            var creatingSurveyTemplate = PreparingSurveyTemplate_StartAndDueDateCalculated_Field_StartDateOFAcademicPeriod();
            var creatingSurveyTemplateEntity = new EntityReference("cmc_staffsurveytemplate", creatingSurveyTemplate.Id);
            var creatingAcademicPeriod = PreparingAcademicPeriodHavingStartDateIsNULL();
            var creatingStaffCourse = PreparingStaffCourse(creatingAcademicPeriod, creatingUser.ToEntityReference());

            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>
            {
                creatingSurveyTemplate,
                creatingStaffCourse,
                creatingAcademicPeriod
            });

            var entitycollection = new EntityCollection();
            entitycollection.Entities.Add(creatingStaffCourse);

            #endregion

            #region ACT

            var mockServiceProvider = InitializeMockService(xrmFakedContext, null, Operation.Create);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            var mockLogger = new Mock<ILogger>();
            var mockILanguageService = new Mock<ILanguageService>();
            var mockConfigurationService = new ConfigurationService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
            AddInputParameters(mockServiceProvider, "Target", creatingSurveyTemplateEntity);
            AddInputParameters(mockServiceProvider, "Staffs", entitycollection);
            var staffService = new StaffSurveyService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService(), mockConfigurationService, mockILanguageService.Object);
            staffService.CreateStaffSurveyFromTemplate(mockExecutionContext.Object);

            #endregion

            #region ASSERT
            var mockPluginExecutionContext = (IPluginExecutionContext)mockServiceProvider.Object.GetService(typeof(IPluginExecutionContext));
            var response = mockPluginExecutionContext.OutputParameters["staffSurveyResponse"];

            JavaScriptSerializer json_serializer = new JavaScriptSerializer();
            var routes_list = json_serializer.DeserializeObject(response.ToString()) as Dictionary<string, object>;
            object name;
            (routes_list).TryGetValue("failedStaffCourses", out name);
            Assert.AreEqual(((object[])name)[0], "TestStaffCourse");
            #endregion
        }
        [TestMethod]
        [TestCategory("Plugin"), TestCategory("Negative")]
        public void CreateStaffSurveyFromStaffSurveyTemplate_WhenStaffSurveyStartDateAndStaffSurveyDueDate_CalculatedType_StartAndDuedatehaveAcademicPeriodEndateNULL()
        {

            #region ARRANGE
            var creatingUser = preparingSystemUser();
            var creatingSurveyTemplate = PreparingSurveyTemplate_StartAndDueDateCalculated_Field_EndtDateofAcademicPeriod();
            var creatingSurveyTemplateEntity = new EntityReference("cmc_staffsurveytemplate", creatingSurveyTemplate.Id);
            var creatingAcademicPeriod = PreparingAcademicPeriodHavingEndDateIsNULL();
            var creatingStaffCourse = PreparingStaffCourse(creatingAcademicPeriod, creatingUser.ToEntityReference());

            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>
            {
                creatingSurveyTemplate,
                creatingStaffCourse,
                creatingAcademicPeriod
            });

            var entitycollection = new EntityCollection();
            entitycollection.Entities.Add(creatingStaffCourse);

            #endregion

            #region ACT
            var mockServiceProvider = InitializeMockService(xrmFakedContext, null, Operation.Create);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            var mockLogger = new Mock<ILogger>();
            var mockILanguageService = new Mock<ILanguageService>();
            var mockConfigurationService = new ConfigurationService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
            AddInputParameters(mockServiceProvider, "Target", creatingSurveyTemplateEntity);
            AddInputParameters(mockServiceProvider, "Staffs", entitycollection);
            var staffService = new StaffSurveyService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService(), mockConfigurationService, mockILanguageService.Object);
            staffService.CreateStaffSurveyFromTemplate(mockExecutionContext.Object);

            #endregion

            #region ASSERT

            var mockPluginExecutionContext = (IPluginExecutionContext)mockServiceProvider.Object.GetService(typeof(IPluginExecutionContext));
            var response = mockPluginExecutionContext.OutputParameters["staffSurveyResponse"];

            JavaScriptSerializer json_serializer = new JavaScriptSerializer();
            var routes_list = json_serializer.DeserializeObject(response.ToString()) as Dictionary<string, object>;
            object name;
            (routes_list).TryGetValue("failedStaffCourses", out name);
            Assert.AreEqual(((object[])name)[0], "TestStaffCourse");
            #endregion
        }

        [TestCategory("Plugin"), TestCategory("Positive")]
        [TestMethod]
        public void CreateStaffSurveyFromStaffSurveyTemplate_WhenStaffSurveyStartDateAndStaffSurveyDueDate_Duedatecalculationfield_IsAssignmentDate()
        {
            #region ARRANGE

            var creatingUser = preparingSystemUser();
            var creatingSurveyTemplate = PreparingSurveyTemplateDuedatecalculationfield();
            var creatingSurveyTemplateEntity = new EntityReference("cmc_staffsurveytemplate", creatingSurveyTemplate.Id);
            var creatingAcademicPeriod = PreparingAcademicPeriod();
            var creatingStaffCourse = PreparingStaffCourse(creatingAcademicPeriod, creatingUser.ToEntityReference());

            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>
            {
                creatingSurveyTemplate,
                creatingStaffCourse,
                creatingAcademicPeriod
            });

            var entitycollection = new EntityCollection();
            entitycollection.Entities.Add(creatingStaffCourse);

            #endregion

            #region ACT

            var mockServiceProvider = InitializeMockService(xrmFakedContext, null, Operation.Create);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            var mockLogger = new Mock<ILogger>();
            var mockILanguageService = new Mock<ILanguageService>();
            var mockConfigurationService = new ConfigurationService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
            AddInputParameters(mockServiceProvider, "Target", creatingSurveyTemplateEntity);
            AddInputParameters(mockServiceProvider, "Staffs", entitycollection);
            var staffService = new StaffSurveyService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService(), mockConfigurationService, mockILanguageService.Object);
            staffService.CreateStaffSurveyFromTemplate(mockExecutionContext.Object);

            #endregion

            #region ASSERT
            var fetchXml = $@"<fetch>
                                <entity name='cmc_staffsurvey'>
                                      <all-attributes/>                                               
                                </entity>
                            </fetch>";
            var data = xrmFakedContext.GetFakedOrganizationService().RetrieveMultipleAll(fetchXml)?.Entities?.Count > 0;
            //var data = xrmFakedContext.Data["cmc_staffsurvey"].Count > 0;
            Assert.IsTrue(data);

            #endregion

        }

        [TestCategory("Plugin"), TestCategory("Positive")]
        [TestMethod]
        public void CreateStaffSurveyFromStaffSurveyTemplate_WhenStaffSurveyStartDateAndStaffSurveyDueDate_Duedatecalculationfield_IsStartDateofAcademicPeriod()
        {
            #region ARRANGE

            var creatingUser = preparingSystemUser();
            var creatingSurveyTemplate = PreparingSurveyTemplateDuedatecalculationfield1();
            var creatingSurveyTemplateEntity = new EntityReference("cmc_staffsurveytemplate", creatingSurveyTemplate.Id);
            var creatingAcademicPeriod = PreparingAcademicPeriod();
            var creatingStaffCourse = PreparingStaffCourse(creatingAcademicPeriod, creatingUser.ToEntityReference());

            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>
            {
                creatingSurveyTemplate,
                creatingStaffCourse,
                creatingAcademicPeriod
            });

            var entitycollection = new EntityCollection();
            entitycollection.Entities.Add(creatingStaffCourse);

            #endregion

            #region ACT

            var mockServiceProvider = InitializeMockService(xrmFakedContext, null, Operation.Create);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            var mockLogger = new Mock<ILogger>();
            var mockILanguageService = new Mock<ILanguageService>();
            var mockConfigurationService = new ConfigurationService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
            AddInputParameters(mockServiceProvider, "Target", creatingSurveyTemplateEntity);
            AddInputParameters(mockServiceProvider, "Staffs", entitycollection);
            var staffService = new StaffSurveyService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService(), mockConfigurationService, mockILanguageService.Object);
            staffService.CreateStaffSurveyFromTemplate(mockExecutionContext.Object);

            #endregion

            #region ASSERT

            var fetchXml = $@"<fetch>
                                <entity name='cmc_staffsurvey'>
                                      <all-attributes/>                                               
                                </entity>
                            </fetch>";
            var data = xrmFakedContext.GetFakedOrganizationService().RetrieveMultipleAll(fetchXml)?.Entities?.Count > 0;
            //var data = xrmFakedContext.Data["cmc_staffsurvey"].Count > 0;
            Assert.IsTrue(data);

            #endregion

        }

        #region DATA PREPARATION

        private SystemUser preparingSystemUser()
        {
            var user = new SystemUser
            {
                SystemUserId=Guid.NewGuid()
            };
            return user;
        }
        private Entity PreparingAcademicPeriod()
        {
            var academicPeriod = new mshied_academicperiod()
            {
                mshied_academicperiodId = Guid.NewGuid(),
                mshied_StartDate= DateTime.Now,
                mshied_EndDate= DateTime.Now.AddDays(8),
            };
            return academicPeriod;
        }
        private cmc_staffsurveytemplate PreparingSurveyTemplate()   
        {
            var surveyTemplateEntity = new cmc_staffsurveytemplate()
            {
                cmc_staffsurveytemplateId = Guid.NewGuid(),
                cmc_staffsurveytemplatename = "TestSurveyTemplate",
                cmc_startdatecalculationtype = cmc_staffsurveytemplate_cmc_startdatecalculationtype.Static,
                cmc_startdatestatic = DateTime.Now,
                cmc_startdatenumberofdays = 2,
                cmc_startdatecalculationfield = cmc_staffsurveytemplate_cmc_startdatecalculationfield.AssignmentDate,
                cmc_startdatedaysdirection = cmc_staffsurveytemplate_cmc_startdatedaysdirection.After,
                cmc_duedatecalculationtype = cmc_staffsurveytemplate_cmc_duedatecalculationtype.Static,
                cmc_duedatestatic = DateTime.Now.AddDays(5)
            };
            return surveyTemplateEntity;
        }
        private cmc_staffsurveytemplate PreparingSurveyTemplate1()
        {
            var surveyTemplateEntity = new cmc_staffsurveytemplate()
            {
                cmc_staffsurveytemplateId = Guid.NewGuid(),
                cmc_staffsurveytemplatename = "TestSurveyTemplate",
                cmc_startdatecalculationtype = cmc_staffsurveytemplate_cmc_startdatecalculationtype.Calculated,
                cmc_startdatenumberofdays = 2,
                cmc_startdatecalculationfield = cmc_staffsurveytemplate_cmc_startdatecalculationfield.StartDateofAcademicPeriod,
                cmc_startdatedaysdirection = cmc_staffsurveytemplate_cmc_startdatedaysdirection.After,
                cmc_duedatecalculationtype = cmc_staffsurveytemplate_cmc_duedatecalculationtype.Static,
                cmc_duedatestatic = DateTime.Now.AddDays(5)
            };
            return surveyTemplateEntity;
        }
        private cmc_staffsurveytemplate PreparingSurveyTemplate2()
        {
            var surveyTemplateEntity = new cmc_staffsurveytemplate()
            {
                cmc_staffsurveytemplateId = Guid.NewGuid(),
                cmc_staffsurveytemplatename = "TestSurveyTemplate",
                cmc_startdatecalculationtype = cmc_staffsurveytemplate_cmc_startdatecalculationtype.Calculated,
                cmc_startdatenumberofdays = 2,
                cmc_startdatecalculationfield = cmc_staffsurveytemplate_cmc_startdatecalculationfield.EndDateofAcademicPeriod,
                cmc_startdatedaysdirection = cmc_staffsurveytemplate_cmc_startdatedaysdirection.After,
                cmc_duedatecalculationtype = cmc_staffsurveytemplate_cmc_duedatecalculationtype.Static,
                cmc_duedatestatic = DateTime.Now.AddDays(15),
              
            };
            return surveyTemplateEntity;
        }
        private cmc_staffsurveytemplate PreparingSurveyTemplate3()
        {
            var surveyTemplateEntity = new cmc_staffsurveytemplate()
            {
                cmc_staffsurveytemplateId = Guid.NewGuid(),
                cmc_staffsurveytemplatename = "TestSurveyTemplate",
                cmc_startdatecalculationtype = cmc_staffsurveytemplate_cmc_startdatecalculationtype.Calculated,
                cmc_startdatenumberofdays = 2,
                cmc_startdatecalculationfield = cmc_staffsurveytemplate_cmc_startdatecalculationfield.EndDateofAcademicPeriod,
                cmc_startdatedaysdirection = cmc_staffsurveytemplate_cmc_startdatedaysdirection.After,
                cmc_duedatecalculationtype = cmc_staffsurveytemplate_cmc_duedatecalculationtype.Static,
                cmc_duedatestatic = DateTime.Now.AddDays(5),

            };
            return surveyTemplateEntity;
        }
        private cmc_staffsurveytemplate PreparingSurveyTemplateDuedatecalculationfield()
        {
            var surveyTemplateEntity = new cmc_staffsurveytemplate()
            {
                cmc_staffsurveytemplateId = Guid.NewGuid(),
                cmc_staffsurveytemplatename = "TestSurveyTemplate",
                cmc_startdatecalculationtype = cmc_staffsurveytemplate_cmc_startdatecalculationtype.Calculated,
                //cmc_startdatestatic = DateTime.Now.AddDays(-1),
                //cmc_startdatenumberofdays = 2,
                cmc_startdatecalculationfield = cmc_staffsurveytemplate_cmc_startdatecalculationfield.StartDateofAcademicPeriod,
                cmc_startdatedaysdirection = cmc_staffsurveytemplate_cmc_startdatedaysdirection.After,
                cmc_duedatecalculationtype = cmc_staffsurveytemplate_cmc_duedatecalculationtype.Calculated,
                cmc_duedatecalculationfield = cmc_staffsurveytemplate_cmc_duedatecalculationfield.AssignmentDate,
                cmc_duedatedaysdirection = cmc_staffsurveytemplate_cmc_duedatedaysdirection.After,
                cmc_duedatenumberofdays = 4,
                cmc_duedatestatic = DateTime.Now.AddDays(3)
            };
            return surveyTemplateEntity;
        }
        private cmc_staffsurveytemplate PreparingSurveyTemplateDuedatecalculationfield1()
        {
            var surveyTemplateEntity = new cmc_staffsurveytemplate()
            {
                cmc_staffsurveytemplateId = Guid.NewGuid(),
                cmc_staffsurveytemplatename = "TestSurveyTemplate",
                cmc_startdatecalculationtype = cmc_staffsurveytemplate_cmc_startdatecalculationtype.Calculated,
                //cmc_startdatestatic = DateTime.Now.AddDays(-1),
                //cmc_startdatenumberofdays = 2,
                cmc_startdatecalculationfield = cmc_staffsurveytemplate_cmc_startdatecalculationfield.StartDateofAcademicPeriod,
                cmc_startdatedaysdirection = cmc_staffsurveytemplate_cmc_startdatedaysdirection.After,
                cmc_duedatecalculationtype = cmc_staffsurveytemplate_cmc_duedatecalculationtype.Calculated,
                cmc_duedatecalculationfield = cmc_staffsurveytemplate_cmc_duedatecalculationfield.StartDateofAcademicPeriod,
                cmc_duedatedaysdirection = cmc_staffsurveytemplate_cmc_duedatedaysdirection.After,
                cmc_duedatenumberofdays = 4,
                cmc_duedatestatic = DateTime.Now.AddDays(3)
            };
            return surveyTemplateEntity;
        }
        private cmc_staffsurveytemplate PreparingSurveyTemplate_StartAndDueDateCalculated_Field_StartDateOFAcademicPeriod()
        {
            var surveyTemplateEntity = new cmc_staffsurveytemplate()
            {
                cmc_staffsurveytemplateId = Guid.NewGuid(),
                cmc_staffsurveytemplatename = "TestSurveyTemplate",
                cmc_startdatecalculationtype = cmc_staffsurveytemplate_cmc_startdatecalculationtype.Calculated,
                cmc_startdatenumberofdays = 2,
                cmc_startdatecalculationfield = cmc_staffsurveytemplate_cmc_startdatecalculationfield.StartDateofAcademicPeriod,
                cmc_startdatedaysdirection = cmc_staffsurveytemplate_cmc_startdatedaysdirection.Before,
                cmc_duedatecalculationtype = cmc_staffsurveytemplate_cmc_duedatecalculationtype.Calculated,
                cmc_duedatecalculationfield = cmc_staffsurveytemplate_cmc_duedatecalculationfield.StartDateofAcademicPeriod,
                cmc_duedatedaysdirection = cmc_staffsurveytemplate_cmc_duedatedaysdirection.Before,
                cmc_duedatenumberofdays = 2
            };
            return surveyTemplateEntity;
        }
        private mshied_academicperiod PreparingAcademicPeriodHavingStartDateIsNULL()
        {
            var academicPeriod = new mshied_academicperiod()
            {
                mshied_academicperiodId = Guid.NewGuid(),
                mshied_StartDate = null,
                mshied_EndDate = DateTime.Now.AddDays(8),
            };
            return academicPeriod;
        }

        private cmc_staffsurveytemplate PreparingSurveyTemplate_StartAndDueDateCalculated_Field_EndtDateofAcademicPeriod()   // startDate Calculation, StartDateCalculationField AssignmentDate,DueDate Static
        {
            var surveyTemplateEntity = new cmc_staffsurveytemplate()
            {
                cmc_staffsurveytemplateId = Guid.NewGuid(),
                cmc_staffsurveytemplatename = "TestSurveyTemplate",
                cmc_startdatecalculationtype = cmc_staffsurveytemplate_cmc_startdatecalculationtype.Calculated,
                cmc_startdatenumberofdays = 2,
                cmc_startdatecalculationfield = cmc_staffsurveytemplate_cmc_startdatecalculationfield.EndDateofAcademicPeriod,
                cmc_startdatedaysdirection = cmc_staffsurveytemplate_cmc_startdatedaysdirection.After,
                cmc_duedatecalculationtype = cmc_staffsurveytemplate_cmc_duedatecalculationtype.Calculated,
                cmc_duedatecalculationfield = cmc_staffsurveytemplate_cmc_duedatecalculationfield.EndDateofAcademicPeriod,
                cmc_duedatedaysdirection = cmc_staffsurveytemplate_cmc_duedatedaysdirection.After,
                cmc_duedatenumberofdays = 2
            };
            return surveyTemplateEntity;
        }
        private Entity PreparingAcademicPeriodHavingEndDateIsNULL()
        {
            var academicPeriod = new mshied_academicperiod()
            {
                mshied_academicperiodId = Guid.NewGuid(),
                mshied_StartDate = DateTime.Now,
                mshied_EndDate = null,
            };
            return academicPeriod;
        }

        private mshied_coursesection PreparingStaffCourse(Entity academicInstance,EntityReference staffInstance)
        {
            var staffCourseEntity = new mshied_coursesection()
            {
                mshied_coursesectionId = Guid.NewGuid(),
                mshied_name = "TestStaffCourse",
                mshied_AcademicPeriodId= new EntityReference("mshied_academicperiodid", academicInstance.Id),
                statecode =mshied_coursesectionState.Active,
                mshied_InstructorId= staffInstance,
            };
            return staffCourseEntity;
        }

        #endregion
    }
}
