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
    public class AttachAppointmentICalFileActivityTest : XrmUnitTestBase
    {
        [TestCategory("Activity"), TestCategory("Positive")]
        [TestMethod]
        public void AttachAppointmentICalFileActivity_ActivityMimeAttachment()
        {
            var creatingEmail = PreparingEmaailInstance();
            var creatingAppointment = PreparingAppointment();
            var creatingActivityParty = PreparingActivityParty(creatingAppointment);
            var creatingActivityParty1 = PreparingActivityParty1(creatingAppointment);
            var creatingActivityParty2 = PreparingActivityParty2(creatingAppointment);
            var creatingActivity = new EntityReference("ActivityId", creatingActivityParty.Id);

            List<ActivityParty> activityParties = new List<ActivityParty>();
            activityParties.Add(creatingActivityParty);
            activityParties.Add(creatingActivityParty1);
            activityParties.Add(creatingActivityParty2);

            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>()
            {
              creatingEmail,
              creatingAppointment,
              creatingActivityParty,
              creatingActivityParty1,
              creatingActivityParty2
            });

            var mockLogger = new Mock<ILogger>();
            var mockILanguageService = new Mock<ILanguageService>();
            var mockAppointmentService = new AppointmentService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService(), mockILanguageService.Object);
            var result= mockAppointmentService.AttachAppointmentICalFileService(creatingEmail.ToEntityReference(), creatingAppointment.ToEntityReference());

            #region ASSERT
            var value = result.Attributes["filename"];
           Assert.AreEqual("Appointment.ics", value);

            #endregion
        }

        private Email PreparingEmaailInstance()
        {
            var email = new Email()
            {
                Id = Guid.NewGuid()
            };

            return email;
        }
        private Models.Appointment PreparingAppointment()
        {
            var email = new Models.Appointment()
            {
                Id = Guid.NewGuid(),
                ScheduledStart = DateTime.Today,
                ScheduledEnd = DateTime.Today.AddDays(5),
                Location = "Test Location",
                Description = "Test Description",
                IsAllDayEvent = true,
                Subject = "Test Subject",
            };

            return email;
        }
       
        private ActivityParty PreparingActivityParty(Entity AppointmentInstance)
        {
            var activityParty = new ActivityParty()
            {
                ActivityId= AppointmentInstance.ToEntityReference(),
                PartyId=new EntityReference("partyid", Guid.NewGuid()),
                ActivityPartyId=Guid.NewGuid(),
                AddressUsed ="TestAddreas",
                ParticipationTypeMask= activityparty_participationtypemask.Organizer
               
            };
            return activityParty;
        }
        private ActivityParty PreparingActivityParty1(Entity AppointmentInstance)
        {
            var activityParty = new ActivityParty()
            {
                ActivityId = AppointmentInstance.ToEntityReference(),
                PartyId = new EntityReference("partyid", Guid.NewGuid()),
                ActivityPartyId = Guid.NewGuid(),
                AddressUsed = "TestAddreas",
                ParticipationTypeMask = activityparty_participationtypemask.Requiredattendee

            };
            return activityParty;
        }
        private ActivityParty PreparingActivityParty2(Entity AppointmentInstance)
        {
            var activityParty = new ActivityParty()
            {
                ActivityId = AppointmentInstance.ToEntityReference(),
                PartyId = new EntityReference("partyid", Guid.NewGuid()),
                ActivityPartyId = Guid.NewGuid(),
                AddressUsed = "TestAddreas",
                ParticipationTypeMask = activityparty_participationtypemask.Optionalattendee

            };
            return activityParty;
        }

    }


}
