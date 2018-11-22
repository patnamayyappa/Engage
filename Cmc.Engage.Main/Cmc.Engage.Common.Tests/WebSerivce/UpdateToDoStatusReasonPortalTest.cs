using System;
using System.Collections.Generic;
using Cmc.Core.Xrm.ServerExtension.Core;
using Cmc.Core.Xrm.ServerExtension.Logging;
using Cmc.Engage.Common.Plugins.Tests.Utilities;
using Cmc.Engage.Communication;
using Cmc.Engage.Models;
using FakeXrmEasy;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Moq;

namespace Cmc.Engage.Common.Tests.WebSerivce
{
    [TestClass]
    public class UpdateToDoStatusReasonPortalTest : XrmUnitTestBase
    {

        [TestCategory("Web Service"), TestCategory("Positive")]
        [TestMethod]
        public void UpdateToDoStatusReasonPortalTest_UpdateToDoStatus_MarkedAscomplete()
        {

            #region Arrange
            var student = CreateStudentEntity();
            var toDo = CreateToDoEntity(student, false);

            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>()
            {
                student,
                toDo
            });
            var mockServiceProvider = InitializeMockService(xrmFakedContext, toDo, Operation.RetrieveMultiple);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            #endregion Arrange

            #region Act
            var mockLogger = new Mock<ILogger>();
            var mocktoDoService = new UpdateToDoStatusReasonLogic(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());

            var markToDoAsRead = new UpdateToDoStatusReasonPortalLogic(mockLogger.Object, mocktoDoService, xrmFakedContext.GetFakedOrganizationService());

            var stringInput = "{'ToDoId':'" + toDo.Id + "','Status':'1','CloseComment':'Test Comment','StudentId':'" + student.Id + "'}";

            markToDoAsRead.DoWork(mockExecutionContext.Object, stringInput);
            #endregion Act

            #region Assert

            xrmFakedContext.Data["cmc_todo"].TryGetValue(toDo.Id, out toDo);
            Assert.AreEqual((int)cmc_todo_statuscode.MarkedasComplete, ((OptionSetValue)toDo["statuscode"]).Value);

            #endregion Assert

        }
        [TestCategory("Web Service"), TestCategory("Positive")]
        [TestMethod]
        public void UpdateToDoStatusReasonPortalTest_UpdateToDoStatus_Complete()
        {

            #region Arrange
            var student = CreateStudentEntity();
            var toDo = CreateToDoEntity(student, true);

            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>()
            {
                student,
                toDo
            });
            var mockServiceProvider = InitializeMockService(xrmFakedContext, toDo, Operation.RetrieveMultiple);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            #endregion Arrange

            #region Act
            var mockLogger = new Mock<ILogger>();
            var mocktoDoService = new UpdateToDoStatusReasonLogic(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());

            var markToDoAsRead = new UpdateToDoStatusReasonPortalLogic(mockLogger.Object, mocktoDoService, xrmFakedContext.GetFakedOrganizationService());

            var stringInput = "{'ToDoId':'" + toDo.Id + "','Status':'1','CloseComment':'Test Comment','StudentId':'" + student.Id + "'}";

            markToDoAsRead.DoWork(mockExecutionContext.Object, stringInput);
            #endregion Act

            #region Assert

            xrmFakedContext.Data["cmc_todo"].TryGetValue(toDo.Id, out toDo);
            Assert.AreEqual((int)cmc_todo_statuscode.Complete, ((OptionSetValue)toDo["statuscode"]).Value);

            #endregion Assert

        }

        [TestCategory("Web Service"), TestCategory("Positive")]
        [TestMethod]
        public void UpdateToDoStatusReasonPortalTest_UpdateToDoStatus_Incomplete()
        {

            #region Arrange
            var student = CreateStudentEntity();
            var toDo = CreateToDoEntity(student, true);
            toDo["statuscode"] = new OptionSetValue(2);
            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>()
            {
                student,
                toDo
            });
            var mockServiceProvider = InitializeMockService(xrmFakedContext, toDo, Operation.RetrieveMultiple);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            #endregion Arrange

            #region Act
            var mockLogger = new Mock<ILogger>();
            var mocktoDoService = new UpdateToDoStatusReasonLogic(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());

            var markToDoAsRead = new UpdateToDoStatusReasonPortalLogic(mockLogger.Object, mocktoDoService, xrmFakedContext.GetFakedOrganizationService());

            var stringInput = "{'ToDoId':'" + toDo.Id + "','Status':'0','CloseComment':'Test Comment','StudentId':'" + student.Id + "'}";

            markToDoAsRead.DoWork(mockExecutionContext.Object, stringInput);
            #endregion Act

            #region Assert

            xrmFakedContext.Data["cmc_todo"].TryGetValue(toDo.Id, out toDo);
            Assert.AreEqual((int)cmc_todo_statuscode.Incomplete, ((OptionSetValue)toDo["statuscode"]).Value);

            #endregion Assert

        }

        [TestCategory("Web Service"), TestCategory("Positive")]
        [TestMethod]
        public void UpdateToDoStatusReasonPortalTest_UpdateToDoStatus_Canceled()
        {

            #region Arrange
            var student = CreateStudentEntity();
            var toDo = CreateToDoEntity(student, true);

            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>()
            {
                student,
                toDo
            });
            var mockServiceProvider = InitializeMockService(xrmFakedContext, toDo, Operation.RetrieveMultiple);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            #endregion Arrange

            #region Act
            var mockLogger = new Mock<ILogger>();
            var mocktoDoService = new UpdateToDoStatusReasonLogic(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());

            var markToDoAsRead = new UpdateToDoStatusReasonPortalLogic(mockLogger.Object, mocktoDoService, xrmFakedContext.GetFakedOrganizationService());

            var stringInput = "{'ToDoId':'" + toDo.Id + "','Status':'2','CloseComment':'Test Comment','StudentId':'" + student.Id + "'}";

            object result=markToDoAsRead.DoWork(mockExecutionContext.Object, stringInput);
            #endregion Act

            #region Assert

            xrmFakedContext.Data["cmc_todo"].TryGetValue(toDo.Id, out toDo);
            Assert.AreEqual((int)cmc_todo_statuscode.Canceled, ((OptionSetValue)toDo["statuscode"]).Value);

            #endregion Assert

        }

        [TestCategory("Web Service"), TestCategory("Negative")]
        [TestMethod]
        public void UpdateToDoStatusReasonPortalTest_UpdateToDoStatus_InputIsNull()
        {

            #region Arrange
            var student = CreateStudentEntity();
            var toDo = CreateToDoEntity(student, true);

            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>()
            {
                student,
                toDo
            });
            var mockServiceProvider = InitializeMockService(xrmFakedContext, toDo, Operation.RetrieveMultiple);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            #endregion Arrange

            #region Act
            var mockLogger = new Mock<ILogger>();
            var mocktoDoService = new UpdateToDoStatusReasonLogic(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());

            var markToDoAsRead = new UpdateToDoStatusReasonPortalLogic(mockLogger.Object, mocktoDoService, xrmFakedContext.GetFakedOrganizationService());

            var stringInput = "";

            object result=markToDoAsRead.DoWork(mockExecutionContext.Object, stringInput);
            #endregion Act

            #region Assert

            Assert.IsFalse((bool)result);

            #endregion Assert

        }

        [TestCategory("Web Service"), TestCategory("Negative")]
        [TestMethod]
        public void UpdateToDoStatusReasonPortalTest_UpdateToDoStatus_AssignedtostudentidIsNull()
        {

            #region Arrange
            var student = CreateStudentEntity();
            var toDo = CreateToDoEntity1(student, true);

            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>()
            {
                student,
                toDo
            });
            var mockServiceProvider = InitializeMockService(xrmFakedContext, toDo, Operation.RetrieveMultiple);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            #endregion Arrange

            #region Act
            var mockLogger = new Mock<ILogger>();
            var mocktoDoService = new UpdateToDoStatusReasonLogic(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());

            var markToDoAsRead = new UpdateToDoStatusReasonPortalLogic(mockLogger.Object, mocktoDoService, xrmFakedContext.GetFakedOrganizationService());

            var stringInput = "{'ToDoId':'" + toDo.Id + "','Status':'2','CloseComment':'Test Comment','StudentId':'" + student.Id + "'}";

            object result = markToDoAsRead.DoWork(mockExecutionContext.Object, stringInput);
            #endregion Act

            #region Assert

            Assert.IsFalse((bool)result);

            #endregion Assert

        }

        [TestCategory("Web Service"), TestCategory("Negative")]
        [TestMethod]
        public void UpdateToDoStatusReasonPortalTest_UpdateToDoStatus_StatuscodeIsNull()
        {

            #region Arrange
            var student = CreateStudentEntity();
            var toDo = CreateToDoEntity2(student, true);

            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>()
            {
                student,
                toDo
            });
            var mockServiceProvider = InitializeMockService(xrmFakedContext, toDo, Operation.RetrieveMultiple);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            #endregion Arrange

            #region Act
            var mockLogger = new Mock<ILogger>();
            var mocktoDoService = new UpdateToDoStatusReasonLogic(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());

            var markToDoAsRead = new UpdateToDoStatusReasonPortalLogic(mockLogger.Object, mocktoDoService, xrmFakedContext.GetFakedOrganizationService());

            var stringInput = "{'ToDoId':'" + toDo.Id + "','Status':'2','CloseComment':'Test Comment','StudentId':'" + student.Id + "'}";

            object result = markToDoAsRead.DoWork(mockExecutionContext.Object, stringInput);
            #endregion Act

            #region Assert

            Assert.IsFalse((bool)result);

            #endregion Assert

        }

        [TestCategory("Web Service"), TestCategory("Negative")]
        [TestMethod]
        public void UpdateToDoStatusReasonPortalTest_UpdateToDoStatus_ToDoStatusesIsCanceled()
        {

            #region Arrange
            var student = CreateStudentEntity();
            var toDo = CreateToDoEntity3(student, true);

            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>()
            {
                student,
                toDo
            });
            var mockServiceProvider = InitializeMockService(xrmFakedContext, toDo, Operation.RetrieveMultiple);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            #endregion Arrange

            #region Act
            var mockLogger = new Mock<ILogger>();
            var mocktoDoService = new UpdateToDoStatusReasonLogic(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());

            var markToDoAsRead = new UpdateToDoStatusReasonPortalLogic(mockLogger.Object, mocktoDoService, xrmFakedContext.GetFakedOrganizationService());

            var stringInput = "{'ToDoId':'" + toDo.Id + "','Status':'2','CloseComment':'Test Comment','StudentId':'" + student.Id + "'}";

            object result = markToDoAsRead.DoWork(mockExecutionContext.Object, stringInput);
            #endregion Act

            #region Assert

            Assert.IsFalse((bool)result);

            #endregion Assert

        }

        [TestCategory("Web Service"), TestCategory("Negative")]
        [TestMethod]
        public void UpdateToDoStatusReasonPortalTest_UpdateToDoStatus_CmcTodoStatuscodeIsComplete()
        {

            #region Arrange
            var student = CreateStudentEntity();
            var toDo = CreateToDoEntity4(student, false);

            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>()
            {
                student,
                toDo
            });
            var mockServiceProvider = InitializeMockService(xrmFakedContext, toDo, Operation.RetrieveMultiple);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            #endregion Arrange

            #region Act
            var mockLogger = new Mock<ILogger>();
            var mocktoDoService = new UpdateToDoStatusReasonLogic(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());

            var markToDoAsRead = new UpdateToDoStatusReasonPortalLogic(mockLogger.Object, mocktoDoService, xrmFakedContext.GetFakedOrganizationService());

            var stringInput = "{'ToDoId':'" + toDo.Id + "','Status':'2','CloseComment':'Test Comment','StudentId':'" + student.Id + "'}";

            object result = markToDoAsRead.DoWork(mockExecutionContext.Object, stringInput);
            #endregion Act

            #region Assert

            Assert.IsFalse((bool)result);

            #endregion Assert

        }

        [TestCategory("Web Service"), TestCategory("Negative")]
        [TestMethod]
        public void UpdateToDoStatusReasonPortalTest_UpdateToDoStatus_ToDoStatusesIsDefault()
        {

            #region Arrange
            var student = CreateStudentEntity();
            var toDo = CreateToDoEntity3(student, true);

            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>()
            {
                student,
                toDo
            });
            var mockServiceProvider = InitializeMockService(xrmFakedContext, toDo, Operation.RetrieveMultiple);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            #endregion Arrange

            #region Act
            var mockLogger = new Mock<ILogger>();
            var mocktoDoService = new UpdateToDoStatusReasonLogic(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());

            var markToDoAsRead = new UpdateToDoStatusReasonPortalLogic(mockLogger.Object, mocktoDoService, xrmFakedContext.GetFakedOrganizationService());

            var stringInput = "{'ToDoId':'" + toDo.Id + "','Status':'4','CloseComment':'Test Comment','StudentId':'" + student.Id + "'}";

            object result = markToDoAsRead.DoWork(mockExecutionContext.Object, stringInput);
            #endregion Act

            #region Assert

            Assert.IsFalse((bool)result);

            #endregion Assert

        }


        [TestCategory("Web Service"), TestCategory("Negative")]
        [TestMethod]
        public void UpdateToDoStatusReasonPortalTest_UpdateToDoStatus_ArgumentException()
        {

            #region Arrange
            
            var xrmFakedContext = new XrmFakedContext();
            
            #endregion Arrange

            #region Act
            var mockLogger = new Mock<ILogger>();
            
            #endregion Act

            #region Assert
            Assert.ThrowsException<ArgumentNullException>(() => new UpdateToDoStatusReasonLogic(null, null));
            Assert.ThrowsException<ArgumentNullException>(() => new UpdateToDoStatusReasonLogic(mockLogger.Object, null));
            Assert.ThrowsException<ArgumentException>(() => new UpdateToDoStatusReasonPortalLogic(null, null, null));
            Assert.ThrowsException<ArgumentException>(() => new UpdateToDoStatusReasonPortalLogic(mockLogger.Object, null, null));
            Assert.ThrowsException<ArgumentException>(() => new UpdateToDoStatusReasonPortalLogic(mockLogger.Object, null, xrmFakedContext.GetFakedOrganizationService()));
            #endregion Assert

        }
        private Entity CreateToDoEntity(Models.Contact student, bool cancomplete)
        {
            return new cmc_todo()
            {
                Id = Guid.NewGuid(),
                cmc_assignedtostudentid = student.ToEntityReference(),
                cmc_readunread = new OptionSetValue((int)cmc_readunread.Unread),
                cmc_studentcancomplete = cancomplete,
                statecode = cmc_todoState.Active,
                statuscode = cmc_todo_statuscode.Incomplete,
                cmc_requiredoptional = new OptionSetValue((int)cmc_requiredoptional.Optional)
            };
        }
        private Entity CreateToDoEntity1(Models.Contact student, bool cancomplete)
        {
            return new cmc_todo()
            {
                Id = Guid.NewGuid(),
                //cmc_assignedtostudentid = student.ToEntityReference(),
                cmc_readunread = new OptionSetValue((int)cmc_readunread.Unread),
                cmc_studentcancomplete = cancomplete,
                statecode = cmc_todoState.Active,
                statuscode = cmc_todo_statuscode.Incomplete,
                cmc_requiredoptional = new OptionSetValue((int)cmc_requiredoptional.Optional)
            };
        }

        private Entity CreateToDoEntity2(Models.Contact student, bool cancomplete)
        {
            return new cmc_todo()
            {
                Id = Guid.NewGuid(),
                cmc_assignedtostudentid = student.ToEntityReference(),
                cmc_readunread = new OptionSetValue((int)cmc_readunread.Unread),
                cmc_studentcancomplete = cancomplete,
                statecode = cmc_todoState.Active,
                statuscode = cmc_todo_statuscode.Waived,
                cmc_requiredoptional = new OptionSetValue((int)cmc_requiredoptional.Optional)
            };
        }
        private Entity CreateToDoEntity3(Models.Contact student, bool cancomplete)
        {
            return new cmc_todo()
            {
                Id = Guid.NewGuid(),
                cmc_assignedtostudentid = student.ToEntityReference(),
                cmc_readunread = new OptionSetValue((int)cmc_readunread.Unread),
                cmc_studentcancomplete = cancomplete,
                statecode = cmc_todoState.Active,
                //statuscode = cmc_todo_statuscode.Waived,
                cmc_requiredoptional = new OptionSetValue((int)cmc_requiredoptional.Required)
            };
        }
        private Entity CreateToDoEntity4(Models.Contact student, bool cancomplete)
        {
            return new cmc_todo()
            {
                Id = Guid.NewGuid(),
                cmc_assignedtostudentid = student.ToEntityReference(),
                cmc_readunread = new OptionSetValue((int)cmc_readunread.Unread),
                cmc_studentcancomplete = cancomplete,
                statecode = cmc_todoState.Active,
                statuscode = cmc_todo_statuscode.Complete,
                cmc_requiredoptional = new OptionSetValue((int)cmc_requiredoptional.Required)
            };
        }

        private Models.Contact CreateStudentEntity()
        {
            return new Models.Contact()
            {
                Id = Guid.NewGuid(),
            };
        }
    }
}