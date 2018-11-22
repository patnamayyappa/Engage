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
    public class RetrieveStaffAppointmentsTest : XrmUnitTestBase
    {
        [TestCategory("Web Service"), TestCategory("Positive")]
        [TestMethod]
        public void RetrieveStaffAppointments_RetrieveStaffAppointmentsByUser_staffAppoinmentJson()
        {
            #region ARRANGE
            
            // Creating entities 
            var contact = PreapareContact();
            var systemUser = PrepareSystemUser();
            var appointment = PrepareAppointment();
            var activityParty = PrepareActivityParty(systemUser.Id, appointment.ToEntityReference());
            var activityParty1 = PrepareActivityParty1(contact.Id, appointment.ToEntityReference());

            List<ActivityParty> listActivityPartyInstance = new List<ActivityParty>();
            listActivityPartyInstance.Add(activityParty);
            listActivityPartyInstance.Add(activityParty1);

            var appointmentInstance = AppointmentInstance(listActivityPartyInstance, appointment, systemUser.Id);
            
            //Creating Faked Context 
            var xrmFakedContext = new XrmFakedContext();

            xrmFakedContext.Initialize(new List<Entity>()
            {
                contact,
                appointment,
                systemUser,
                activityParty,
                appointmentInstance,
                activityParty1
            });

            //Initialize the Mock Service
            var mockServiceProvider = InitializeMockService(xrmFakedContext, contact, Operation.cmc_RetrieveStaffAppointments);
            // Adding Input parameter 
            AddInputParameters(mockServiceProvider, "ContactId", contact.Id.ToString());
            AddInputParameters(mockServiceProvider, "UserId", systemUser.Id.ToString());

            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);

            //Mock the ILogger
            var mockLogger = new Mock<ILogger>();
            var mockILanguageService = new Mock<ILanguageService>();
            #endregion ARRANGE

            #region ACT

            var appointmentService = new AppointmentService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService(), mockILanguageService.Object);

            appointmentService.RetrieveStaffAppointments(mockExecutionContext.Object);

            #endregion ACT

            #region ASSERT
           
            var mockPluginExecutionContext = (IPluginExecutionContext)mockServiceProvider.Object.GetService(typeof(IPluginExecutionContext));

            var result   = mockPluginExecutionContext.OutputParameters["StaffAppointmentsJson"];

            Assert.IsNotNull(result);
            #endregion ASSERT
            
        }

        [TestCategory("Plugin"), TestCategory("Negative")]
        [TestMethod]
        public void RetrieveStaffAppointments_RetrieveStaffAppointmentsByUser_staffAppoinmentJson_Exception()
        {
            #region ARRANGE
            
            //Creating Faked Context 
            var xrmFakedContext = new XrmFakedContext();

            xrmFakedContext.Initialize(new List<Entity>());

            //Initialize the Mock Service
            var mockServiceProvider = InitializeMockService(xrmFakedContext, null, Operation.cmc_RetrieveStaffAppointments);
            // Adding Input parameter 
            AddInputParameters(mockServiceProvider, "ContactId", null);
            AddInputParameters(mockServiceProvider, "UserId", null);

            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);

            //Mock the ILogger
            var mockLogger = new Mock<ILogger>();
            var mockILanguageService = new Mock<ILanguageService>();
            #endregion ARRANGE

            #region ACT

            var appointmentService = new AppointmentService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService(), mockILanguageService.Object);

            #endregion ACT

            #region ASSERT
            Assert.ThrowsException<InvalidPluginExecutionException>(() => appointmentService.RetrieveStaffAppointments(mockExecutionContext.Object));
            #endregion ASSERT

        }


        private ActivityParty PrepareActivityParty(Guid systemuserGuid, EntityReference appointment)
        {
            var activityPatyInstance = new ActivityParty()
            {
                Id = Guid.NewGuid(),
                ActivityId = new EntityReference("appointment", appointment.Id),
                ParticipationTypeMask = activityparty_participationtypemask.Requiredattendee,
                PartyId = new EntityReference("systemuserid", systemuserGuid),                
            };
            return activityPatyInstance;
        }

        private ActivityParty PrepareActivityParty1(Guid contactGuid, EntityReference appointment)
        {
            var activityPatyInstance = new ActivityParty()
            {
                Id = Guid.NewGuid(),
                ActivityId = new EntityReference("appointment", appointment.Id),
                ParticipationTypeMask = activityparty_participationtypemask.Requiredattendee,
                PartyId = new EntityReference("contact", contactGuid),
            };
            return activityPatyInstance;
        }

        private Entity PrepareAppointment()           
        {
            var appointment = new Models.Appointment()
            {
                Id = Guid.NewGuid()
            };
            return appointment;
        }
        private Entity AppointmentInstance(List<ActivityParty> activityParties, Entity appointment, Guid systemuserid)
        {
            var apt = new Models.Appointment()
            {
                Id = appointment.Id,
                RequiredAttendees = activityParties,
                OwnerId = new EntityReference("systemuser", systemuserid),
                ScheduledStart = DateTime.Today,
                ScheduledEnd = DateTime.Today.AddDays(5),
                Subject = "Math",
                Description = "Unit Test",
                StatusCode = appointment_statuscode.Free,
               
            };
            return apt;
        }
        private Entity PrepareSystemUser()
        {
            var systemUser=new SystemUser()
            {
                SystemUserId = Guid.NewGuid(),
            };
            return systemUser;
        }

        private Contact PreapareContact()
        {
            var contact = new Contact()
            {
                Id = Guid.NewGuid(),
            };
            return contact;
        }
    }
}
