using System;
using System.Collections.Generic;
using Cmc.Core.Xrm.ServerExtension.Logging;
using Cmc.Engage.Common.Plugins.Tests.Utilities;
using Cmc.Engage.Models;
using FakeXrmEasy;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Moq;

namespace Cmc.Engage.Common.Tests.WebSerivce
{
    [TestClass]
    public class MarkToDoAsReadPortalTest : XrmUnitTestBase
    {
        [TestCategory("Web Service"), TestCategory("Positive")]
        [TestMethod]
        public void MarkToDoAsReadPortalTest_UpdateToDo()
        {
            #region Arrange
            var student = CreateStudentEntity();
            var toDo = CreateToDoEntity(student);

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
            var mockAppointmentService = new MarkToDoAsReadLogic(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
            var stringInput = "{'ToDoIds':'" + toDo.Id + "','StudentId':'"+ student.Id + "'}";
            var markToDoAsRead = new MarkToDoAsReadPortalLogic(mockLogger.Object, mockAppointmentService, xrmFakedContext.GetFakedOrganizationService());
            markToDoAsRead.DoWork(mockExecutionContext.Object, stringInput);
            #endregion Act

            #region Assert

            xrmFakedContext.Data["cmc_todo"].TryGetValue(toDo.Id, out toDo);
            Assert.AreEqual( (int)cmc_readunread.Read, ((OptionSetValue)toDo["cmc_readunread"]).Value);
            
            #endregion Assert
        }

        [TestCategory("Web Service"), TestCategory("Negative")]
        [TestMethod]
        public void MarkToDoAsReadPortalTest_UpdateToDo_ArgumentNullException()
        {
            #region Arrange
           
            var xrmFakedContext = new XrmFakedContext();
            
            #endregion Arrange

            #region Act
            var mockLogger = new Mock<ILogger>();
                        
            #endregion Act

            #region Assert
            Assert.ThrowsException<ArgumentNullException>(() => new MarkToDoAsReadLogic(null, xrmFakedContext.GetFakedOrganizationService()));
            Assert.ThrowsException<ArgumentNullException>(() => new MarkToDoAsReadLogic(mockLogger.Object, null));
            Assert.ThrowsException<ArgumentNullException>(() => new MarkToDoAsReadPortalLogic(null, null, null));
            Assert.ThrowsException<ArgumentException>(() => new MarkToDoAsReadPortalLogic(mockLogger.Object, null, null));
            Assert.ThrowsException<ArgumentNullException>(() => new MarkToDoAsReadPortalLogic(mockLogger.Object, null, xrmFakedContext.GetFakedOrganizationService()));
            #endregion Assert
        }

        [TestCategory("Web Service"), TestCategory("Negative")]
        [TestMethod]
        public void MarkToDoAsReadPortalTest_UpdateToDo_toDoIdIsNull()
        {
            #region Arrange
            var student = CreateStudentEntity();
            var toDo = CreateToDoEntity(student);

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
            var mockAppointmentService = new MarkToDoAsReadLogic(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
            var stringInput = "{'ToDoIds':'','StudentId':'" + student.Id + "'}";
            var markToDoAsRead = new MarkToDoAsReadPortalLogic(mockLogger.Object, mockAppointmentService, xrmFakedContext.GetFakedOrganizationService());
            #endregion Act

            #region Assert

            Assert.IsFalse((bool)markToDoAsRead.DoWork(mockExecutionContext.Object, stringInput));

            #endregion Assert
        }

        [TestCategory("Web Service"), TestCategory("Negative")]
        [TestMethod]
        public void MarkToDoAsReadPortalTest_UpdateToDo_StudentIdIsNull()
        {
            #region Arrange
            var student = CreateStudentEntity();
            var toDo = CreateToDoEntity(student);

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
            var mockAppointmentService = new MarkToDoAsReadLogic(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
            var stringInput = "{'ToDoIds':'" + toDo.Id + "','StudentId':''}";
            var markToDoAsRead = new MarkToDoAsReadPortalLogic(mockLogger.Object, mockAppointmentService, xrmFakedContext.GetFakedOrganizationService());
            #endregion Act

            #region Assert

            Assert.IsFalse((bool)markToDoAsRead.DoWork(mockExecutionContext.Object, stringInput));

            #endregion Assert
        }

        [TestCategory("Web Service"), TestCategory("Positive")]
        [TestMethod]
        public void MarkToDoAsReadPortalTest_UpdateToDo_MarkToDosAsRead_toDoIdIsNull()
        {
            #region Arrange
            var student = CreateStudentEntity();
            var toDo = CreateToDoEntity(student);
            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>()
            {
                student,
                //toDo
            });
            var mockServiceProvider = InitializeMockService(xrmFakedContext, toDo, Operation.RetrieveMultiple);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            #endregion Arrange

            #region Act
            var mockLogger = new Mock<ILogger>();
            var mockAppointmentService = new MarkToDoAsReadLogic(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
            var stringInput = "{'ToDoIds':'" + toDo.Id + "','StudentId':'" + student.Id + "'}";
            var markToDoAsRead = new MarkToDoAsReadPortalLogic(mockLogger.Object, mockAppointmentService, xrmFakedContext.GetFakedOrganizationService());
            #endregion Act

            #region Assert

            Assert.IsTrue((bool)markToDoAsRead.DoWork(mockExecutionContext.Object, stringInput));

            #endregion Assert
        }

        private Entity CreateToDoEntity(Models.Contact student)
        {
            return new cmc_todo()
            {
                Id = Guid.NewGuid(),
                cmc_assignedtostudentid = student.ToEntityReference(),
                cmc_readunread = new OptionSetValue((int)cmc_readunread.Unread)
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
