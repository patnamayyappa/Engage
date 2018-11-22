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
    public class StaffSurveyPostOperation_CreateStaffSurveyResponse : XrmUnitTestBase
    {
        [TestCategory("Plugin"), TestCategory("Positive")]
        [TestMethod]
        public void StaffSurveyPostOperationService_AssociateQuestions()
        {
            #region Arrange
            var contact = PreparingContact();
            var contact1 = PreparingContact();
            var staffCourse = PreparingStaffCourse();
            var courseHistory = PrepareCourseHistory(contact.ToEntityReference(), staffCourse.ToEntityReference());
            var courseHistory1 = PrepareCourseHistory(contact.ToEntityReference(), staffCourse.ToEntityReference());
            var createStaffSurveyTemplate = CreateStaffSurveyTemplate();
            var createStaffSurveyTemplateQuestion = CreateStaffSurveyQuestion(createStaffSurveyTemplate.ToEntityReference());
            var staffSurvey = CreateStaffSurvey(staffCourse.ToEntityReference(), createStaffSurveyTemplate.ToEntityReference());

            var xrmFakedContext = new XrmFakedContext();
                     
            xrmFakedContext.Initialize(new List<Entity>()
            {
                courseHistory,
                courseHistory1,
                staffCourse,
                staffSurvey,
                contact,
                contact1,
                createStaffSurveyTemplate,
                createStaffSurveyTemplateQuestion
            });
            var xrmFakedContext2 = new XrmFakedContext();


            xrmFakedContext.AddRelationship("cmc_staffsurvey_cmc_staffsurveyquestion", new XrmFakedRelationship
            {
                IntersectEntity = "cmc_staffsurvey",
                RelationshipType = XrmFakedRelationship.enmFakeRelationshipType.ManyToMany,
                Entity1LogicalName = cmc_staffsurvey.EntityLogicalName,
                Entity1Attribute = "cmc_staffsurvey",
                Entity2LogicalName = cmc_staffsurveyquestion.EntityLogicalName,
                Entity2Attribute = "cmc_staffsurveyquestion"
            });

            xrmFakedContext.AddRelationship("cmc_contact_coursehistory_studentid", new XrmFakedRelationship
            {
                RelationshipType = XrmFakedRelationship.enmFakeRelationshipType.OneToMany,
                Entity1LogicalName = Contact.EntityLogicalName,
                Entity1Attribute = "contactid",
                Entity2LogicalName = "mshied_coursehistory",
                Entity2Attribute = "mshied_studentid"
            });

            xrmFakedContext.AddRelationship("cmc_staffcourse_cmc_coursehistory", new XrmFakedRelationship
            {
                RelationshipType = XrmFakedRelationship.enmFakeRelationshipType.OneToMany,
                Entity1LogicalName = mshied_coursesection.EntityLogicalName,
                Entity1Attribute = "mshied_coursesectionid",
                Entity2LogicalName = "mshied_coursehistory",
                Entity2Attribute = "mshied_coursesectionid"
            });
                        
            EntityReferenceCollection courseHistoryCollection = new EntityReferenceCollection();
            courseHistoryCollection.Add(new EntityReference("mshied_coursehistory", courseHistory.Id));
            courseHistoryCollection.Add(new EntityReference("mshied_coursehistory", courseHistory1.Id));

            Relationship studentRelationship = new Relationship("cmc_contact_coursehistory_studentid");
            xrmFakedContext.GetFakedOrganizationService().Associate(contact.LogicalName, contact.Id, studentRelationship, courseHistoryCollection);
            
            Relationship staffCourseRelationship = new Relationship("cmc_staffcourse_cmc_coursehistory");
            xrmFakedContext.GetFakedOrganizationService().Associate(staffCourse.LogicalName, staffCourse.Id, staffCourseRelationship, courseHistoryCollection);


            #endregion Arrange

            #region ACT          

            var mockLogger = new Mock<ILogger>();
            var mockILanguageService = new Mock<ILanguageService>();

            var mockServiceProvider = InitializeMockService(xrmFakedContext, staffSurvey, Operation.Create);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            var mockConfigurationService = new ConfigurationService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());

            var staffSurveyService = new StaffSurveyService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService(), mockConfigurationService,mockILanguageService.Object);
            staffSurveyService.PerformStaffSurveyPostOperationLogic(mockExecutionContext.Object);
            #endregion

            #region Assert

            /*This plugin will be called post Create action 
            Post creation of Staff Survey Survey Response object should be created.*/

            var data = xrmFakedContext.Data["cmc_staffsurveyresponse"].Count > 0;
            Assert.IsTrue(data);

            #endregion Assert
        }

        private mshied_coursesection PreparingStaffCourse()
        {
            return new mshied_coursesection()
            {
                mshied_coursesectionId = Guid.NewGuid(),
                mshied_name = "Test Staff Course",
                statecode = mshied_coursesectionState.Active
                };
        }
        
        private Entity CreateStaffSurveyTemplate()
        {
            return new cmc_staffsurveytemplate
            {
                Id = Guid.NewGuid(),
                cmc_staffsurveytemplatename = "Test Template",
                cmc_description = "Test Description"
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
        
        private Contact PreparingContact()
        {
            return new Contact
            {
                ContactId = Guid.NewGuid(),
                ["fullname"] = "Ankur k",
                
            };
        }
        private Entity PrepareCourseHistory(EntityReference contact, EntityReference staffCourse)
        {
            var courseHistory =  new Entity("mshied_coursehistory", Guid.NewGuid())
            {
               // ["mshied_coursehistoryid"] = Guid.NewGuid(),
                ["mshied_name"] ="Test Course History",
               // ["mshied_coursesectionid"]=staffCourse,
              //  ["mshied_studentid"] =contact
            };
            return courseHistory;
        }

        private Entity CreateStaffSurvey(EntityReference staffCourse, EntityReference staffSurveyTemplate)
        {
            return new cmc_staffsurvey
            {
                Id = Guid.NewGuid(),
                cmc_staffsurveyname = "Test Survey",
                statecode = cmc_staffsurveyState.Active,
                cmc_coursesectionid = staffCourse,
                cmc_staffsurveytemplateid =staffSurveyTemplate
            };
        }
    }
}

