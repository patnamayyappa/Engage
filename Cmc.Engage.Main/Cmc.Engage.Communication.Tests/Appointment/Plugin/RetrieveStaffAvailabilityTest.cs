﻿using System;
using System.Collections.Generic;
using System.Reflection;
using Cmc.Core.Xrm.ServerExtension.Logging;
using Cmc.Engage.Common;
using Cmc.Engage.Common.Plugins.Tests.Utilities;
using Cmc.Engage.Models;
using FakeXrmEasy;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Moq;

namespace Cmc.Engage.Communication.Tests.Appointment.Plugin
{
    [TestClass]
    public class RetrieveStaffAvailabilityTest : XrmUnitTestBase
    {
        [TestCategory("Web Service"), TestCategory("Positive")]
        [TestMethod]
        public void RetrieveStaffAvailability_Test()
        {
            #region ARRANGE
            var accountInstance = PrepareContactInstance();
            var userInstance = PrepareUserInstance();
            var userLocationInstance = PrepareCmcUserLocation(accountInstance.Id);
            var officeHoursInstance = PrepareOfficeHours(userLocationInstance.Id,userInstance.Id,accountInstance.Id);
            var activityPartyInstance = PrepareActivityParty(accountInstance.Id);

            List<ActivityParty> listActivityPartyInstance = new List<ActivityParty>();
            listActivityPartyInstance.Add(activityPartyInstance);

            var appointmentInstance = PrepareAppointmentInstance(accountInstance.Id, listActivityPartyInstance);

            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>()
            {
                userLocationInstance,
                officeHoursInstance,
                accountInstance,
                appointmentInstance,
                activityPartyInstance,
                userInstance
            });
            xrmFakedContext.ProxyTypesAssembly = Assembly.GetAssembly(typeof(cmc_officehours));
            var mockServiceProvider = InitializeMockService(xrmFakedContext, accountInstance,Operation.cmc_RetrieveStaffAvailabilityFromOfficeHours);
            AddInputParameters(mockServiceProvider, "AccountId", accountInstance.Id.ToString());
            AddInputParameters(mockServiceProvider, "UserId", userInstance.Id.ToString());
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);

            var entityCollections = new EntityCollection();
            entityCollections.Entities.Add(officeHoursInstance);

            var mockOrganizationServices = new Mock<IOrganizationService>(); ;
            mockOrganizationServices.Setup(r => r.RetrieveMultiple(It.IsAny<FetchExpression>())).Returns(entityCollections);

            #endregion

            #region ACT

            var mockLogger = new Mock<ILogger>();
            var mockILanguageService = new Mock<ILanguageService>();

            var mockAppointmentService =
                new AppointmentService(mockLogger.Object, mockOrganizationServices.Object, mockILanguageService.Object);

            mockAppointmentService.RetrieveStaffAvailability(mockExecutionContext.Object);

            #endregion

            #region ASSERT

            var mockPluginExecutionContext =
                (IPluginExecutionContext) mockServiceProvider.Object.GetService(typeof(IPluginExecutionContext));
            var result = mockPluginExecutionContext.OutputParameters["StaffAvailabilityJson"];
            Assert.IsNotNull(result);

            #endregion
        }

        private Account PrepareContactInstance()
        {
            var accountInstance = new Account()
            {
                AccountId = Guid.NewGuid(),
                
            };
            return accountInstance;
        }

        private Models.Appointment PrepareAppointmentInstance(Guid accountId, List<ActivityParty> activityParties)
        {
            var appointmentInstance = new Models.Appointment()
            {
                Id = Guid.NewGuid(),
                OwnerId = new EntityReference("Account", accountId),
                ScheduledStart = DateTime.Today,
                ScheduledEnd = DateTime.Today.AddDays(5),
                Subject = "Test Subject",
                Description = "Test Description",
                RequiredAttendees = activityParties
            };
            return appointmentInstance;
        }

        private SystemUser PrepareUserInstance()
        {
            var userInstance = new SystemUser()
            {
                SystemUserId = Guid.NewGuid()
            };
            return userInstance;
        }

        private ActivityParty PrepareActivityParty(Guid accountId)
        {
            var activityPatyInstance = new ActivityParty()
            {
                Id = Guid.NewGuid(),
                PartyId = new EntityReference("Account", accountId),
                ParticipationTypeMask = activityparty_participationtypemask.Requiredattendee
            };
            return activityPatyInstance;
        }

        private cmc_officehours PrepareOfficeHours(Guid userLocationId,Guid systemUserId,Guid accountId)
        {
            var officeHoursInstance = new cmc_officehours()
            {
                Id = Guid.NewGuid(),
                cmc_monday = true,
                cmc_tuesday = true,
                cmc_wednesday = true,
                cmc_thursday = true,
                cmc_friday = true,
                cmc_saturday = false,
                cmc_sunday = false,
                cmc_startdate = DateTime.Today,
                cmc_enddate = DateTime.UtcNow.AddDays(5),
                cmc_starttime = DateTime.Today,
                cmc_endtime = DateTime.Today.AddHours(6),
                cmc_duration = 6,
                OwnerId = new EntityReference("SystemUser",systemUserId),
                cmc_userlocationid = new EntityReference("cmc_userlocation", userLocationId),
                ["cmc_accountid"]=new EntityReference("account",accountId)
            };
            return officeHoursInstance;
        }

        private Entity PrepareCmcUserLocation(Guid accountId)
        {
            var cmcUserLocation = new Entity("cmc_userlocation", Guid.NewGuid())
            {
                ["cmc_accountid"]=new EntityReference("account", accountId),
                ["statecode"]=0,
            };
            return cmcUserLocation;
        }
    }
}
