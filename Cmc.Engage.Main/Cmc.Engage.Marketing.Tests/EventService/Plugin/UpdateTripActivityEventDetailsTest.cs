using System;
using System.Collections.Generic;
using System.Reflection;
using Cmc.Core.Xrm.ServerExtension.Logging;
using Cmc.Engage.Common;
using Cmc.Engage.Common.Plugins.Tests.Utilities;
using Cmc.Engage.Models.Tests;
using FakeItEasy;
using FakeXrmEasy;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Moq;

namespace Cmc.Engage.Marketing.Tests.EventService.Plugin
{
    [TestClass]
    public class UpdateTripActivityEventDetailsTest : XrmUnitTestBase
    {
        [TestMethod]
        [TestCategory("Plugin"), TestCategory("Positive")]
        public void EventService_UpdateTripActivityEventDetails_UpdateEvent()
        {
            #region ARRANGE

            var owner = PrepareSystemUser();
            var msVenue = PrepareVenue();
            var msEvent = CreateMsEvent(owner.Id, msVenue);
            var trip = CreateCmcTrip(owner.Id);
            var tripActivity = CreateTripActivityEvent(owner.Id, trip, msEvent);
            var dataEventPreImage = PreImage(msEvent);
            var dataEventPostImage = PostImage(msEvent);

            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.ProxyTypesAssembly = Assembly.GetAssembly(typeof(msevtmgt_Event));
            xrmFakedContext.Initialize(new List<Entity>()
            {
                owner,
                msEvent,
                msVenue,
                trip,
                tripActivity
            });
            var mockServiceProvider = InitializeMockService(xrmFakedContext, msEvent, Operation.Update);
            AddPostEntityImage(mockServiceProvider, "PostImage", dataEventPostImage);
            AddPreEntityImage(mockServiceProvider, "PreImage", dataEventPreImage);

            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            #endregion

            #region ACT

            var mockLogger = new Mock<ILogger>();
            var mockUpdateEventService = new Marketing.EventService(mockLogger.Object,
                xrmFakedContext.GetFakedOrganizationService(), new Mock<ILanguageService>().Object);
            mockUpdateEventService.UpdateTripActivityEventDetails(mockExecutionContext.Object);

            #endregion

            #region ASSERT
            var resultData = new Entity("cmc_tripactivity");
            xrmFakedContext.Data["cmc_tripactivity"].TryGetValue(tripActivity.Id, out resultData);

            var data = resultData.Attributes["cmc_name"].ToString();
            Assert.IsTrue(data.Contains(dataEventPostImage.GetAttributeValue<string>("msevtmgt_name")));
            #endregion
        }
        #region Data Preparation

        private Models.SystemUser PrepareSystemUser()
        {
            return new Models.SystemUser()
            {
                Id = Guid.NewGuid()
            };
        }

        private Models.Extn.msevtmgt_Event CreateMsEvent(Guid userId, msevtmgt_Venue venue)
        {
            return new Models.Extn.msevtmgt_Event()
            {
                Id = Guid.NewGuid(),
                OwnerId = new EntityReference("systemuser", userId),
                msevtmgt_EventStartDate = DateTime.Now.AddDays(1),
                msevtmgt_EventEndDate = DateTime.Now.AddDays(10),
                msevtmgt_EventTimeZone = 2,
                msevtmgt_Name = "Test Event",
                msevtmgt_PrimaryVenue = venue.ToEntityReference(),
                cmc_startdatetime = DateTime.Now,
                cmc_enddatetime = DateTime.Now.AddDays(8),
            };
        }

        private msevtmgt_Venue PrepareVenue()
        {
            return new msevtmgt_Venue()
            {
                Id = Guid.NewGuid(),
                msevtmgt_Country = "United States",
                msevtmgt_City = "Merrillville",
                msevtmgt_AddressLine1 = "355 E. 83rd Ave",
                msevtmgt_AddressLine2 = "Extended Perks Logo",
                msevtmgt_AddressLine3 = "Merrillville ",
                msevtmgt_StateProvince = "Merrillville ",
                msevtmgt_PostalCode = "46410"
            };
        }
        private Entity PreImage(Models.Extn.msevtmgt_Event msevtmgtEvent)
        {
            return new msevtmgt_Event()
            {
                Id = msevtmgtEvent.Id,
                ["msevtmgt_eventstartdate"] = msevtmgtEvent.GetAttributeValue<DateTime>("msevtmgt_eventstartdate"),
                ["msevtmgt_eventenddate"] = msevtmgtEvent.GetAttributeValue<DateTime>("msevtmgt_eventenddate"),
                ["msevtmgt_eventtimezone"] = msevtmgtEvent.GetAttributeValue<int>("msevtmgt_eventtimezone"),
                ["msevtmgt_name"] = msevtmgtEvent.GetAttributeValue<string>("msevtmgt_name"),
                ["cmc_startdatetime"] = msevtmgtEvent.GetAttributeValue<DateTime>("cmc_startdatetime"),
                ["cmc_enddatetime"] = msevtmgtEvent.GetAttributeValue<DateTime>("cmc_enddatetime"),
            };
        }
        private Entity PostImage(Models.Extn.msevtmgt_Event msevtmgtEvent)
        {
            return new msevtmgt_Event()
            {
                Id = msevtmgtEvent.Id,
                ["msevtmgt_eventstartdate"] = DateTime.Now,
                ["msevtmgt_eventenddate"] = DateTime.Now.AddDays(20),
                ["msevtmgt_eventtimezone"] = 5,
                ["msevtmgt_name"] = "Updated Event Name",
                ["cmc_startdatetime"] = DateTime.Now.AddDays(1),
                ["cmc_enddatetime"] = DateTime.Now.AddDays(10),
            };
        }
        private cmc_tripactivity CreateTripActivityEvent(Guid userId, cmc_trip cmcTrip, Entity marketingList)
        {
            return new cmc_tripactivity()
            {
                Id = Guid.NewGuid(),
                OwnerId = new EntityReference("systemuser", userId),
                cmc_name = "Trip Event",
                cmc_trip = new EntityReference("cmc_trip", cmcTrip.Id),
                cmc_activitytype = Models.Tests.cmc_tripactivity_cmc_activitytype.Event,
                cmc_linkedtoevent = marketingList.ToEntityReference(),
                cmc_StartDateTime = marketingList.GetAttributeValue<DateTime>("msevtmgt_EventStartDate"),
                cmc_EndDateTime = marketingList.GetAttributeValue<DateTime>("msevtmgt_EventEndDate"),
                cmc_location = "No.177, Baiyi Upper Street, Chengdu",
                ["cmc_estimatedexpense"] = new Money(123),
                ["cmc_actualexpense"] = new Money(123),
                cmc_locationstartdatetime = DateTime.Now,
                cmc_locationtimezone = 2,
                cmc_locationenddatetime = DateTime.Now.AddDays(5)

            };
        }
        private cmc_trip CreateCmcTrip(Guid userId)
        {
            return new cmc_trip()
            {
                Id = Guid.NewGuid(),
                OwnerId = new EntityReference("systemuser", userId),
                cmc_tripname = "Unit Test Trip",
                cmc_StartDate = DateTime.Now,
                cmc_EndDate = DateTime.Now.AddDays(10),
            };
        }

        #endregion

    }
}
