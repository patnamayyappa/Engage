using System;
using System.Collections.Generic;
using System.Reflection;
using Cmc.Core.Xrm.ServerExtension.Logging;
using Cmc.Engage.Common;
using Cmc.Engage.Common.Plugins.Tests.Utilities;
using Cmc.Engage.Models.Extn;
using Cmc.Engage.Models.Tests;
using FakeItEasy;
using FakeXrmEasy;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Moq;

namespace Cmc.Engage.Marketing.Tests
{
    [TestClass]
    public class UpdateEventDetailsTest : XrmUnitTestBase
    {
        [TestMethod]
        [TestCategory("Plugin"), TestCategory("Positive")]
        public void EventService_UpdateEventDetails_CreateEvent()
        {
            #region ARRANGE

            var owner = PrepareSystemUser();
            var msEvent = CreateMsEvent(owner.Id);
            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.ProxyTypesAssembly = Assembly.GetAssembly(typeof(Models.Tests.msevtmgt_Event));
            xrmFakedContext.Initialize(new List<Entity>()
            {
                owner,
                msEvent
            });
            var mockServiceProvider = InitializeMockService(xrmFakedContext, msEvent, Operation.Create);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            var time = DateTime.UtcNow;
            var result = new ParameterCollection()
            {
                {"UtcTimeFromLocalTime", time}
            };
            var calculateUtcTimeFromLocalTimeRequest = new UtcTimeFromLocalTimeResponse()
            {
                ResponseName = "UtcTimeFromLocalTime",
                Results = result
            };
            A.CallTo(() => xrmFakedContext.GetFakedOrganizationService().Execute(A<UtcTimeFromLocalTimeRequest>._)).Returns(calculateUtcTimeFromLocalTimeRequest);
            #endregion

            #region ACT

            var mockLogger = new Mock<ILogger>();
            var mockUpdateEventService = new Marketing.EventService(mockLogger.Object,
                xrmFakedContext.GetFakedOrganizationService(), new Mock<ILanguageService>().Object);
            mockUpdateEventService.UpdateEventDetails(mockExecutionContext.Object);

            #endregion

            #region ASSERT
            var data = xrmFakedContext.Data["msevtmgt_event"];
            Assert.IsNotNull(data);

            #endregion
        }

        [TestMethod]
        [TestCategory("Plugin"), TestCategory("Positive")]
        public void EventService_UpdateEventDetails_UpdateEvent_TimeZoneChanged()
        {
            #region ARRANGE

            var owner = PrepareSystemUser();
            var msEvent = CreateMsEvent(owner.Id);
            var dataEventPreImage = PreImage(msEvent);
            var dataEventPostImage = PostImage(msEvent);

            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.ProxyTypesAssembly = Assembly.GetAssembly(typeof(Models.Tests.msevtmgt_Event));

            xrmFakedContext.Initialize(new List<Entity>()
            {
                owner,
                msEvent
            });

            var mockServiceProvider = InitializeMockService(xrmFakedContext, msEvent, Operation.Update);
            AddPostEntityImage(mockServiceProvider, "PostImage", dataEventPostImage);
            AddPreEntityImage(mockServiceProvider, "PreImage", dataEventPreImage);

            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            var time = DateTime.UtcNow;
            var result = new ParameterCollection()
            {
                {"UtcTimeFromLocalTime", time}
            };
            var calculateUtcTimeFromLocalTimeRequest = new UtcTimeFromLocalTimeResponse()
            {
                ResponseName = "UtcTimeFromLocalTime",
                Results = result
            };
            A.CallTo(() => xrmFakedContext.GetFakedOrganizationService().Execute(A<UtcTimeFromLocalTimeRequest>._)).Returns(calculateUtcTimeFromLocalTimeRequest);
            #endregion

            #region ACT

            var mockLogger = new Mock<ILogger>();
            var mockUpdateEventService = new Marketing.EventService(mockLogger.Object,
                xrmFakedContext.GetFakedOrganizationService(), new Mock<ILanguageService>().Object);
            mockUpdateEventService.UpdateEventDetails(mockExecutionContext.Object);

            #endregion

            #region ASSERT
            var resultData = new Entity();
            xrmFakedContext.Data["msevtmgt_event"].TryGetValue(msEvent.Id, out resultData);
            Assert.AreNotEqual(resultData.GetAttributeValue<DateTime>("cmc_startdatetime"),dataEventPostImage.GetAttributeValue<DateTime>("cmc_startdatetime"));
            #endregion
        }

        [TestMethod]
        [TestCategory("Plugin"), TestCategory("Positive")]
        public void EventService_UpdateEventDetails_UpdateEvent_TimeZoneIsNotChanged()
        {
            #region ARRANGE

            var owner = PrepareSystemUser();
            var msEvent = CreateMsEvent(owner.Id);
            var dataEventPreImage = PreImage(msEvent);
            var dataEventPostImage = PostImage(msEvent);
            dataEventPostImage["msevtmgt_eventtimezone"] = 2;

            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.ProxyTypesAssembly = Assembly.GetAssembly(typeof(Models.Tests.msevtmgt_Event));
            xrmFakedContext.Initialize(new List<Entity>()
            {
                owner,
                msEvent
            });
            var mockServiceProvider = InitializeMockService(xrmFakedContext, msEvent, Operation.Update);
            AddPostEntityImage(mockServiceProvider, "PostImage", dataEventPostImage);
            AddPreEntityImage(mockServiceProvider, "PreImage", dataEventPreImage);

            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            var time = DateTime.UtcNow;
            var result = new ParameterCollection()
            {
                {"UtcTimeFromLocalTime", time}
            };
            var calculateUtcTimeFromLocalTimeRequest = new UtcTimeFromLocalTimeResponse()
            {
                ResponseName = "UtcTimeFromLocalTime",
                Results = result
            };
            A.CallTo(() => xrmFakedContext.GetFakedOrganizationService().Execute(A<UtcTimeFromLocalTimeRequest>._)).Returns(calculateUtcTimeFromLocalTimeRequest);
            #endregion

            #region ACT

            var mockLogger = new Mock<ILogger>();
            var mockUpdateEventService = new Marketing.EventService(mockLogger.Object,
                xrmFakedContext.GetFakedOrganizationService(), new Mock<ILanguageService>().Object);
            mockUpdateEventService.UpdateEventDetails(mockExecutionContext.Object);

            #endregion

            #region ASSERT
            
            var resultData = new Entity();
            xrmFakedContext.Data["msevtmgt_event"].TryGetValue(msEvent.Id, out resultData);
            Assert.AreNotEqual(resultData.GetAttributeValue<DateTime>("cmc_startdatetime"), dataEventPostImage.GetAttributeValue<DateTime>("cmc_startdatetime"));
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

        private Models.Extn.msevtmgt_Event CreateMsEvent(Guid userId)
        {
            return new Models.Extn.msevtmgt_Event()
            {
                Id = Guid.NewGuid(),
                OwnerId = new EntityReference("systemuser", userId),
                msevtmgt_EventStartDate = DateTime.Now,
                msevtmgt_EventEndDate = DateTime.Now.AddDays(5),
                msevtmgt_EventTimeZone = 2,
                msevtmgt_Name = "Test Event",
                cmc_startdatetime = DateTime.Now,
                cmc_enddatetime = DateTime.Now.AddDays(8),
            };
        }

        private Entity PreImage(Models.Extn.msevtmgt_Event msevtmgtEvent)
        {
            return new Models.Tests.msevtmgt_Event()
            {
                Id = msevtmgtEvent.Id,
                ["msevtmgt_eventstartdate"] =msevtmgtEvent.GetAttributeValue<DateTime>("msevtmgt_eventstartdate"),
                ["msevtmgt_eventenddate"] = msevtmgtEvent.GetAttributeValue<DateTime>("msevtmgt_eventenddate"),
                ["msevtmgt_eventtimezone"] = msevtmgtEvent.GetAttributeValue<int>("msevtmgt_eventtimezone"),
                ["msevtmgt_name"] = msevtmgtEvent.GetAttributeValue<string>("msevtmgt_name"),
                ["cmc_startdatetime"] = msevtmgtEvent.GetAttributeValue<DateTime>("cmc_startdatetime"),
                ["cmc_enddatetime"] = msevtmgtEvent.GetAttributeValue<DateTime>("cmc_enddatetime"),
            };
        }
        private Entity PostImage(Models.Extn.msevtmgt_Event msevtmgtEvent)
        {
            return new Models.Tests.msevtmgt_Event()
            {
                Id = msevtmgtEvent.Id,
                ["msevtmgt_eventstartdate"] = DateTime.Now.AddDays(2),
                ["msevtmgt_eventenddate"] = DateTime.Now.AddDays(8),
                ["msevtmgt_eventtimezone"] = 5,
                ["msevtmgt_name"] = msevtmgtEvent.GetAttributeValue<string>("msevtmgt_name"),
                ["cmc_startdatetime"] = msevtmgtEvent.GetAttributeValue<DateTime>("msevtmgt_eventenddate"),
                ["cmc_enddatetime"] = msevtmgtEvent.GetAttributeValue<DateTime>("msevtmgt_eventenddate"),
            };
        }

        #endregion

    }
}
