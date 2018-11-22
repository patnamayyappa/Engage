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

namespace Cmc.Engage.Communication.Tests.Appointment.Plugin
{
    [TestClass]
    public class CreateStaffAppointmentTest:XrmUnitTestBase
    {
        [TestCategory("Web Service"), TestCategory("Positive")]
        [TestMethod]
        public void CreateStaffAppointment_RetriveStaffAppointmentsJson()
        {
            #region ARRANGE

            var contact = PrepareContact();
            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>()
            {
                contact
            });

            //Initialize the Mock Service
            var mockServiceProvider = InitializeMockService(xrmFakedContext, contact, Operation.Create);
            //Adding Input Parameter
            AddInputParameters(mockServiceProvider, "ContactId", contact.ContactId.ToString());
            AddInputParameters(mockServiceProvider, "UserId", Guid.NewGuid().ToString());
            AddInputParameters(mockServiceProvider, "StartDate", DateTime.Now.ToString());
            AddInputParameters(mockServiceProvider, "EndDate", DateTime.Now.ToString());
            AddInputParameters(mockServiceProvider, "LocationId", Guid.NewGuid().ToString());
            AddInputParameters(mockServiceProvider, "Title", "Test Title .");
            AddInputParameters(mockServiceProvider, "Description", "Test Discription");
          
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);

            //Mock the ILogger
            var mockLogger = new Mock<ILogger>();
            var mockILanguageService = new Mock<ILanguageService>();
            #endregion ARRANGE
            #region ACT 
            var appointmentService = new AppointmentService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService(), mockILanguageService.Object);
            appointmentService.CreateStaffAppointment(mockExecutionContext.Object);

            #endregion ACT
            #region ASSERT

            var mockPluginExecutionContext = (IPluginExecutionContext)mockServiceProvider.Object.GetService(typeof(IPluginExecutionContext));

            var result = mockPluginExecutionContext.OutputParameters["StaffAppointmentsJson"];

            Assert.IsNotNull(result);

            #endregion ASSERT
        }

        private Contact PrepareContact()
        {
            var contact=new Contact()
            {
                Id = Guid.NewGuid()
            };
            return contact;
        }
    }
}
