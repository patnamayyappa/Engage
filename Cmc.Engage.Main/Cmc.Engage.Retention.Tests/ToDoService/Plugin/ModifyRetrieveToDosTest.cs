using System;
using System.Collections.Generic;
using System.Linq;
using Cmc.Core.Xrm.ServerExtension.Logging;
using Cmc.Engage.Common.Plugins.Tests.Utilities;
using Cmc.Engage.Models.Tests;
using FakeItEasy;
using FakeXrmEasy;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Moq;

namespace Cmc.Engage.Retention.Tests.ToDoService.Plugin
{
    [TestClass]
    public class ModifyRetrieveToDosTest : XrmUnitTestBase
    {
        [TestCategory("Plugin"), TestCategory("Positive")]
        [TestMethod]
        public void ModifyRetrieveToDosService_RetrieveMultiple()
        {
            #region ARRANGE

            var contactInstance = PrepareContact();
            var creatingTodo = PrepareTodo(contactInstance);
            var academicPeriod = PrepareAcademicPeriod();
            var academicProgress = PreapreAcademicProgress(academicPeriod.Id, contactInstance.Id);

            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>()
            {
                contactInstance,
                creatingTodo,
                academicPeriod,
                academicProgress
            });

            // use fake to retrieve statments where the fakexrm is not implemented.
            A.CallTo(() => xrmFakedContext.GetFakedOrganizationService().RetrieveMultiple(A<FetchExpression>._)).Returns(new EntityCollection(new List<Entity> { academicPeriod }));
            var mockServiceProvider = InitializeMockService(xrmFakedContext, creatingTodo, Operation.cmc_todo);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            #endregion

            #region ACT
            var mockLogger = new Mock<ILogger>();

            ConditionExpression condition2 = new ConditionExpression();
            condition2.AttributeName = "cmc_todoname";
            condition2.Operator = ConditionOperator.Equal;
            condition2.Values.Add("{Filter By Academic Period}");

            ConditionExpression condition3 = new ConditionExpression();
            condition3.AttributeName = "cmc_assignedtostudentid";
            condition3.Operator = ConditionOperator.Equal;
            condition3.Values.Add(contactInstance.Id);

            FilterExpression filter1 = new FilterExpression();
            //filter1.Conditions.Add(condition1);
            filter1.Conditions.Add(condition2);

            var query = new QueryExpression("cmc_todo")
            {
                ColumnSet = new ColumnSet(true)
            };
            //query.ColumnSet.AddColumn("LastName");
            query.Criteria.AddFilter(filter1);
            query.Criteria.AddCondition(condition3);

            AddInputParameters(mockServiceProvider, "Query", query);

            var todoService = new Retention.ToDoService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
            todoService.ModifyRetrieveToDos(mockExecutionContext.Object);
            #endregion

            #region ASSERT
            // check the above conditions should are removed -- updated with someother conditions
            Assert.IsFalse(query.Criteria.Filters.Any(r => r.Conditions.Any(p => p.AttributeName == "cmc_todoname")));
            #endregion
        }


        [TestMethod]
        [TestCategory("Plugin"), TestCategory("Positive")]
        public void ModifyRetrieveToDosService_FetchExpression_RetrieveMultiple()
        {
            #region ARRANGE

            var contactInstance = PrepareContact();
            var creatingTodo = PrepareTodo(contactInstance);
            var academicPeriod = PrepareAcademicPeriod();
            var academicProgress = PreapreAcademicProgress(academicPeriod.Id, contactInstance.Id);

            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>()
            {
                contactInstance,
                creatingTodo,
                academicPeriod,
                academicProgress
            });

            ConditionExpression condition2 = new ConditionExpression();
            condition2.AttributeName = "cmc_todoname";
            condition2.Operator = ConditionOperator.Equal;
            condition2.Values.Add("{Filter By Academic Period}");

            ConditionExpression condition3 = new ConditionExpression();
            condition3.AttributeName = "cmc_assignedtostudentid";
            condition3.Operator = ConditionOperator.Equal;
            condition3.Values.Add(contactInstance.Id);

            FilterExpression filter1 = new FilterExpression();
            //filter1.Conditions.Add(condition1);
            filter1.Conditions.Add(condition2);
            filter1.Conditions.Add(condition3);



            QueryExpression contactquery = new QueryExpression()
            {
                EntityName = "cmc_todo",
                ColumnSet = new ColumnSet(true),
                Criteria = filter1,

            };

            var fetchXmlToQueryExpressionResponse = new FetchXmlToQueryExpressionResponse()
            {
                Results = new ParameterCollection
                        {
                            { "Query", contactquery}
                        }
            };

            A.CallTo(() => xrmFakedContext.GetFakedOrganizationService().Execute(A<FetchXmlToQueryExpressionRequest>._)).Returns(fetchXmlToQueryExpressionResponse);

            // use fake to retrieve statments where the fakexrm is not implemented.
            A.CallTo(() => xrmFakedContext.GetFakedOrganizationService().RetrieveMultiple(A<FetchExpression>._)).Returns(new EntityCollection(new List<Entity> { academicPeriod }));
            var mockServiceProvider = InitializeMockService(xrmFakedContext, creatingTodo, Operation.cmc_todo);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            var mockPluginExecutionContext = (IPluginExecutionContext)mockServiceProvider.Object.GetService(typeof(IPluginExecutionContext));
            #endregion

            #region ACT
            var mockLogger = new Mock<ILogger>();



            var query = new FetchExpression("cmc_todo");


            AddInputParameters(mockServiceProvider, "Query", query);

            var todoService = new Retention.ToDoService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
            todoService.ModifyRetrieveToDos(mockExecutionContext.Object);
            #endregion

            #region ASSERT            
            // Passing Only one Input parameters but we getting two 
            Assert.AreEqual(2, mockPluginExecutionContext.InputParameters.Values.Count);
            #endregion
        }



        [TestMethod]
        [TestCategory("Plugin"), TestCategory("Negative")]
        public void ModifyRetrieveToDosService_Expression_RetrieveMultiple()
        {
            #region ARRANGE

            var contactInstance = PrepareContact();
            var creatingTodo = PrepareTodo(contactInstance);
            var academicPeriod = PrepareAcademicPeriod();
            var academicProgress = PreapreAcademicProgress(academicPeriod.Id, contactInstance.Id);

            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>()
            {
                contactInstance,
                creatingTodo,
                academicPeriod,
                academicProgress
            });
           


            QueryExpression contactquery = new QueryExpression()
            {
                EntityName = "cmc_todo",
            };           

          
            var mockServiceProvider = InitializeMockService(xrmFakedContext, creatingTodo, Operation.cmc_todo);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
           
            #endregion

            #region ACT
            var mockLogger = new Mock<ILogger>();
            var query = new FetchExpression("cmc_todo");
            AddInputParameters(mockServiceProvider, "Query", "");
            var todoService = new Retention.ToDoService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
            todoService.ModifyRetrieveToDos(mockExecutionContext.Object);
            #endregion

            #region ASSERT            
            // Passing Only one Input parameters but we getting two 
            // No Assert 
            #endregion
        }

        [TestMethod]
        [TestCategory("Plugin"), TestCategory("Negative")]
        public void ModifyRetrieveToDosService_RetrieveMultiple_Negative()
        {
            #region ARRANGE

            var contactInstance = PrepareContact();
            var creatingTodo = PrepareTodoNegative(contactInstance);
            var academicPeriod = PrepareAcademicPeriod();
            var academicProgress = PreapreAcademicProgress(academicPeriod.Id, contactInstance.Id);

            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>()
            {
                contactInstance,
                creatingTodo,
                academicPeriod,
                academicProgress
            });

            // use fake to retrieve statments where the fakexrm is not implemented.
            A.CallTo(() => xrmFakedContext.GetFakedOrganizationService().RetrieveMultiple(A<FetchExpression>._)).Returns(new EntityCollection(new List<Entity> { academicPeriod }));
            var mockServiceProvider = InitializeMockService(xrmFakedContext, creatingTodo, Operation.cmc_todo);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            #endregion

            #region ACT
            var mockLogger = new Mock<ILogger>();

            ConditionExpression condition2 = new ConditionExpression();
            condition2.AttributeName = "cmc_todoname";
            condition2.Operator = ConditionOperator.Equal;
            condition2.Values.Add("{Filter By Academic Period}");

            ConditionExpression condition3 = new ConditionExpression();
            condition3.AttributeName = "cmc_assignedtostudentid";
            condition3.Operator = ConditionOperator.Equal;
            condition3.Values.Add(contactInstance.Id);

            FilterExpression filter1 = new FilterExpression();
            //filter1.Conditions.Add(condition1);
            filter1.Conditions.Add(condition2);

            var query = new QueryExpression("cmc_todo")
            {
                ColumnSet = new ColumnSet(true)
            };
            //query.ColumnSet.AddColumn("LastName");
            query.Criteria.AddFilter(filter1);
            //query.Criteria.AddCondition(condition3);

            AddInputParameters(mockServiceProvider, "Query", query);

            var todoService = new Retention.ToDoService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
            todoService.ModifyRetrieveToDos(mockExecutionContext.Object);
            #endregion

            #region ASSERT
            //No Assert For Negative 
            #endregion
        }
        private Entity PrepareTodo(Entity contact)
        {
            var todo = new Entity("cmc_todo", Guid.NewGuid())
            {
                ["cmc_todoname"] = "Test to do",
                ["cmc_assignedtostudentid"] = contact.ToEntityReference()
                //cmc_duedate = 2018-05-28
            };
            return todo;
        }

        private Entity PrepareContact()
        {
            var contact = new Entity("Contact", Guid.NewGuid())
            {
                ["LastName"] = "Brown",
            };
            return contact;
        }

        private mshied_academicperiod PrepareAcademicPeriod()
        {
            return new mshied_academicperiod
            {
                Id = Guid.NewGuid(),
                mshied_StartDate = DateTime.Today,
                mshied_EndDate = DateTime.Today.AddDays(5),
            }; ;
        }

        private mshied_academicperioddetails PreapreAcademicProgress(Guid academicPeriod, Guid contactGuid)
        {
            var academicProgress = new mshied_academicperioddetails()
            {
                Id = Guid.NewGuid(),
                mshied_AcademicPeriodID = new EntityReference("mshied_academicperiod", academicPeriod),
                mshied_StudentId = new EntityReference("contact", contactGuid)
            };
            return academicProgress;
        }


        private Entity PrepareTodoNegative(Entity contact)
        {
            var todo = new Entity("cmc_todo", Guid.NewGuid())
            {
                ["cmc_todoname"] = "Test to do",                
                //cmc_duedate = 2018-05-28
            };
            return todo;
        }

    }

}



