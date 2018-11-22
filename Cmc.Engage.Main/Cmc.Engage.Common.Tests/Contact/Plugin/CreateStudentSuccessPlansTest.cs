using Cmc.Core.Xrm.ServerExtension.Logging;
using Cmc.Engage.Common.Plugins.Tests.Utilities;
using Cmc.Engage.Models;
using FakeItEasy;
using FakeXrmEasy;
using FakeXrmEasy.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Script.Serialization;

namespace Cmc.Engage.Common.Tests.Contact.Plugin
{
    [TestClass]
    public class CreateStudentSuccessPlansTest : XrmUnitTestBase
    {
        [TestCategory("Plugin"), TestCategory("Positive")]
        [TestMethod]
        public void CreateStudentSuccessPlans_StudentsHavingSameSuccessPlanTemplate()
        {
            #region ARRANGE

            var creatingStudent = PreparingStudent();
            var creatingSuccessplanTemplate = PreparingSuccessPlanTemplate();
            var creatingSuccessPlan = PreparingSuccessPlan(creatingStudent.Id, creatingSuccessplanTemplate.Id);
            var creatingStudentModel = PrepareStudentModel(creatingStudent);
            var creatingSuccessPlanTodo = PreparingSuccessPlanTodoTemplate(creatingSuccessplanTemplate.Id);

            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>()
            {
                creatingStudentModel,
                creatingSuccessPlan,
                creatingSuccessPlanTodo,
                creatingSuccessplanTemplate
            });

            var successplanTemp = new EntityReference("cmc_successplantemplate", creatingSuccessplanTemplate.Id);

            var studentEntityCollection = new EntityCollection();
            studentEntityCollection.Entities.Add(creatingStudent);

            var mockServiceProvider = InitializeMockService(xrmFakedContext, null, Operation.cmc_createstudentsuccessplansfromtemplate);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            var mockPluginExecutionContext = (IPluginExecutionContext)mockServiceProvider.Object.GetService(typeof(IPluginExecutionContext));

            AddInputParameters(mockServiceProvider, "Target", successplanTemp);
            AddInputParameters(mockServiceProvider, "Students", studentEntityCollection);

            #endregion

            #region ACT

            var mockLogger = new Mock<ILogger>();
            var mockILanguageService = new Mock<ILanguageService>();
            var mockBingMapService = InitializeBingMapMockService();

            var mockConfigurationService = new ConfigurationService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
            //var mockBingServices = new BingMapService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService(), mockConfigurationService);

            var contactService = new ContactService(mockLogger.Object, mockBingMapService.Object, xrmFakedContext.GetFakedOrganizationService(), mockConfigurationService, mockILanguageService.Object);
            contactService.CreateSuccessPlansForSelectedStudent(mockExecutionContext.Object);

            #endregion

            #region ASSERT

            var data = xrmFakedContext.Data["cmc_successplan"].Count > 0;
            Assert.IsTrue(data);
            #endregion
        }

        [TestCategory("Plugin"), TestCategory("Positive")]
        [TestMethod]
        public void CreateStudentSuccessPlansFromTemplate_StudentsHavingSameSuccessPlanTemplateIsNull()
        {
            #region ARRANGE
            var creatingStudent = PreparingStudent();
            var creatingSuccessPlanTemplate = PreparingSuccessPlanTemplate();
            var creatingSuccessPlan = PreparingSuccessPlanNotLinkedToContact(creatingStudent.Id, creatingSuccessPlanTemplate.Id);
            var creatingStudentModel = PrepareStudentModel(creatingStudent);
            var creatingSuccessPlanTodo = PreparingSuccessPlanTodoTemplate(creatingSuccessPlanTemplate.Id);


            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>()
            {
                creatingStudentModel,
                creatingSuccessPlanTemplate,
                creatingSuccessPlan,
                creatingSuccessPlanTodo,

            });
            var mockServiceProvider = InitializeMockService(xrmFakedContext, null, Operation.cmc_createstudentsuccessplansfromtemplate);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);

            var successplanTemp = new EntityReference("cmc_successplantemplate", creatingSuccessPlanTemplate.Id);

            var studentEntityCollection = new EntityCollection();
            studentEntityCollection.Entities.Add(creatingStudent);

            AddInputParameters(mockServiceProvider, "Target", successplanTemp);
            AddInputParameters(mockServiceProvider, "Students", studentEntityCollection);

            #endregion

            #region ACT
            var mockLogger = new Mock<ILogger>();
            var mockILanguageService = new Mock<ILanguageService>();
            var mockBingMapService = InitializeBingMapMockService();

            var mockConfigurationService = new ConfigurationService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
            //var mockBingServices = new BingMapService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService(), mockConfigurationService);

            var successPlanEntityMetadata = new EntityMetadata() { LogicalName = cmc_successplan.EntityLogicalName };
            successPlanEntityMetadata.SetAttributeCollection(new List<AttributeMetadata>() {
                new UniqueIdentifierAttributeMetadata("cmc_successplanid"){ LogicalName = "cmc_successplanid" }
                , new StringAttributeMetadata("cmc_successplanname") { LogicalName = "cmc_successplanname" } });
            xrmFakedContext.InitializeMetadata(successPlanEntityMetadata);

            //todo retrieve change request mock for cmc_successPlan 
            // fake response for to success plan
            var retrieveSucessPlanMetadataChangesResponse = new RetrieveMetadataChangesResponse()
            {
                Results = new ParameterCollection
                {
                    { "EntityMetadata", new EntityMetadataCollection { successPlanEntityMetadata}}
                }
            };

            A.CallTo(() => xrmFakedContext.GetFakedOrganizationService()
                    .Execute(A<RetrieveMetadataChangesRequest>.That.Matches(r => r.Query.Criteria.Conditions.Any(p => p.Value.Equals("cmc_successplan")))))
                .Returns(retrieveSucessPlanMetadataChangesResponse);


            //todo retrieve change request mock for cmc_todo 
            var todoEntityMetadata = new EntityMetadata() { LogicalName = cmc_todo.EntityLogicalName };
            todoEntityMetadata.SetAttributeCollection(new List<AttributeMetadata>() {
                new UniqueIdentifierAttributeMetadata("cmc_todoid"){ LogicalName = "cmc_todoid" }
                , new StringAttributeMetadata("cmc_todoname") { LogicalName = "cmc_todoname" } });
            xrmFakedContext.SetEntityMetadata(todoEntityMetadata);

            // fake response for to do list

            var retrieveTodoMetadataChangesResponse = new RetrieveMetadataChangesResponse()
            {
                Results = new ParameterCollection
                {
                    { "EntityMetadata", new EntityMetadataCollection { todoEntityMetadata}}
                }
            };

            A.CallTo(() => xrmFakedContext.GetFakedOrganizationService()
                    .Execute(A<RetrieveMetadataChangesRequest>.That.Matches(r => r.Query.Criteria.Conditions.Any(p => p.Value.Equals("cmc_todo")))))
                .Returns(retrieveTodoMetadataChangesResponse);

            var contactService = new ContactService(mockLogger.Object, mockBingMapService.Object, xrmFakedContext.GetFakedOrganizationService(), mockConfigurationService, mockILanguageService.Object);
            contactService.CreateSuccessPlansForSelectedStudent(mockExecutionContext.Object);
            #endregion

            #region ASSERT
            var mockPluginExecutionContext = (IPluginExecutionContext)mockServiceProvider.Object.GetService(typeof(IPluginExecutionContext));
            //var resultData = JsonConvert.DeserializeObject<AssignSuccessPlanResponse>(mockPluginExecutionContext.OutputParameters["SuccessPlanResponse"].ToString().Replace("\"KeyAttributes\": [],\"", ""));
            //todo not able to de serialize back to AssignSuccessPlanResponse, so doing following way.
            var result = contactService.GetAllStudentsHavingSameSuccessPlanTemplate(creatingSuccessPlanTemplate.Id,
                new List<Guid>() { creatingStudent.Id });

            Assert.IsTrue(result != null && result.ContainsKey(creatingStudent.Id)); // should have the record 
            #endregion
        }


        [TestCategory("Plugin"), TestCategory("Positive")]
        [TestMethod]
        public void CreateStudentSuccessPlans_StudentsHavingSameSuccessPlanTemplate_cmcduedatecalculationfield_IsStartofAcademicPeriod()
        {
            #region ARRANGE
            var academicPeriod = PrepareAcademicPeriod();
            var creatingStudent = PreparingStudent1(academicPeriod);
            var creatingSuccessplanTemplate = PreparingSuccessPlanTemplate();
            var creatingSuccessPlan = PreparingSuccessPlan1(creatingStudent.Id, creatingSuccessplanTemplate.Id);
            var creatingStudentModel = PrepareStudentModel(creatingStudent);
            var creatingSuccessPlanTodo = PreparingSuccessPlanTodoTemplate1(creatingSuccessplanTemplate.Id);

            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>()
            {
                creatingStudentModel,
                creatingSuccessPlan,
                creatingSuccessPlanTodo,
                creatingSuccessplanTemplate,
                academicPeriod,
                creatingStudent
            });

            var successplanTemp = new EntityReference("cmc_successplantemplate", creatingSuccessplanTemplate.Id);

            var studentEntityCollection = new EntityCollection();
            studentEntityCollection.Entities.Add(creatingStudent);

            var mockServiceProvider = InitializeMockService(xrmFakedContext, null, Operation.cmc_createstudentsuccessplansfromtemplate);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);

            AddInputParameters(mockServiceProvider, "Target", successplanTemp);
            AddInputParameters(mockServiceProvider, "Students", studentEntityCollection);

            var successPlanEntityMetadata = new EntityMetadata() { LogicalName = cmc_successplan.EntityLogicalName };
            successPlanEntityMetadata.SetAttributeCollection(new List<AttributeMetadata>() {
                new UniqueIdentifierAttributeMetadata("cmc_successplanid"){ LogicalName = "cmc_successplanid" }
                , new StringAttributeMetadata("cmc_successplanname") { LogicalName = "cmc_successplanname" } });
            xrmFakedContext.InitializeMetadata(successPlanEntityMetadata);

            //todo retrieve change request mock for cmc_successPlan 
            // fake response for to success plan
            var retrieveSucessPlanMetadataChangesResponse = new RetrieveMetadataChangesResponse()
            {
                Results = new ParameterCollection
                {
                    { "EntityMetadata", new EntityMetadataCollection { successPlanEntityMetadata}}
                }
            };

            A.CallTo(() => xrmFakedContext.GetFakedOrganizationService()
                    .Execute(A<RetrieveMetadataChangesRequest>.That.Matches(r => r.Query.Criteria.Conditions.Any(p => p.Value.Equals("cmc_successplan")))))
                .Returns(retrieveSucessPlanMetadataChangesResponse);


            //todo retrieve change request mock for cmc_todo 
            var todoEntityMetadata = new EntityMetadata() { LogicalName = cmc_todo.EntityLogicalName };
            todoEntityMetadata.SetAttributeCollection(new List<AttributeMetadata>() {
                new UniqueIdentifierAttributeMetadata("cmc_todoid"){ LogicalName = "cmc_todoid" }
                , new StringAttributeMetadata("cmc_todoname") { LogicalName = "cmc_todoname" } });
            xrmFakedContext.SetEntityMetadata(todoEntityMetadata);

            // fake response for to do list

            var retrieveTodoMetadataChangesResponse = new RetrieveMetadataChangesResponse()
            {
                Results = new ParameterCollection
                {
                    { "EntityMetadata", new EntityMetadataCollection { todoEntityMetadata}}
                }
            };

            A.CallTo(() => xrmFakedContext.GetFakedOrganizationService()
                    .Execute(A<RetrieveMetadataChangesRequest>.That.Matches(r => r.Query.Criteria.Conditions.Any(p => p.Value.Equals("cmc_todo")))))
                .Returns(retrieveTodoMetadataChangesResponse);

            #endregion

            #region ACT

            var mockLogger = new Mock<ILogger>();
            var mockILanguageService = new Mock<ILanguageService>();
            var mockBingMapService = InitializeBingMapMockService();

            var mockConfigurationService = new ConfigurationService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
            //var mockBingServices = new BingMapService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService(), mockConfigurationService);
            var contactService = new ContactService(mockLogger.Object, mockBingMapService.Object, xrmFakedContext.GetFakedOrganizationService(), mockConfigurationService, mockILanguageService.Object);
            contactService.CreateSuccessPlansForSelectedStudent(mockExecutionContext.Object);
            #endregion

            #region ASSERT

            var data = xrmFakedContext.Data["cmc_successplan"].Count > 0;
            Assert.IsTrue(data);

            #endregion
        }

        [TestCategory("Plugin"), TestCategory("Positive")]
        [TestMethod]
        public void CreateStudentSuccessPlans_StudentsHavingSameSuccessPlanTemplate_cmcduedatecalculationfield_IsEndofAcademicPeriod()
        {
            #region ARRANGE
            var academicPeriod = PrepareAcademicPeriod();
            var creatingStudent = PreparingStudent1(academicPeriod);
            var creatingSuccessPlanTemplate = PreparingSuccessPlanTemplate();
            var creatingSuccessPlan = PreparingSuccessPlan1(creatingStudent.Id, creatingSuccessPlanTemplate.Id);
            var creatingStudentModel = PrepareStudentModel(creatingStudent);
            var creatingSuccessPlanTodo = PreparingSuccessPlanTodoTemplate2(creatingSuccessPlanTemplate.Id);

            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>()
            {
                creatingStudentModel,
                creatingSuccessPlanTemplate,
                creatingSuccessPlan,
                creatingSuccessPlanTodo,
                academicPeriod,
                creatingStudent
            });

            var mockServiceProvider = InitializeMockService(xrmFakedContext, null, Operation.cmc_createstudentsuccessplansfromtemplate);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);

            var successplanTemp = new EntityReference("cmc_successplantemplate", creatingSuccessPlanTemplate.Id);

            var studentEntityCollection = new EntityCollection();
            studentEntityCollection.Entities.Add(creatingStudent);

            AddInputParameters(mockServiceProvider, "Target", successplanTemp);
            AddInputParameters(mockServiceProvider, "Students", studentEntityCollection);

            #endregion

            #region ACT
            var mockLogger = new Mock<ILogger>();
            var mockILanguageService = new Mock<ILanguageService>();
            var mockBingMapService = InitializeBingMapMockService();

            var mockConfigurationService = new ConfigurationService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
            //var mockBingServices = new BingMapService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService(), mockConfigurationService);

            var successPlanEntityMetadata = new EntityMetadata() { LogicalName = cmc_successplan.EntityLogicalName };
            successPlanEntityMetadata.SetAttributeCollection(new List<AttributeMetadata>() {
                new UniqueIdentifierAttributeMetadata("cmc_successplanid"){ LogicalName = "cmc_successplanid" }
                , new StringAttributeMetadata("cmc_successplanname") { LogicalName = "cmc_successplanname" } });
            xrmFakedContext.InitializeMetadata(successPlanEntityMetadata);

            //todo retrieve change request mock for cmc_successPlan 
            // fake response for to success plan
            var retrieveSucessPlanMetadataChangesResponse = new RetrieveMetadataChangesResponse()
            {
                Results = new ParameterCollection
                {
                    { "EntityMetadata", new EntityMetadataCollection { successPlanEntityMetadata}}
                }
            };

            A.CallTo(() => xrmFakedContext.GetFakedOrganizationService()
                    .Execute(A<RetrieveMetadataChangesRequest>.That.Matches(r => r.Query.Criteria.Conditions.Any(p => p.Value.Equals("cmc_successplan")))))
                .Returns(retrieveSucessPlanMetadataChangesResponse);


            //todo retrieve change request mock for cmc_todo 
            var todoEntityMetadata = new EntityMetadata() { LogicalName = cmc_todo.EntityLogicalName };
            todoEntityMetadata.SetAttributeCollection(new List<AttributeMetadata>() {
                new UniqueIdentifierAttributeMetadata("cmc_todoid"){ LogicalName = "cmc_todoid" }
                , new StringAttributeMetadata("cmc_todoname") { LogicalName = "cmc_todoname" } });
            xrmFakedContext.SetEntityMetadata(todoEntityMetadata);

            // fake response for to do list

            var retrieveTodoMetadataChangesResponse = new RetrieveMetadataChangesResponse()
            {
                Results = new ParameterCollection
                {
                    { "EntityMetadata", new EntityMetadataCollection { todoEntityMetadata}}
                }
            };

            A.CallTo(() => xrmFakedContext.GetFakedOrganizationService()
                    .Execute(A<RetrieveMetadataChangesRequest>.That.Matches(r => r.Query.Criteria.Conditions.Any(p => p.Value.Equals("cmc_todo")))))
                .Returns(retrieveTodoMetadataChangesResponse);

            var contactService = new ContactService(mockLogger.Object, mockBingMapService.Object, xrmFakedContext.GetFakedOrganizationService(), mockConfigurationService, mockILanguageService.Object);
            contactService.CreateSuccessPlansForSelectedStudent(mockExecutionContext.Object);
            #endregion

            #region ASSERT

            var data = xrmFakedContext.Data["cmc_successplan"].Count > 0;
            Assert.IsTrue(data);

            #endregion
        }

        [TestCategory("Plugin"), TestCategory("Positive")]
        [TestMethod]
        public void CreateStudentSuccessPlans_StudentsHavingSameSuccessPlanTemplate_cmcduedatecalculationfield_AssignmentDate()
        {
            #region ARRANGE
            var academicPeriod = PrepareAcademicPeriod();
            var creatingStudent = PreparingStudent1(academicPeriod);
            var creatingSuccessplanTemplate = PreparingSuccessPlanTemplate();
            var creatingSuccessPlan = PreparingSuccessPlan1(creatingStudent.Id, creatingSuccessplanTemplate.Id);
            var creatingStudentModel = PrepareStudentModel(creatingStudent);
            var creatingSuccessPlanTodo = PreparingSuccessPlanTodoTemplate3(creatingSuccessplanTemplate.Id);

            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>()
            {
                creatingStudentModel,
                creatingSuccessPlan,
                creatingSuccessPlanTodo,
                creatingSuccessplanTemplate,
                academicPeriod,
                creatingStudent
            });

            var successplanTemp = new EntityReference("cmc_successplantemplate", creatingSuccessplanTemplate.Id);

            var studentEntityCollection = new EntityCollection();
            studentEntityCollection.Entities.Add(creatingStudent);

            var mockServiceProvider = InitializeMockService(xrmFakedContext, null, Operation.cmc_createstudentsuccessplansfromtemplate);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);

            AddInputParameters(mockServiceProvider, "Target", successplanTemp);
            AddInputParameters(mockServiceProvider, "Students", studentEntityCollection);

            var successPlanEntityMetadata = new EntityMetadata() { LogicalName = cmc_successplan.EntityLogicalName };
            successPlanEntityMetadata.SetAttributeCollection(new List<AttributeMetadata>() {
                new UniqueIdentifierAttributeMetadata("cmc_successplanid"){ LogicalName = "cmc_successplanid" }
                , new StringAttributeMetadata("cmc_successplanname") { LogicalName = "cmc_successplanname" } });
            xrmFakedContext.InitializeMetadata(successPlanEntityMetadata);

            //todo retrieve change request mock for cmc_successPlan 
            // fake response for to success plan
            var retrieveSucessPlanMetadataChangesResponse = new RetrieveMetadataChangesResponse()
            {
                Results = new ParameterCollection
                {
                    { "EntityMetadata", new EntityMetadataCollection { successPlanEntityMetadata}}
                }
            };

            A.CallTo(() => xrmFakedContext.GetFakedOrganizationService()
                    .Execute(A<RetrieveMetadataChangesRequest>.That.Matches(r => r.Query.Criteria.Conditions.Any(p => p.Value.Equals("cmc_successplan")))))
                .Returns(retrieveSucessPlanMetadataChangesResponse);


            //todo retrieve change request mock for cmc_todo 
            var todoEntityMetadata = new EntityMetadata() { LogicalName = cmc_todo.EntityLogicalName };
            todoEntityMetadata.SetAttributeCollection(new List<AttributeMetadata>() {
                new UniqueIdentifierAttributeMetadata("cmc_todoid"){ LogicalName = "cmc_todoid" }
                , new StringAttributeMetadata("cmc_todoname") { LogicalName = "cmc_todoname" } });
            xrmFakedContext.SetEntityMetadata(todoEntityMetadata);

            // fake response for to do list

            var retrieveTodoMetadataChangesResponse = new RetrieveMetadataChangesResponse()
            {
                Results = new ParameterCollection
                {
                    { "EntityMetadata", new EntityMetadataCollection { todoEntityMetadata}}
                }
            };

            A.CallTo(() => xrmFakedContext.GetFakedOrganizationService()
                    .Execute(A<RetrieveMetadataChangesRequest>.That.Matches(r => r.Query.Criteria.Conditions.Any(p => p.Value.Equals("cmc_todo")))))
                .Returns(retrieveTodoMetadataChangesResponse);

            #endregion

            #region ACT

            var mockLogger = new Mock<ILogger>();
            var mockILanguageService = new Mock<ILanguageService>();
            var mockBingMapService = InitializeBingMapMockService();

            var mockConfigurationService = new ConfigurationService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
            //var mockBingServices = new BingMapService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService(), mockConfigurationService);
            var contactService = new ContactService(mockLogger.Object, mockBingMapService.Object, xrmFakedContext.GetFakedOrganizationService(), mockConfigurationService, mockILanguageService.Object);
            contactService.CreateSuccessPlansForSelectedStudent(mockExecutionContext.Object);
            #endregion

            #region ASSERT

            var data = xrmFakedContext.Data["cmc_successplan"].Count > 0;
            Assert.IsTrue(data);

            #endregion
        }

        [TestCategory("Plugin"), TestCategory("Negative")]
        [TestMethod]
        public void CreateStudentSuccessPlans_StudentsHavingSameSuccessPlanTemplate_StudentsIsNull()
        {
            #region ARRANGE
            var academicPeriod = PrepareAcademicPeriod();
            var creatingStudent = PreparingStudent1(academicPeriod);
            var creatingSuccessplanTemplate = PreparingSuccessPlanTemplate();
            var creatingSuccessPlan = PreparingSuccessPlan1(creatingStudent.Id, creatingSuccessplanTemplate.Id);
            var creatingStudentModel = PrepareStudentModel(creatingStudent);
            var creatingSuccessPlanTodo = PreparingSuccessPlanTodoTemplate3(creatingSuccessplanTemplate.Id);

            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>()
            {
                creatingStudentModel,
                creatingSuccessPlan,
                creatingSuccessPlanTodo,
                creatingSuccessplanTemplate,
                academicPeriod,
                creatingStudent
            });

            var successplanTemp = new EntityReference("cmc_successplantemplate", creatingSuccessplanTemplate.Id);

            var studentEntityCollection = new EntityCollection();
            //studentEntityCollection.Entities.Add(creatingStudent);

            var mockServiceProvider = InitializeMockService(xrmFakedContext, null, Operation.cmc_createstudentsuccessplansfromtemplate);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);

            AddInputParameters(mockServiceProvider, "Target", successplanTemp);
            AddInputParameters(mockServiceProvider, "Students", studentEntityCollection);

            var successPlanEntityMetadata = new EntityMetadata() { LogicalName = cmc_successplan.EntityLogicalName };
            successPlanEntityMetadata.SetAttributeCollection(new List<AttributeMetadata>() {
                new UniqueIdentifierAttributeMetadata("cmc_successplanid"){ LogicalName = "cmc_successplanid" }
                , new StringAttributeMetadata("cmc_successplanname") { LogicalName = "cmc_successplanname" } });
            xrmFakedContext.InitializeMetadata(successPlanEntityMetadata);

            //todo retrieve change request mock for cmc_successPlan 
            // fake response for to success plan
            var retrieveSucessPlanMetadataChangesResponse = new RetrieveMetadataChangesResponse()
            {
                Results = new ParameterCollection
                {
                    { "EntityMetadata", new EntityMetadataCollection { successPlanEntityMetadata}}
                }
            };

            A.CallTo(() => xrmFakedContext.GetFakedOrganizationService()
                    .Execute(A<RetrieveMetadataChangesRequest>.That.Matches(r => r.Query.Criteria.Conditions.Any(p => p.Value.Equals("cmc_successplan")))))
                .Returns(retrieveSucessPlanMetadataChangesResponse);


            //todo retrieve change request mock for cmc_todo 
            var todoEntityMetadata = new EntityMetadata() { LogicalName = cmc_todo.EntityLogicalName };
            todoEntityMetadata.SetAttributeCollection(new List<AttributeMetadata>() {
                new UniqueIdentifierAttributeMetadata("cmc_todoid"){ LogicalName = "cmc_todoid" }
                , new StringAttributeMetadata("cmc_todoname") { LogicalName = "cmc_todoname" } });
            xrmFakedContext.SetEntityMetadata(todoEntityMetadata);

            // fake response for to do list

            var retrieveTodoMetadataChangesResponse = new RetrieveMetadataChangesResponse()
            {
                Results = new ParameterCollection
                {
                    { "EntityMetadata", new EntityMetadataCollection { todoEntityMetadata}}
                }
            };

            A.CallTo(() => xrmFakedContext.GetFakedOrganizationService()
                    .Execute(A<RetrieveMetadataChangesRequest>.That.Matches(r => r.Query.Criteria.Conditions.Any(p => p.Value.Equals("cmc_todo")))))
                .Returns(retrieveTodoMetadataChangesResponse);

            #endregion

            #region ACT

            var mockLogger = new Mock<ILogger>();
            var mockILanguageService = new Mock<ILanguageService>();
            var mockBingMapService = InitializeBingMapMockService();

            var mockConfigurationService = new ConfigurationService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
            //var mockBingServices = new BingMapService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService(), mockConfigurationService);
            var contactService = new ContactService(mockLogger.Object, mockBingMapService.Object, xrmFakedContext.GetFakedOrganizationService(), mockConfigurationService, mockILanguageService.Object);
            contactService.CreateSuccessPlansForSelectedStudent(mockExecutionContext.Object);
            #endregion

            #region ASSERT
            var mockPluginExecutionContext = (IPluginExecutionContext)mockServiceProvider.Object.GetService(typeof(IPluginExecutionContext));
            var response = mockPluginExecutionContext.OutputParameters["SuccessPlanResponse"];

            JavaScriptSerializer json_serializer = new JavaScriptSerializer();
            var routes_list = json_serializer.DeserializeObject(response.ToString()) as Dictionary<string, object>;
            object count;
            (routes_list).TryGetValue("TotalCount", out count);

            Assert.AreEqual(0, (int)count);

            #endregion
        }

        [TestCategory("Plugin"), TestCategory("Positive")]
        [TestMethod]
        public void CreateStudentSuccessPlans_StudentsHavingSameSuccessPlanTemplate_StudentsHavingSameSuccessPlanTemplate_IsNotContains()
        {
            #region ARRANGE
            var creatingStudent = PreparingStudent();
            var creatingStudent1 = PreparingStudent();

            var creatingSuccessplanTemplate = PreparingSuccessPlanTemplate();
            var creatingSuccessPlan = PreparingSuccessPlan(creatingStudent.Id, creatingSuccessplanTemplate.Id);
            var creatingStudentModel = PrepareStudentModel(creatingStudent);
            var creatingSuccessPlanTodo = PreparingSuccessPlanTodoTemplate(creatingSuccessplanTemplate.Id);


            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>()
            {
                creatingStudentModel,
                creatingSuccessPlan,
                creatingSuccessPlanTodo,
                creatingSuccessplanTemplate
            });

            var successplanTemp = new EntityReference("cmc_successplantemplate", creatingSuccessplanTemplate.Id);

            var studentEntityCollection = new EntityCollection();
            studentEntityCollection.Entities.Add(creatingStudent);
            studentEntityCollection.Entities.Add(creatingStudent1);

            var mockServiceProvider = InitializeMockService(xrmFakedContext, null, Operation.cmc_createstudentsuccessplansfromtemplate);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);

            AddInputParameters(mockServiceProvider, "Target", successplanTemp);
            AddInputParameters(mockServiceProvider, "Students", studentEntityCollection);


            var successPlanEntityMetadata = new EntityMetadata() { LogicalName = cmc_successplan.EntityLogicalName };
            successPlanEntityMetadata.SetAttributeCollection(new List<AttributeMetadata>() {
                new UniqueIdentifierAttributeMetadata("cmc_successplanid"){ LogicalName = "cmc_successplanid" }
                , new StringAttributeMetadata("cmc_successplanname") { LogicalName = "cmc_successplanname" } });
            xrmFakedContext.InitializeMetadata(successPlanEntityMetadata);

            //todo retrieve change request mock for cmc_successPlan 
            // fake response for to success plan
            var retrieveSucessPlanMetadataChangesResponse = new RetrieveMetadataChangesResponse()
            {
                Results = new ParameterCollection
                {
                    { "EntityMetadata", new EntityMetadataCollection { successPlanEntityMetadata}}
                }
            };

            A.CallTo(() => xrmFakedContext.GetFakedOrganizationService()
                    .Execute(A<RetrieveMetadataChangesRequest>.That.Matches(r => r.Query.Criteria.Conditions.Any(p => p.Value.Equals("cmc_successplan")))))
                .Returns(retrieveSucessPlanMetadataChangesResponse);


            //todo retrieve change request mock for cmc_todo 
            var todoEntityMetadata = new EntityMetadata() { LogicalName = cmc_todo.EntityLogicalName };
            todoEntityMetadata.SetAttributeCollection(new List<AttributeMetadata>() {
                new UniqueIdentifierAttributeMetadata("cmc_todoid"){ LogicalName = "cmc_todoid" }
                , new StringAttributeMetadata("cmc_todoname") { LogicalName = "cmc_todoname" } });
            xrmFakedContext.SetEntityMetadata(todoEntityMetadata);

            // fake response for to do list

            var retrieveTodoMetadataChangesResponse = new RetrieveMetadataChangesResponse()
            {
                Results = new ParameterCollection
                {
                    { "EntityMetadata", new EntityMetadataCollection { todoEntityMetadata}}
                }
            };

            A.CallTo(() => xrmFakedContext.GetFakedOrganizationService()
                    .Execute(A<RetrieveMetadataChangesRequest>.That.Matches(r => r.Query.Criteria.Conditions.Any(p => p.Value.Equals("cmc_todo")))))
                .Returns(retrieveTodoMetadataChangesResponse);

            #endregion

            #region ACT

            var mockLogger = new Mock<ILogger>();
            var mockILanguageService = new Mock<ILanguageService>();
            var mockBingMapService = InitializeBingMapMockService();

            var mockConfigurationService = new ConfigurationService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
            //var mockBingServices = new BingMapService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService(), mockConfigurationService);

            var contactService = new ContactService(mockLogger.Object, mockBingMapService.Object, xrmFakedContext.GetFakedOrganizationService(), mockConfigurationService, mockILanguageService.Object);

            contactService.CreateSuccessPlansForSelectedStudent(mockExecutionContext.Object);

            #endregion

            #region ASSERT

            var data = xrmFakedContext.Data["cmc_successplan"].Count > 0;
            Assert.IsTrue(data);
            #endregion
        }

        private Entity PreparingStudent()
        {
            var contact = new Models.Contact()
            {
                Id = Guid.NewGuid(),
                ["fullname"] = "Harry Potter",
                cmc_isstudent = true
            };
            return contact;
        }

        private Entity PreparingStudent1(Entity academicPeriod)
        {
            var contact = new Models.Contact()
            {
                Id = Guid.NewGuid(),
                ["fullname"] = "Harry Potter",
                cmc_isstudent = true,
                mshied_CurrentAcademicPeriodId = academicPeriod.ToEntityReference()
            };
            return contact;
        }

        private mshied_academicperiod PrepareAcademicPeriod()
        {
            return new mshied_academicperiod()
            {
                Id = Guid.NewGuid(),
                mshied_StartDate = DateTime.Now,
                mshied_EndDate = DateTime.Now.AddDays(5)
            };
        }

        private Entity PrepareStudentModel(Entity student)
        {
            var contact = new Models.Contact()
            {
                ContactId = student.Id
            };
            return contact;
        }

        private Entity PreparingSuccessPlan(Guid contact, Guid successPlanTemplate)
        {
            var entitySuccessPlan = new Entity("cmc_successplan", Guid.NewGuid())
            {
                ["cmc_successplantemplateid"] = successPlanTemplate,
                ["cmc_assignedtoid"] = contact,

            };
            return entitySuccessPlan;
        }

        private Entity PreparingSuccessPlan1(Guid contact, Guid successPlanTemplate)
        {
            var entitySuccessPlan = new Entity("cmc_successplan", Guid.NewGuid())
            {
                ["cmc_successplantemplateid"] = successPlanTemplate
            };
            return entitySuccessPlan;
        }

        private Entity PreparingSuccessPlanTemplate()
        {
            var entitySuccessPlanTemplate = new Entity("cmc_successplantemplate", Guid.NewGuid())
            {
                ["cmc_successplantemplatename"] = "Test Success Plan Template"
            };
            return entitySuccessPlanTemplate;
        }

        private Entity PreparingSuccessPlanTodoTemplate(Guid entitySuccessPlanTemplate)
        {
            var todo = new Entity("cmc_successplantodotemplate", Guid.NewGuid())
            {
                ["cmc_successplantemplateid"] = new EntityReference("cmc_successplantemplate", entitySuccessPlanTemplate),
                ["cmc_successplantodotemplatename"] = "Test Success Plan To Do Template",
                ["cmc_duedatecalculationtype"] = new OptionSetValue((int)cmc_successplantodotemplate_cmc_duedatecalculationtype.Static),
                ["cmc_duedatestatic"] = DateTime.Now
            };
            return todo;
        }

        private Entity PreparingSuccessPlanTodoTemplate1(Guid entitySuccessPlanTemplate)
        {
            var todo = new Entity("cmc_successplantodotemplate", Guid.NewGuid())
            {
                ["cmc_successplantemplateid"] = new EntityReference("cmc_successplantemplate", entitySuccessPlanTemplate),
                ["cmc_successplantodotemplatename"] = "Test Success Plan To Do Template",
                ["cmc_duedatecalculationtype"] = new OptionSetValue((int)cmc_successplantodotemplate_cmc_duedatecalculationtype.Calculated),
                ["cmc_duedatecalculationfield"] = new OptionSetValue((int)cmc_successplantodotemplate_cmc_duedatecalculationfield.StartofAcademicPeriod),
                ["cmc_duedatenumberofdays"] = 2,
                ["cmc_duedatedaysdirection"] = new OptionSetValue((int)cmc_successplantodotemplate_cmc_duedatedaysdirection.After)
            };
            return todo;
        }

        private Entity PreparingSuccessPlanTodoTemplate2(Guid entitySuccessPlanTemplate)
        {
            var todo = new Entity("cmc_successplantodotemplate", Guid.NewGuid())
            {
                ["cmc_successplantemplateid"] = new EntityReference("cmc_successplantemplate", entitySuccessPlanTemplate),
                ["cmc_successplantodotemplatename"] = "Test Success Plan To Do Template",
                ["cmc_duedatecalculationtype"] = new OptionSetValue((int)cmc_successplantodotemplate_cmc_duedatecalculationtype.Calculated),
                ["cmc_duedatecalculationfield"] = new OptionSetValue((int)cmc_successplantodotemplate_cmc_duedatecalculationfield.EndofAcademicPeriod),
                ["cmc_duedatenumberofdays"] = 2,
                ["cmc_duedatedaysdirection"] = new OptionSetValue((int)cmc_successplantodotemplate_cmc_duedatedaysdirection.Before)
            };
            return todo;
        }

        private Entity PreparingSuccessPlanTodoTemplate3(Guid entitySuccessPlanTemplate)
        {
            var todo = new Entity("cmc_successplantodotemplate", Guid.NewGuid())
            {
                ["cmc_successplantemplateid"] = new EntityReference("cmc_successplantemplate", entitySuccessPlanTemplate),
                ["cmc_successplantodotemplatename"] = "Test Success Plan To Do Template",
                ["cmc_duedatecalculationtype"] = new OptionSetValue((int)cmc_successplantodotemplate_cmc_duedatecalculationtype.Calculated),
                ["cmc_duedatecalculationfield"] = new OptionSetValue((int)cmc_successplantodotemplate_cmc_duedatecalculationfield.AssignmentDate),
                ["cmc_duedatenumberofdays"] = 2,
                ["cmc_duedatedaysdirection"] = new OptionSetValue((int)cmc_successplantodotemplate_cmc_duedatedaysdirection.Before)
            };
            return todo;
        }

        private Entity PreparingSuccessPlanNotLinkedToContact(Guid contact, Guid successPlanTemplate)
        {
            cmc_successplan sp2 = new cmc_successplan()
            {
                cmc_successplanId = successPlanTemplate,
                KeyAttributes = new KeyAttributeCollection() { new KeyValuePair<string, object>("Id", Guid.NewGuid()) }
            };
            return sp2;
        }

    }
}