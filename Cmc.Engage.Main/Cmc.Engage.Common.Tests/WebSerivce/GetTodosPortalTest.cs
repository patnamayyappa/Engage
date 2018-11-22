using System;
using System.Collections.Generic;
using Cmc.Core.Xrm.ServerExtension.Logging;
using Cmc.Engage.Common.Plugins.Tests.Utilities;
using Cmc.Engage.Models.Tests;
using FakeXrmEasy;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Moq;

namespace Cmc.Engage.Common.Tests.WebSerivce
{
    [TestClass]
    public class GetTodosPortalTest : XrmUnitTestBase
    {
        [TestCategory("Web Service"), TestCategory("Positive")]
        [TestMethod]
        public void GetTodosPortalLogicTest_ReturnToDOs_Incomplete()
        {
            #region Arrange
            var organizationEntity = CreateOrganizationEntity();
            var languageEntity = CreatePortalLanguageEntity();
            var student = CreateStudentEntity(languageEntity);
            var toDo = CreateToDoEntity(student);
            var academicPeriod = CreateAcademicPeriodEntity();
            var academicProgress = CreateAcademicProgressEntity(academicPeriod, student);

            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>()
                {
                    languageEntity,
                    organizationEntity,
                    academicProgress,
                    student,
                    toDo,
                    academicPeriod

                });
            var mockServiceProvider = InitializeMockService(xrmFakedContext, toDo, Operation.RetrieveMultiple);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            #endregion Arrange

            #region Act
            var mockLogger = new Mock<ILogger>();
            var portalUserLanguageService = new GetPortalUserLanguageCode(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
            var todoService = new GetTodosLogic(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
            var mockILanguageService = new Mock<ILanguageService>();
            var stringInput = "{'StudentId':'" + student.Id + "','WebsiteId':'','StatusCode':'" + (int)cmc_todo_statuscode.Incomplete + "'}";
            var portalTodoLogic = new GetTodosPortalLogic(mockLogger.Object,
                xrmFakedContext.GetFakedOrganizationService(), portalUserLanguageService, todoService,
                mockILanguageService.Object);

            var data = portalTodoLogic.DoWork(mockExecutionContext.Object, stringInput) as Dictionary<Guid, GetTodosPortalLogic.Output>;
            #endregion Act

            #region Assert
            Assert.IsTrue(data.ContainsKey(toDo.Id));
            #endregion Assert
        }

        [TestCategory("Web Service"), TestCategory("Negative")]
        [TestMethod]
        public void GetTodosPortalLogicTest_ReturnToDOs_NegativeScenario()
        {
            #region Arrange

            var xrmFakedContext = new XrmFakedContext();


            #endregion Arrange

            #region Act
            var mockLogger = new Mock<ILogger>();
            var portalUserLanguageService = new GetPortalUserLanguageCode(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
            var todoService = new GetTodosLogic(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());

            #endregion Act

            #region Assert
            Assert.ThrowsException<ArgumentNullException>(() => new GetTodosLogic(null, null));
            Assert.ThrowsException<ArgumentNullException>(() => new GetTodosLogic(mockLogger.Object, null));
            Assert.ThrowsException<ArgumentNullException>(() => new GetPortalUserLanguageCode(null, null));
            Assert.ThrowsException<ArgumentNullException>(() => new GetPortalUserLanguageCode(mockLogger.Object, null));
            Assert.ThrowsException<ArgumentNullException>(() => new GetTodosPortalLogic(null, null, null, null, null));
            Assert.ThrowsException<ArgumentNullException>(() => new GetTodosPortalLogic(mockLogger.Object, null, null, null, null));
            Assert.ThrowsException<ArgumentNullException>(() => new GetTodosPortalLogic(mockLogger.Object, null, portalUserLanguageService, null, null));
            Assert.ThrowsException<ArgumentNullException>(() => new GetTodosPortalLogic(mockLogger.Object, null, portalUserLanguageService, todoService, null));

            #endregion Assert
        }

        [TestCategory("Web Service"), TestCategory("Positive")]
        [TestMethod]
        public void GetTodosPortalLogicTest_ReturnToDOs_Complete()
        {
            #region Arrange
            var organizationEntity = CreateOrganizationEntity();
            var languageEntity = CreatePortalLanguageEntity();
            var student = CreateStudentEntity1(languageEntity);
            var toDo = CreateToDoEntity2(student);
            var academicPeriod = CreateAcademicPeriodEntity();
            var academicProgress = CreateAcademicProgressEntity(academicPeriod, student);

            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>()
                {
                    languageEntity,
                    organizationEntity,
                    academicProgress,
                    student,
                    toDo,
                    academicPeriod

                });
            var mockServiceProvider = InitializeMockService(xrmFakedContext, toDo, Operation.RetrieveMultiple);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            #endregion Arrange

            #region Act
            var mockLogger = new Mock<ILogger>();
            var portalUserLanguageService = new GetPortalUserLanguageCode(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
            var todoService = new GetTodosLogic(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
            var mockILanguageService = new Mock<ILanguageService>();
            var stringInput = "{'StudentId':'" + student.Id + "','WebsiteId':'','StatusCode':'" + (int)cmc_todo_statuscode.Complete + "'}";
            var portalTodoLogic = new GetTodosPortalLogic(mockLogger.Object,
                xrmFakedContext.GetFakedOrganizationService(), portalUserLanguageService, todoService,
                mockILanguageService.Object);

            var data = portalTodoLogic.DoWork(mockExecutionContext.Object, stringInput) as Dictionary<Guid, GetTodosPortalLogic.Output>;
            #endregion Act

            #region Assert
            Assert.IsTrue(data.ContainsKey(toDo.Id));
            #endregion Assert
        }

        [TestCategory("Web Service"), TestCategory("Positive")]
        [TestMethod]
        public void GetTodosPortalLogicTest_ReturnToDOs_Canceled()
        {
            #region Arrange
            var organizationEntity = CreateOrganizationEntity();
            var languageEntity = CreatePortalLanguageEntity();
            var student = CreateStudentEntity(languageEntity);
            var toDo = CreateToDoEntity3(student);
            var academicPeriod = CreateAcademicPeriodEntity();
            var academicProgress = CreateAcademicProgressEntity(academicPeriod, student);

            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>()
                {
                    languageEntity,
                    organizationEntity,
                    academicProgress,
                    student,
                    toDo,
                    academicPeriod

                });
            var mockServiceProvider = InitializeMockService(xrmFakedContext, toDo, Operation.RetrieveMultiple);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            #endregion Arrange

            #region Act
            var mockLogger = new Mock<ILogger>();
            var portalUserLanguageService = new GetPortalUserLanguageCode(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
            var todoService = new GetTodosLogic(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
            var mockILanguageService = new Mock<ILanguageService>();
            var stringInput = "{'StudentId':'" + student.Id + "','WebsiteId':'','StatusCode':'" + (int)cmc_todo_statuscode.Canceled + "'}";
            var portalTodoLogic = new GetTodosPortalLogic(mockLogger.Object,
                xrmFakedContext.GetFakedOrganizationService(), portalUserLanguageService, todoService,
                mockILanguageService.Object);

            var data = portalTodoLogic.DoWork(mockExecutionContext.Object, stringInput) as Dictionary<Guid, GetTodosPortalLogic.Output>;
            #endregion Act

            #region Assert
            Assert.IsTrue(data.ContainsKey(toDo.Id));
            #endregion Assert
        }

        [TestCategory("Web Service"), TestCategory("Positive")]
        [TestMethod]
        public void GetTodosPortalLogicTest_ReturnToDOs_WebsiteLanguage()
        {
            #region Arrange
            var adx_Website = new Entity("adx_Website") { Id = Guid.NewGuid() };
            var organizationEntity = CreateOrganizationEntity();
            var languageEntity = CreatePortalLanguageEntity();
            var student = CreateStudentEntity1(languageEntity);
            var toDo = CreateToDoEntity(student);
            var academicPeriod = CreateAcademicPeriodEntity();
            var academicProgress = CreateAcademicProgressEntity(academicPeriod, student);
            var websiteEntity = WebsiteLanguageEntity(languageEntity, adx_Website);


            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>()
                {

                    languageEntity,
                    organizationEntity,
                    academicProgress,
                    student,
                    toDo,
                    academicPeriod,
                    websiteEntity,
                });
            var mockServiceProvider = InitializeMockService(xrmFakedContext, toDo, Operation.RetrieveMultiple);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            #endregion Arrange

            #region Act
            var mockLogger = new Mock<ILogger>();
            var portalUserLanguageService = new GetPortalUserLanguageCode(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
            var todoService = new GetTodosLogic(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
            var mockILanguageService = new Mock<ILanguageService>();
            var stringInput = "{'StudentId':'" + student.Id + "','WebsiteId':'" + adx_Website.Id + "','StatusCode':'1'}";
            var portalTodoLogic = new GetTodosPortalLogic(mockLogger.Object,
                xrmFakedContext.GetFakedOrganizationService(), portalUserLanguageService, todoService,
                mockILanguageService.Object);
            var data = portalTodoLogic.DoWork(mockExecutionContext.Object, stringInput) as Dictionary<Guid, GetTodosPortalLogic.Output>;
            #endregion Act

            #region Assert

            Assert.IsTrue(data.ContainsKey(toDo.Id));
            #endregion Assert
        }

        [TestCategory("Web Service"), TestCategory("Positive")]
        [TestMethod]
        public void GetTodosPortalLogicTest_ReturnToDOs_InputIsNull()
        {
            #region Arrange
            var adx_Website = new Entity("adx_Website") { Id = Guid.NewGuid() };
            var organizationEntity = CreateOrganizationEntity();
            var languageEntity = CreatePortalLanguageEntity();
            var student = CreateStudentEntity1(languageEntity);
            var toDo = CreateToDoEntity(student);
            var academicPeriod = CreateAcademicPeriodEntity();
            var academicProgress = CreateAcademicProgressEntity(academicPeriod, student);
            var websiteEntity = WebsiteLanguageEntity(languageEntity, adx_Website);


            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>()
                {

                    languageEntity,
                    organizationEntity,
                    academicProgress,
                    student,
                    toDo,
                    academicPeriod,
                    websiteEntity,
                });
            var mockServiceProvider = InitializeMockService(xrmFakedContext, toDo, Operation.RetrieveMultiple);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            #endregion Arrange

            #region Act
            var mockLogger = new Mock<ILogger>();
            var portalUserLanguageService = new GetPortalUserLanguageCode(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
            var todoService = new GetTodosLogic(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
            var mockILanguageService = new Mock<ILanguageService>();
            var stringInput = "{'StudentId':'','WebsiteId':'','StatusCode':'1'}";
            var portalTodoLogic = new GetTodosPortalLogic(mockLogger.Object,
                xrmFakedContext.GetFakedOrganizationService(), portalUserLanguageService, todoService,
                mockILanguageService.Object);
            var data = portalTodoLogic.DoWork(mockExecutionContext.Object, stringInput) as Dictionary<Guid, GetTodosPortalLogic.Output>;
            #endregion Act

            #region Assert

            Assert.IsNull(data);
            #endregion Assert
        }

        [TestCategory("Web Service"), TestCategory("Positive")]
        [TestMethod]
        public void GetTodosPortalLogicTest_ReturnToDOs_AcademicPeriod_IsNull()
        {
            #region Arrange
            var organizationEntity = CreateOrganizationEntity();
            var languageEntity = CreatePortalLanguageEntity();
            var student = CreateStudentEntity(languageEntity);
            var toDo = CreateToDoEntity(student);
            toDo.cmc_duedate = DateTime.Parse("01-01-1753");
            var academicPeriod = CreateAcademicPeriodEntity1();
            var academicProgress = CreateAcademicProgressEntity1(student);

            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>()
                {
                    languageEntity,
                    organizationEntity,
                    academicProgress,
                    student,
                    toDo,
                    academicPeriod

                });
            var mockServiceProvider = InitializeMockService(xrmFakedContext, toDo, Operation.RetrieveMultiple);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            #endregion Arrange

            #region Act
            var mockLogger = new Mock<ILogger>();
            var portalUserLanguageService = new GetPortalUserLanguageCode(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
            var todoService = new GetTodosLogic(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
            var mockILanguageService = new Mock<ILanguageService>();
            var stringInput = "{'StudentId':'" + student.Id + "','WebsiteId':'','StatusCode':'" + (int)cmc_todo_statuscode.Incomplete + "'}";
            var portalTodoLogic = new GetTodosPortalLogic(mockLogger.Object,
                xrmFakedContext.GetFakedOrganizationService(), portalUserLanguageService, todoService,
                mockILanguageService.Object);

            var data = portalTodoLogic.DoWork(mockExecutionContext.Object, stringInput) as Dictionary<Guid, GetTodosPortalLogic.Output>;
            #endregion Act

            #region Assert
            Assert.IsTrue(data.ContainsKey(toDo.Id));
            #endregion Assert
        }

        [TestCategory("Web Service"), TestCategory("Positive")]
        [TestMethod]
        public void GetTodosPortalLogicTest_ReturnToDOs_StatusCodeIsNull()
        {
            #region Arrange
            var organizationEntity = CreateOrganizationEntity();
            var languageEntity = CreatePortalLanguageEntity();
            var student = CreateStudentEntity(languageEntity);
            var toDo = CreateToDoEntity(student);
            toDo.cmc_duedate = DateTime.Parse("01-01-1753");
            var academicPeriod = CreateAcademicPeriodEntity1();
            var academicProgress = CreateAcademicProgressEntity1(student);

            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>()
                {
                    languageEntity,
                    organizationEntity,
                    academicProgress,
                    student,
                    toDo,
                    academicPeriod

                });
            var mockServiceProvider = InitializeMockService(xrmFakedContext, toDo, Operation.RetrieveMultiple);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            #endregion Arrange

            #region Act
            var mockLogger = new Mock<ILogger>();
            var portalUserLanguageService = new GetPortalUserLanguageCode(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
            var todoService = new GetTodosLogic(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
            var mockILanguageService = new Mock<ILanguageService>();
            var stringInput = "{'StudentId':'" + student.Id + "','WebsiteId':'','StatusCode':'3'}";
            var portalTodoLogic = new GetTodosPortalLogic(mockLogger.Object,
                xrmFakedContext.GetFakedOrganizationService(), portalUserLanguageService, todoService,
                mockILanguageService.Object);

            var data = portalTodoLogic.DoWork(mockExecutionContext.Object, stringInput) as Dictionary<Guid, GetTodosPortalLogic.Output>;
            #endregion Act

            #region Assert
            Assert.IsTrue(data.ContainsKey(toDo.Id));
            #endregion Assert
        }


        private cmc_todo CreateToDoEntity(Models.Contact student)
        {
            return new cmc_todo()
            {
                Id = Guid.NewGuid(),
                cmc_assignedtostudentid = student.ToEntityReference(),
                cmc_todoname = "Test Data",
                cmc_readunread = new OptionSetValue((int)cmc_readunread.Unread),
                cmc_ownershiptype = new OptionSetValue((int)cmc_ownershiptype.StudentOwned),
                statuscode = cmc_todo_statuscode.Incomplete,
                cmc_requiredoptional = new OptionSetValue((int)cmc_requiredoptional.Required),
                cmc_duedate = DateTime.Now
            };
        }
        //01-01-1753
        private cmc_todo CreateToDoEntity2(Models.Contact student)
        {
            return new cmc_todo()
            {
                Id = Guid.NewGuid(),
                cmc_assignedtostudentid = student.ToEntityReference(),
                cmc_todoname = "Test Data",
                cmc_readunread = new OptionSetValue((int)cmc_readunread.Unread),
                cmc_ownershiptype = new OptionSetValue((int)cmc_ownershiptype.StudentOwned),
                statuscode = cmc_todo_statuscode.Complete,
                cmc_requiredoptional = new OptionSetValue((int)cmc_requiredoptional.Required),
                cmc_duedate = DateTime.Now
            };
        }

        private cmc_todo CreateToDoEntity3(Models.Contact student)
        {
            return new cmc_todo()
            {
                Id = Guid.NewGuid(),
                cmc_assignedtostudentid = student.ToEntityReference(),
                cmc_todoname = "Test Data",
                cmc_readunread = new OptionSetValue((int)cmc_readunread.Unread),
                cmc_ownershiptype = new OptionSetValue((int)cmc_ownershiptype.StudentOwned),
                statuscode = cmc_todo_statuscode.Canceled,
                cmc_requiredoptional = new OptionSetValue((int)cmc_requiredoptional.Required),
                cmc_duedate = DateTime.Now
            };
        }

        private Models.Contact CreateStudentEntity(Entity language)
        {
            return new Models.Contact()
            {
                Id = Guid.NewGuid(),
                ["adx_preferredlanguageid"] = language.ToEntityReference()
            };
        }

        private Models.Contact CreateStudentEntity1(Entity language)
        {
            return new Models.Contact()
            {
                Id = Guid.NewGuid(),
                //["adx_preferredlanguageid"] = language.ToEntityReference()
            };
        }

        private mshied_academicperiod CreateAcademicPeriodEntity1()
        {
            return new mshied_academicperiod()
            {
                Id = Guid.NewGuid(),
                //cmc_startdate = DateTime.Now.AddDays(-5),
                //cmc_enddate = DateTime.Now.AddDays(5),

            };
        }
        private mshied_academicperiod CreateAcademicPeriodEntity()
        {
            return new mshied_academicperiod()
            {
                Id = Guid.NewGuid(),
                mshied_StartDate = DateTime.Now.AddDays(-5),
                mshied_EndDate = DateTime.Now.AddDays(5),

            };
        }

        private Organization CreateOrganizationEntity()
        {
            return new Organization()
            {
                Id = Guid.NewGuid(),
                languagecode = 1033

            };
        }
        private adx_portallanguage CreatePortalLanguageEntity()
        {
            return new adx_portallanguage()
            {
                Id = Guid.NewGuid(),
                adx_lcid = 1033

            };
        }

        private adx_websitelanguage WebsiteLanguageEntity(Entity portalLanguage, Entity adx_Website)
        {
            return new adx_websitelanguage()
            {
                Id = Guid.NewGuid(),
                adx_WebsiteId = adx_Website.ToEntityReference(),
                adx_PortalLanguageId = portalLanguage.ToEntityReference(),
                ["adx_lcid"] = 1033

            };
        }
        private mshied_academicperioddetails CreateAcademicProgressEntity(mshied_academicperiod academicperiod, Models.Contact student)
        {
            return new mshied_academicperioddetails()
            {
                Id = Guid.NewGuid(),
                mshied_AcademicPeriodID = academicperiod.ToEntityReference(),
                mshied_StudentId = student.ToEntityReference(),
                statecode = 0
            };

        }
        private mshied_academicperioddetails CreateAcademicProgressEntity1(Models.Contact student)
        {
            return new mshied_academicperioddetails()
            {
                Id = Guid.NewGuid(),
                //cmc_academicperiodid = academicperiod.ToEntityReference(),
                mshied_StudentId = student.ToEntityReference(),
                statecode = 0
            };

        }

    }
}
