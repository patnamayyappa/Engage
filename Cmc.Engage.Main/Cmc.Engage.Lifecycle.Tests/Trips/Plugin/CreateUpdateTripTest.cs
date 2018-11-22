using System;
using System.Collections.Generic;
using System.Reflection;
using Cmc.Core.Xrm.ServerExtension.Logging;
using Cmc.Engage.Common;
using Cmc.Engage.Common.Plugins.Tests.Utilities;
using Cmc.Engage.Models;
using Cmc.Engage.Models.Extn;
using Cmc.Engage.Models.Tests;
using FakeItEasy;
using FakeXrmEasy;
using FakeXrmEasy.Extensions;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Moq;
using cmc_tripactivity_cmc_tripactivitystatus = Cmc.Engage.Models.cmc_tripactivity_cmc_tripactivitystatus;
using cmc_trip_cmc_status = Cmc.Engage.Models.cmc_trip_cmc_status;
using msevtmgt_Event = Cmc.Engage.Models.Tests.msevtmgt_Event;

namespace Cmc.Engage.Lifecycle.Tests.Trips.Plugin
{
    [TestClass]
    public class CreateUpdateTripTest : XrmUnitTestBase
    {
        [TestMethod]
        [TestCategory("Plugin"), TestCategory("Positive")]
        public void TripService_CreateUpdateTripService_CreateofCmcTrip()
        {
            #region ARRANGE

            var ownerDept = PrepareDepartment();
            var owner = PrepareSystemUser(ownerDept);
            var ownerTrip = PrepareTrip(owner);

            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>()
            {
                ownerDept,
                owner,
                ownerTrip,
            });
            var mockServiceProvider = InitializeMockService(xrmFakedContext, ownerTrip, Operation.Create);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            #endregion

            #region ACT

            var mockLogger = new Mock<ILogger>();
            var mockTripActivityService = new TripService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
            mockTripActivityService.CreateUpdateTripService(mockExecutionContext.Object);

            #endregion

            #region ASSERT

            var resultData = new Entity("cmc_trip");
            xrmFakedContext.Data["cmc_trip"].TryGetValue(ownerTrip.Id, out resultData);

            Assert.AreEqual(resultData.Id, ownerTrip.Id);
            #endregion

        }
        [TestMethod]
        [TestCategory("Plugin"), TestCategory("Positive")]
        public void TripService_CreateUpdateTripService_UpdateofCmcTripName_ActivityTypeAppointment()
        {
            #region ARRANGE

            var ownerDept = PrepareDepartment();
            var owner = PrepareSystemUser(ownerDept);
            var ownerTrip = PrepareTrip(owner);
            var appointment = PrepareAppoinment();
            var tripActivity = CreateTripActivity(owner.Id, ownerTrip, appointment);

            var ownerTripUpdate = PrepareTrip(owner);
            ownerTripUpdate["cmc_tripname"] = "TripName Updated";

            var preImageEntity = PreparePreImage(ownerTripUpdate);

            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>()
            {
                ownerDept,
                owner,
                ownerTrip,
                ownerTripUpdate,
                tripActivity,
                appointment
            });

            var mockServiceProvider = InitializeMockService(xrmFakedContext, ownerTrip, Operation.Update);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            AddPreEntityImage(mockServiceProvider, "PreImage", preImageEntity);
            #endregion

            #region ACT

            var mockLogger = new Mock<ILogger>();
            var mockTripActivityService = new TripService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
            mockTripActivityService.CreateUpdateTripService(mockExecutionContext.Object);

            #endregion

            #region ASSERT

            var resultData = new Entity("cmc_tripactivity");
            xrmFakedContext.Data["cmc_tripactivity"].TryGetValue(tripActivity.Id, out resultData);

            Assert.AreNotEqual(resultData.GetAttributeValue<string>("cmc_name"), tripActivity.GetAttributeValue<string>("cmc_name"));
            #endregion
        }
        [TestMethod]
        [TestCategory("Plugin"), TestCategory("Positive")]
        public void TripService_CreateUpdateTripService_UpdateofCmcTripName_ActivityTypeEvent()
        {
            #region ARRANGE

            var ownerDept = PrepareDepartment();
            var owner = PrepareSystemUser(ownerDept);
            var ownerTrip = PrepareTrip(owner);
            var msEvent = CreateMsEvent(owner.Id);
            var tripActivity = CreateTripActivity1(owner.Id, ownerTrip, msEvent);

            var ownerTripUpdate = PrepareTrip(owner);
            ownerTripUpdate["cmc_tripname"] = "TripName Updated";

            var preImageEntity = PreparePreImage(ownerTripUpdate);

            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.ProxyTypesAssembly = Assembly.GetAssembly(typeof(msevtmgt_Event));

            xrmFakedContext.Initialize(new List<Entity>()
            {
                ownerDept,
                owner,
                ownerTrip,
                ownerTripUpdate,
                tripActivity,
                msEvent
            });

            var mockServiceProvider = InitializeMockService(xrmFakedContext, ownerTrip, Operation.Update);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            AddPreEntityImage(mockServiceProvider, "PreImage", preImageEntity);
            #endregion

            #region ACT
            var attributeMetadata = new LookupAttributeMetadata()
            {
                //LogicalName = "msevtmgt_event",
                Targets = new string[] { msEvent.LogicalName },
                AttributeTypeName = { Value = "Lookup" }
            };

            var entityMetadata = new EntityMetadata() { LogicalName = "cmc_tripactivity" };

            entityMetadata.SetAttributeCollection(new List<LookupAttributeMetadata>() { attributeMetadata });
            var entityResponse = new RetrieveEntityResponse()
            {
                Results = new ParameterCollection
                {
                    { "EntityMetadata", entityMetadata }
                }
            };
            A.CallTo(() => xrmFakedContext.GetFakedOrganizationService().Execute(A<RetrieveEntityRequest>._)).Returns(entityResponse);
            var mockLogger = new Mock<ILogger>();
            var mockTripActivityService = new TripService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
            mockTripActivityService.CreateUpdateTripService(mockExecutionContext.Object);

            #endregion

            #region ASSERT

            var resultData = new Entity("cmc_tripactivity");
            xrmFakedContext.Data["cmc_tripactivity"].TryGetValue(tripActivity.Id, out resultData);

            Assert.AreNotEqual(resultData.GetAttributeValue<string>("cmc_name"), tripActivity.GetAttributeValue<string>("cmc_name"));
            #endregion
        }

        #region DATA PREPARATION
        private Models.cmc_department PrepareDepartment()
        {
            return new Models.cmc_department()
            {
                Id = Guid.NewGuid(),
                cmc_departmentname = "dev"
            };
        }
        private Models.SystemUser PrepareSystemUser(Models.cmc_department cmc_Department)
        {
            return new Models.SystemUser()
            {
                Id = Guid.NewGuid(),
                cmc_departmentid = cmc_Department.ToEntityReference(),
            };
        }
        private Models.cmc_trip PrepareTrip(Models.SystemUser systemUser)
        {
            return new Models.cmc_trip()
            {
                Id = Guid.NewGuid(),
                cmc_tripname = "Unit Test Tirp",
                OwnerId = systemUser.ToEntityReference(),
                cmc_StartDate = DateTime.Now.AddHours(0),
                cmc_EndDate = DateTime.Now.AddHours(-2),
                cmc_Department = systemUser.cmc_departmentid, 
            };
        }
        private msevtmgt_Event CreateMsEvent(Guid userId)
        {
            return new msevtmgt_Event()
            {
                Id = Guid.NewGuid(),
                OwnerId = new EntityReference("systemuser", userId),
                msevtmgt_EventStartDate = DateTime.Now,
                msevtmgt_EventEndDate = DateTime.Now.AddDays(5),
                msevtmgt_EventTimeZone = 2,
                msevtmgt_Name = "Test Event",
            };
        }
        private Models.cmc_tripactivity CreateTripActivity(Guid userId, Models.cmc_trip cmcTrip, Entity activityEntity)
        {
            return new Models.cmc_tripactivity()
            {
                Id = Guid.NewGuid(),
                OwnerId = new EntityReference("systemuser", userId),
                cmc_name = "TripName Updated - Trip ACtivty",
                cmc_trip = cmcTrip.ToEntityReference(),
                cmc_activitytype = Models.cmc_tripactivity_cmc_activitytype.Appointment,
                cmc_LinkedToAppointment = activityEntity.ToEntityReference()
            };
        }
        private Models.Tests.cmc_tripactivity CreateTripActivity1(Guid userId, Models.cmc_trip cmcTrip, Entity activityEntity)
        {
            return new Models.Tests.cmc_tripactivity()
            {
                Id = Guid.NewGuid(),
                OwnerId = new EntityReference("systemuser", userId),
                cmc_name = "TripName Updated - Trip Event",
                cmc_trip = cmcTrip.ToEntityReference(),
                cmc_activitytype = Models.Tests.cmc_tripactivity_cmc_activitytype.Event,
                cmc_linkedtoevent = activityEntity.ToEntityReference()
            };
        }
        private Models.Appointment PrepareAppoinment()
        {
            return new Models.Appointment()
            {
                Id = Guid.NewGuid(),
                Subject = "AppointmentName"
            };
        }
        private Entity PreparePreImage(Entity cmcTrip)
        {
            var preImage = new Entity("cmc_trip", cmcTrip.Id)
            {
                ["ownerid"] = cmcTrip.GetAttributeValue<EntityReference>("ownerid"),
                ["cmc_enddate"] = cmcTrip.GetAttributeValue<DateTime>("cmc_enddate"),
                ["cmc_Department"] = cmcTrip.GetAttributeValue<EntityReference>("cmc_Department"),
                ["cmc_StartDate"] = cmcTrip.GetAttributeValue<DateTime>("cmc_StartDate"),
                ["cmc_tripname"] = cmcTrip.GetAttributeValue<string>("cmc_tripname")
            };
            return preImage;
        }
        #endregion

    }
}
