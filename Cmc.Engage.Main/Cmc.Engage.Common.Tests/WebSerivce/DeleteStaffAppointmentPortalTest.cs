using System;
using System.Collections.Generic;
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
    public class DeleteStaffAppointmentPortalTest : XrmUnitTestBase
    {
        [TestCategory("Web Service"), TestCategory("Positive")]
        [TestMethod]
        public void DeleteStaffAppointmentPortal_DeleteStaffAppointMent()
        {
            #region Arrange

            var staffAppointmnet = CreateStaffAppointmentEntity();
            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>()
            {
                staffAppointmnet
            });

            var mockServiceProvider = InitializeMockService(xrmFakedContext, staffAppointmnet, Operation.RetrieveMultiple);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);

            #endregion Arrange
            
            #region Act

            var mockLogger = new Mock<ILogger>();
            var mockILanguageService = new Mock<ILanguageService>();
            var mockAppointmentService = new AppointmentService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService(), mockILanguageService.Object);

            var stringInput = "{'AppointmentId':'" + staffAppointmnet.Id + "'}";
            var deleteStaffAppointmentPortalLogic = new DeleteStaffAppointmentPortalLogic(mockLogger.Object, mockAppointmentService, xrmFakedContext.GetFakedOrganizationService());
            var deletedAppointmentJson =deleteStaffAppointmentPortalLogic.DoWork(mockExecutionContext.Object, stringInput);

            #endregion Act

            #region Assert
            Assert.IsNotNull(deletedAppointmentJson);
            #endregion Assert
        }

        [TestCategory("Web Service"), TestCategory("Negative")]
        [TestMethod]
        public void DeleteStaffAppointmentPortal_DeleteStaffAppointMent_ArgumentNullException()
        {
            #region Arrange

            var xrmFakedContext = new XrmFakedContext();
            
            #endregion Arrange

            #region Act

            var mockLogger = new Mock<ILogger>();
            
            #endregion Act

            #region Assert
            Assert.ThrowsException<ArgumentException>(() => new DeleteStaffAppointmentPortalLogic(null, null, null));
            Assert.ThrowsException<ArgumentException>(() => new DeleteStaffAppointmentPortalLogic(null, null, xrmFakedContext.GetFakedOrganizationService()));
            Assert.ThrowsException<ArgumentException>(() => new DeleteStaffAppointmentPortalLogic(mockLogger.Object, null, xrmFakedContext.GetFakedOrganizationService()));
            #endregion Assert
        }

        private Appointment CreateStaffAppointmentEntity()
        {
            return new Appointment(){Id = Guid.NewGuid()};
        }
    }
}