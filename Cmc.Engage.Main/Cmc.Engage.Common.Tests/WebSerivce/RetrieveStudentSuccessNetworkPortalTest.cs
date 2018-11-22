using System;
using System.Collections.Generic;
using System.IO;
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
    public class RetrieveStudentSuccessNetworkPortalTest: XrmUnitTestBase
    {
        [TestCategory("Web Service"), TestCategory("Positive")]
        [TestMethod]
        public void RetrieveStudentSuccessNetworkPortalTest_ReturnStudentSuccessNetworkStaff()
        {

            #region ARRANGE
            var student = CreateStudentEntity();
            var staff = PrepareStaffEntity();
            var role = PrepareRoleEntity();
            var successNetwork = CreateSuccessNetworkEntity(staff, role, student);

            var xrmFakedContext = new XrmFakedContext();
            
            xrmFakedContext.Initialize(new List<Entity>()
            {
                role,
                student,
                staff,
                successNetwork
            });
            var mockServiceProvider = InitializeMockService(xrmFakedContext, student, Operation.RetrieveMultiple);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            #endregion

            #region Act

            var mockLogger = new Mock<ILogger>();
            var mockSuccessNetworkService = new RetrieveStudentSuccessNetworkPortalLogic(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());

            var stringInput = "{'StudentId':'" + student.Id + "'}";
            var data = mockSuccessNetworkService.DoWork(mockExecutionContext.Object, stringInput);
            #endregion

            #region Assert
            Assert.IsNotNull(data);
            #endregion
        }

        [TestCategory("Web Service"), TestCategory("Negative")]
        [TestMethod]
        public void RetrieveStudentSuccessNetworkPortalTest_ReturnStudentSuccessNetworkStaff_StudentIdIsNull()
        {

            #region ARRANGE
            var student = CreateStudentEntity();
            var staff = PrepareStaffEntity();
            var role = PrepareRoleEntity();
            var successNetwork = CreateSuccessNetworkEntity(staff, role, student);

            var xrmFakedContext = new XrmFakedContext();

            xrmFakedContext.Initialize(new List<Entity>()
            {
                role,
                student,
                staff,
                successNetwork
            });
            var mockServiceProvider = InitializeMockService(xrmFakedContext, student, Operation.RetrieveMultiple);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            #endregion

            #region Act

            var mockLogger = new Mock<ILogger>();
            var mockSuccessNetworkService = new RetrieveStudentSuccessNetworkPortalLogic(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());

            var stringInput = "{'StudentId':''}";
            object data = mockSuccessNetworkService.DoWork(mockExecutionContext.Object, stringInput) as RetrieveStudentSuccessNetworkPortalLogic.StudentSuccessNetworkStaff;
            #endregion

            #region Assert
            Assert.IsNull(data);
            #endregion
        }

        [TestCategory("Web Service"), TestCategory("Negative")]
        [TestMethod]
        public void RetrieveStudentSuccessNetworkPortalTest_ReturnStudentSuccessNetworkStaff_ArgumentException()
        {

            #region ARRANGE
            
            var xrmFakedContext = new XrmFakedContext();

            #endregion

            #region Act

            var mockLogger = new Mock<ILogger>();

            #endregion

            #region Assert
            Assert.ThrowsException<ArgumentException>(() => new RetrieveStudentSuccessNetworkPortalLogic(null, null));
            Assert.ThrowsException<ArgumentException>(() => new RetrieveStudentSuccessNetworkPortalLogic(null, xrmFakedContext.GetFakedOrganizationService()));

            #endregion
        }

        private Models.Contact CreateStudentEntity()
        {
            return new Models.Contact()
            {
                Id = Guid.NewGuid(),
            };
        }
        private SystemUser PrepareStaffEntity()
        {
            return new SystemUser()
            {
                Id = Guid.NewGuid(),
            };
        }
        private Entity PrepareRoleEntity()
        {
            return new Entity("cmc_title", Guid.NewGuid());
        }
        private cmc_successnetwork CreateSuccessNetworkEntity(SystemUser staff, Entity role, Models.Contact student)
        {
            return new cmc_successnetwork()
            {
                Id = Guid.NewGuid(),
                cmc_staffmemberid = staff.ToEntityReference(),
                cmc_staffroleid = role.ToEntityReference(),
                cmc_studentid = student.ToEntityReference()

            };
        }
    }
}
