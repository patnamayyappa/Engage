using System;
using System.Collections.Generic;
using Cmc.Core.Xrm.ServerExtension.Logging;
using Cmc.Engage.Common.Plugins.Tests.Utilities;
using Cmc.Engage.Models;
using FakeXrmEasy;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Moq;

namespace Cmc.Engage.Common.Tests.Contact.Activity
{
    [TestClass]
    public class AssignSuccessPlanActivityTest : XrmUnitTestBase
    {
        [TestCategory("Activity"), TestCategory("Positive")]
        [TestMethod]

        //Validating that Same SuccessPlan Should Not be Assinged to a Contact and We get Dialog Failure Messsage
        public void AssignSuccessPlanActivity_CreatingSuccessPlansForSelectedStudentsHavingsameSuccessPlan()
        {
            #region ARRANGE

            var creatingAcademicPeriod = PrepareAcademicPeriod();
            var contactInstance = PrepareContact(creatingAcademicPeriod.ToEntityReference());
            var successPlanInstance = PreSuccessplantemplate();
            var creatingSuccessPlan = PrepareSuccessPlan(contactInstance, successPlanInstance);
            var creatingSuccessPlanTodo = PreparingSuccessPlanTodoTemplate(successPlanInstance.Id);

            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>()
            {
                contactInstance,
                successPlanInstance,
                creatingSuccessPlan,
                creatingSuccessPlanTodo
            });

            #endregion

            #region ACT

            var mockLogger = new Mock<ILogger>();
            var mockILanguageService = new Mock<ILanguageService>();
            var mockBingMapService = InitializeBingMapMockService();

            var mockConfigurationService = new ConfigurationService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
            //var mockBingServices = new BingMapService(mockLogger.Object,xrmFakedContext.GetFakedOrganizationService(), mockConfigurationService);
            var mockContactService = new ContactService(mockLogger.Object, mockBingMapService.Object, xrmFakedContext.GetFakedOrganizationService(), mockConfigurationService, mockILanguageService.Object);
            mockContactService.AssignSuccessPlan(contactInstance.ToEntityReference(), successPlanInstance.ToEntityReference());

            #endregion

            #region ASSERT



            #endregion


        }


        private mshied_academicperiod PrepareAcademicPeriod()
        {
            var academicPeriod = new mshied_academicperiod()
            {
                mshied_academicperiodId = Guid.NewGuid(),
                mshied_name = "TestAcademicPeriod",
                mshied_EndDate = DateTime.Today.AddDays(5),
                mshied_StartDate = DateTime.Today
            };
            return academicPeriod;
        }
        private Models.Contact PrepareContact(EntityReference academicInstanse)
        {
            var studentEntity = new Models.Contact()
            {
                Id = Guid.NewGuid(),
                ["fullname"] = "Ankur k",
                mshied_CurrentAcademicPeriodId = academicInstanse

            };
            return studentEntity;
        }

        private cmc_successplan PrepareSuccessPlan(Entity contact, Entity successPlanTemplate)
        {
            var entitySuccessPlan = new cmc_successplan()
            {
                cmc_successplanId = Guid.NewGuid(),
                cmc_successplantemplateid = successPlanTemplate.ToEntityReference(),
                cmc_assignedtoid = contact.ToEntityReference()
            };
            return entitySuccessPlan;
        }
        private cmc_successplantodotemplate PreparingSuccessPlanTodoTemplate(Guid entitySuccessPlanTemplate)
        {
            var todo = new cmc_successplantodotemplate()
            {
                cmc_successplantodotemplateId = entitySuccessPlanTemplate,
                cmc_successplantodotemplatename = "Test Success Plan To Do Template"
            };
            return todo;
        }

        private cmc_successplantemplate PreSuccessplantemplate()
        {
            var successPlanTemplateEntity = new cmc_successplantemplate()
            {
                Id = Guid.NewGuid(),
                cmc_successplantemplatename = "Test Success Plan template"
            };
            return successPlanTemplateEntity;
        }


    }
}
