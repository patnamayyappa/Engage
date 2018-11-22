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
    public class RetrieveNumberOfUnreadToDosPortalTest : XrmUnitTestBase
    {
        [TestCategory("Web Service"), TestCategory("Positive")]
        [TestMethod]
        public void RetrieveNumberOfUnreadToDosPortalTest_ReturnUnreadToDOCount()
        {
            #region Arrange
            var student = CreateStudentEntity();
            var toDo = CreateToDoEntity(student);
            var academicPeriod = CreateAcademicPeriodEntity();
            var academicProgress = CreateAcademicProgressEntity(academicPeriod, student);

            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>()
                {
                    academicProgress,
                    student,
                    toDo,
                    academicPeriod

                });
            var mockServiceProvider = InitializeMockService(xrmFakedContext, toDo, Operation.RetrieveMultiple);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            #endregion Arrange

            xrmFakedContext.AddRelationship("mshied_academicperiod_academicperioddetails_AcademicPeriod", new XrmFakedRelationship
            {
                RelationshipType = XrmFakedRelationship.enmFakeRelationshipType.OneToMany,
                Entity1LogicalName = "mshied_academicperiod",
                Entity1Attribute = "mshied_academicperiodid",
                Entity2LogicalName = "mshied_academicperioddetails",
                Entity2Attribute = "mshied_academicperiodid"
            });
            xrmFakedContext.AddRelationship("mshied_contact_academicperioddetails_StudentId", new XrmFakedRelationship
            {
                RelationshipType = XrmFakedRelationship.enmFakeRelationshipType.OneToMany,
                Entity1LogicalName = "contact",
                Entity1Attribute = "mshied_studentid",
                Entity2LogicalName = "mshied_academicperioddetails",
                Entity2Attribute = "mshied_studentid"
            });

            #region Act
            var mockLogger = new Mock<ILogger>();
            var mocktodoService = new RetrieveNumberOfUnreadToDosLogic(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
            var stringInput = "{'StudentId':'" + student.Id + "'}";
            var markToDoAsRead = new RetrieveNumberOfUnreadToDosPortalLogic(mockLogger.Object, mocktodoService, xrmFakedContext.GetFakedOrganizationService());
            var count = markToDoAsRead.DoWork(mockExecutionContext.Object, stringInput);
            #endregion Act

            #region Assert

            Assert.AreEqual(count,1);

            #endregion Assert
        }

        [TestCategory("Web Service"), TestCategory("Positive")]
        [TestMethod]
        public void RetrieveNumberOfUnreadToDosPortalTest_ReturnUnreadToDOCount_StudentIdIsNull()
        {
            #region Arrange
            var student = CreateStudentEntity();
            var toDo = CreateToDoEntity(student);
            var academicPeriod = CreateAcademicPeriodEntity();
            var academicProgress = CreateAcademicProgressEntity(academicPeriod, student);

            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>()
                {
                    academicProgress,
                    student,
                    toDo,
                    academicPeriod

                });
            var mockServiceProvider = InitializeMockService(xrmFakedContext, toDo, Operation.RetrieveMultiple);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            #endregion Arrange

            xrmFakedContext.AddRelationship("mshied_academicperiod_academicperioddetails_AcademicPeriod", new XrmFakedRelationship
            {
                RelationshipType = XrmFakedRelationship.enmFakeRelationshipType.OneToMany,
                Entity1LogicalName = "mshied_academicperiod",
                Entity1Attribute = "mshied_academicperiodid",
                Entity2LogicalName = "mshied_academicperioddetails",
                Entity2Attribute = "mshied_academicperiodid"
            });
            xrmFakedContext.AddRelationship("mshied_contact_academicperioddetails_StudentId", new XrmFakedRelationship
            {
                RelationshipType = XrmFakedRelationship.enmFakeRelationshipType.OneToMany,
                Entity1LogicalName = "contact",
                Entity1Attribute = "mshied_studentid",
                Entity2LogicalName = "mshied_academicperioddetails",
                Entity2Attribute = "mshied_studentid"
            });

            #region Act
            var mockLogger = new Mock<ILogger>();
            var mocktodoService = new RetrieveNumberOfUnreadToDosLogic(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
            var stringInput = "{'StudentId':''}";
            var markToDoAsRead = new RetrieveNumberOfUnreadToDosPortalLogic(mockLogger.Object, mocktodoService, xrmFakedContext.GetFakedOrganizationService());
            var count = markToDoAsRead.DoWork(mockExecutionContext.Object, stringInput);
            #endregion Act

            #region Assert

            Assert.AreEqual(count, 0);

            #endregion Assert
        }

        [TestCategory("Web Service"), TestCategory("Positive")]
        [TestMethod]
        public void RetrieveNumberOfUnreadToDosPortalTest_ReturnUnreadToDOCount_AcademicPeriodIsNull()
        {
            #region Arrange
            var student = CreateStudentEntity();
            var toDo = CreateToDoEntity(student);
            toDo.cmc_duedate = DateTime.Parse("1-1-1753");
            var academicPeriod = CreateAcademicPeriodEntity();
            academicPeriod.mshied_StartDate = null;
            academicPeriod.mshied_EndDate = null;
            var academicProgress = CreateAcademicProgressEntity(academicPeriod, student);

            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>()
                {
                    academicProgress,
                    student,
                    toDo,
                    academicPeriod

                });
            var mockServiceProvider = InitializeMockService(xrmFakedContext, toDo, Operation.RetrieveMultiple);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            #endregion Arrange

            xrmFakedContext.AddRelationship("mshied_academicperiod_academicperioddetails_AcademicPeriod", new XrmFakedRelationship
            {
                RelationshipType = XrmFakedRelationship.enmFakeRelationshipType.OneToMany,
                Entity1LogicalName = "mshied_academicperiod",
                Entity1Attribute = "mshied_academicperiodid",
                Entity2LogicalName = "mshied_academicperioddetails",
                Entity2Attribute = "mshied_academicperiodid"
            });
            xrmFakedContext.AddRelationship("mshied_contact_academicperioddetails_StudentId", new XrmFakedRelationship
            {
                RelationshipType = XrmFakedRelationship.enmFakeRelationshipType.OneToMany,
                Entity1LogicalName = "contact",
                Entity1Attribute = "mshied_studentid",
                Entity2LogicalName = "mshied_academicperioddetails",
                Entity2Attribute = "mshied_studentid"
            });

            #region Act
            var mockLogger = new Mock<ILogger>();
            var mocktodoService = new RetrieveNumberOfUnreadToDosLogic(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
            var stringInput = "{'StudentId':'" + student.Id + "'}";
            var markToDoAsRead = new RetrieveNumberOfUnreadToDosPortalLogic(mockLogger.Object, mocktodoService, xrmFakedContext.GetFakedOrganizationService());
            var count = markToDoAsRead.DoWork(mockExecutionContext.Object, stringInput);
            #endregion Act

            #region Assert

            Assert.AreEqual(count, 1);

            #endregion Assert
        }

        [TestCategory("Web Service"), TestCategory("Negative")]
        [TestMethod]
        public void RetrieveNumberOfUnreadToDosPortalTest_ReturnUnreadToDOCount_ArgumentNullException()
        {
            #region Arrange
            
            var xrmFakedContext = new XrmFakedContext();
            
            #endregion Arrange

            #region Act
            var mockLogger = new Mock<ILogger>();

            #endregion Act

            #region Assert
            Assert.ThrowsException<ArgumentException>(() => new RetrieveNumberOfUnreadToDosPortalLogic(null, null, null));
            Assert.ThrowsException<ArgumentException>(() => new RetrieveNumberOfUnreadToDosPortalLogic(mockLogger.Object, null, null));
            Assert.ThrowsException<ArgumentException>(() => new RetrieveNumberOfUnreadToDosPortalLogic(mockLogger.Object, null, xrmFakedContext.GetFakedOrganizationService()));
            Assert.ThrowsException<ArgumentNullException>(() => new RetrieveNumberOfUnreadToDosLogic(null, null));
            Assert.ThrowsException<ArgumentNullException>(() => new RetrieveNumberOfUnreadToDosLogic(mockLogger.Object, null));
            #endregion Assert
        }
        private cmc_todo CreateToDoEntity(Models.Contact student)
        {
            return new cmc_todo()
            {
                Id = Guid.NewGuid(),
                cmc_assignedtostudentid = student.ToEntityReference(),
                cmc_readunread = new OptionSetValue((int)cmc_readunread.Unread),
                cmc_ownershiptype= new OptionSetValue((int)cmc_ownershiptype.StudentOwned),
                statuscode = cmc_todo_statuscode.Incomplete,
                cmc_requiredoptional = new OptionSetValue((int)cmc_requiredoptional.Required),
                cmc_duedate = DateTime.Now
            };
        }

        private Models.Contact CreateStudentEntity()
        {
            return new Models.Contact()
            {
                Id = Guid.NewGuid(),
            };
        }

        private mshied_academicperiod CreateAcademicPeriodEntity()
        {
            return new mshied_academicperiod()
            {
                Id = Guid.NewGuid(),
                mshied_StartDate= DateTime.Now.AddDays(-5),
                mshied_EndDate = DateTime.Now.AddDays(5),

            };
        }
        private Entity CreateAcademicProgressEntity(mshied_academicperiod academicperiod, Models.Contact student)
        {
            return new Entity("mshied_academicperioddetails", Guid.NewGuid())
            {
                Id = Guid.NewGuid(),
                ["mshied_academicperiodid"] = academicperiod.Id,
                ["mshied_studentid"] = student.Id,
                ["cmc_startdate"]=DateTime.Now.AddDays(-2),
                ["cmc_enddate"]= DateTime.Now.AddDays(52),
                ["statecode"]=0
            };


        }
    }
}
