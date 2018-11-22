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

namespace Cmc.Engage.Communication.Tests.Appointment.Activity
{
    [TestClass]
    public class RetrieveDepartmentPhoneNumberActivityTest: XrmUnitTestBase
    {
        [TestCategory("Activity"), TestCategory("Positive")]
        [TestMethod]
        public void RetrieveDepartmentPhoneNumberService_Validating_Retriving_PhoneNumber()
        {
            #region ARRANGE
            var creatingdepartment = PreparingDepartment();
            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>()
            {
                creatingdepartment
            });

            #endregion

            #region ACT
            var mockLogger = new Mock<ILogger>();
            var mockILanguageService = new Mock<ILanguageService>();
            var mockAppointmentService = new AppointmentService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService(), mockILanguageService.Object);
            var value = mockAppointmentService.RetrieveDepartmentPhoneNumberService(new List<object>()
            {
                creatingdepartment.ToEntityReference()
            });
            #endregion

            #region ASSERT

            Assert.AreEqual("1234567890", value);
            #endregion
        }

        [TestCategory("Activity"), TestCategory("Negative")]
        [TestMethod]
        public void RetrieveDepartmentPhoneNumberService_Validating_Retriving_PhoneNumber_Deptid_null()
        {
            #region ARRANGE
            
            var xrmFakedContext = new XrmFakedContext();
            
            #endregion

            #region ACT
            var mockLogger = new Mock<ILogger>();
            var mockILanguageService = new Mock<ILanguageService>();
            var mockAppointmentService = new AppointmentService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService(), mockILanguageService.Object);
            var value = mockAppointmentService.RetrieveDepartmentPhoneNumberService(new List<object>());
            #endregion

            #region ASSERT

            Assert.IsNull(value);
            #endregion
        }

        #region DATA PREPARATION
        private cmc_department PreparingDepartment()
        {
            var department = new cmc_department()
            {
                cmc_departmentId = Guid.NewGuid(),
                cmc_phonenumber= "1234567890"

            };
            return department;
        }
        #endregion
    }
}
