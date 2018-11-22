using System;
using System.Collections.Generic;
using Cmc.Core.Xrm.ServerExtension.Logging;
using Cmc.Engage.Common.Plugins.Tests.Utilities;
using Cmc.Engage.Communication;
using Cmc.Engage.Models;
using FakeXrmEasy;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Moq;

namespace Cmc.Engage.Common.Tests.WebSerivce
{
    [TestClass]
    public class RetrieveStaffAppointmentsPortalTest : XrmUnitTestBase
    {
        [TestCategory("Web Service"), TestCategory("Positive")]
        [TestMethod]
        public void RetrieveStaffAppointmentsPortalTest_ReturnAppointmentJson()
        {

            #region ARRANGE

            var contact = PreapareContact();
            var systemUser = PrepareSystemUser();
            var activityParty = PrepareActivityParty(systemUser.Id);

            List<ActivityParty> listActivityPartyInstance = new List<ActivityParty>();
            listActivityPartyInstance.Add(activityParty);

            var appointment = PrepareAppointment(systemUser.Id, listActivityPartyInstance, activityParty.Id);

            var xrmFakedContext = new XrmFakedContext();

            xrmFakedContext.Initialize(new List<Entity>()
            {
                contact,
                appointment,
                systemUser,
                activityParty
            });

            var mockServiceProvider = InitializeMockService(xrmFakedContext, contact, Operation.cmc_RetrieveStaffAppointments);

            var stringInput = "{'UserId':'" + systemUser.Id + "','ContactId':'" + contact.Id + "'}";

            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);

            var mockLogger = new Mock<ILogger>();
            #endregion ARRANGE

            #region ACT

            var retrieveStaffAppointment = new RetrieveStaffAppointmentsPortalLogic(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
            /*
                The organization request type 'Microsoft.Xrm.Sdk.OrganizationRequest' is not yet supported

                the below Logic unit testing has been taken care by  => RetrieveStaffAppointmentsTest.cs

                var staffAvailabilityJson = retrieveStaffAppointment.DoWork(mockExecutionContext.Object, stringInput);
                var mockPluginExecutionContext = (IPluginExecutionContext)mockServiceProvider.Object.GetService(typeof(IPluginExecutionContext));

                var result = mockPluginExecutionContext.OutputParameters["StaffAppointmentsJson"];
             */

            #endregion ACT

            #region ASSERT

            #endregion ASSERT

        }
        [TestCategory("Web Service"), TestCategory("Negative")]
        [TestMethod]
        public void RetrieveStaffAppointmentsPortalTest_ReturnAppointmentJson_Exception()
        {

            #region ARRANGE

            var xrmFakedContext = new XrmFakedContext();

            #endregion ARRANGE

            #region ACT

            var mockLogger = new Mock<ILogger>();

            #endregion ACT

            #region ASSERT
            Assert.ThrowsException<ArgumentException>(() => new RetrieveStaffAppointmentsPortalLogic(null, null));
            Assert.ThrowsException<ArgumentException>(() => new RetrieveStaffAppointmentsPortalLogic(mockLogger.Object, null));
            #endregion ASSERT

        }
        private ActivityParty PrepareActivityParty(Guid systemuserGuid)
        {
            var activityPatyInstance = new ActivityParty()
            {
                Id = Guid.NewGuid(),
                ParticipationTypeMask = activityparty_participationtypemask.Requiredattendee,

            };
            return activityPatyInstance;
        }

        private Entity PrepareAppointment(Guid systemUserGuid, List<ActivityParty> activityParties, Guid activityGuid)
        {
            var appointment = new Models.Appointment()
            {
                Id = Guid.NewGuid(),
                OwnerId = new EntityReference("systemuser", systemUserGuid),
                ScheduledStart = DateTime.Today,
                ScheduledEnd = DateTime.Today.AddDays(5),
                Subject = "Math",
                Description = "Unit Test",
                StatusCode = appointment_statuscode.Free,
                RequiredAttendees = activityParties,
                ActivityId = activityGuid
            };
            return appointment;
        }

        private Entity PrepareSystemUser()
        {
            var systemUser = new SystemUser()
            {
                SystemUserId = Guid.NewGuid(),
            };
            return systemUser;
        }

        private Models.Contact PreapareContact()
        {
            var contact = new Models.Contact()
            {
                Id = Guid.NewGuid(),
            };
            return contact;
        }
    }
}