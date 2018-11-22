using System;
using System.Collections.Generic;
using Cmc.Core.Xrm.ServerExtension.Logging;
using Cmc.Engage.Common;
using Cmc.Engage.Common.Plugins.Tests.Utilities;
using FakeXrmEasy;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Moq;

namespace Cmc.Engage.Communication.Tests.Appointment.Plugin
{
    [TestClass]
    public class DeleteStaffAppointmentTest : XrmUnitTestBase
    {
        [TestCategory("Web Service"), TestCategory("Positive")]
        [TestMethod]
        public void DeleteStaffAppointment_Test()
        {
            #region ARRANGE
            var ownerInstance = PrepareOwnerInstance();
            var appointmentInstance = PrepareAppointmentInstance(ownerInstance);

            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>()
            {
                ownerInstance,
                appointmentInstance
            });

            //Initialize the Mock Service
            var mockServiceProvider = InitializeMockService(xrmFakedContext, appointmentInstance, Operation.cmc_DeleteStaffAppointment);
            AddInputParameters(mockServiceProvider, "AppointmentId", appointmentInstance.Id.ToString());
            //Fetch the Mock Execution Context
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            //Mock External Dependecies

            #endregion

            #region ACT
            //Instantiate the Class with the mocked dependencies.
            //Need to mock all the dependency Injections passed to the services constructor
            //Mock the ILogger
            var mockLogger = new Mock<ILogger>();
            var mockILanguageService = new Mock<ILanguageService>();
            //Mock the IOrganization
            //var mockOrganizationServices = new Mock<IOrganizationService>();
            //mockOrganizationServices
            //    .Setup(r => r.Retrieve(It.IsAny<string>(), appointmentInstance.Id, It.IsAny<ColumnSet>()))
            //    .Returns(appointmentInstance);
            var appointmentService = new AppointmentService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService(), mockILanguageService.Object);
            appointmentService.DeleteStaffAppointment(mockExecutionContext.Object);
            #endregion

            #region ASSERT
            var mockPluginExecutionContext = (IPluginExecutionContext)mockServiceProvider.Object.GetService(typeof(IPluginExecutionContext));
            var result = mockPluginExecutionContext.OutputParameters["StaffAppointmentsJson"];
            Assert.IsNotNull(result);

            #endregion
        }

        private Entity PrepareAppointmentInstance(Entity ownerInstanceEntity)
        {
            var appointmentInstance = new Entity("appointment", Guid.NewGuid())
            {
                ["scheduledstart"] = DateTime.Today.Date,
                ["scheduledend"] = DateTime.Today.AddDays(+5).Date,
                ["subject"] = "Test Appointment",
                ["statuscode"] = true,
                ["ownerid"] = new EntityReference("owner", ownerInstanceEntity.Id)
            };
            return appointmentInstance;
        }

        private Entity PrepareOwnerInstance()
        {
            var ownerInstance = new Entity("owner", Guid.NewGuid())
            {

            };
            return ownerInstance;
        }
    }
}
