using System;
using System.Collections.Generic;
using System.Reflection;
using Cmc.Core.Xrm.ServerExtension.Logging;
using Cmc.Engage.Common;
using Cmc.Engage.Common.Plugins.Tests.Utilities;
using Cmc.Engage.Models;
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

namespace Cmc.Engage.Lifecycle.Tests.Trips.Plugin
{
    [TestClass]
    public class CompleteorCancelTripTest : XrmUnitTestBase
    {
        [TestMethod]
        [TestCategory("Plugin"), TestCategory("Positive")]
        public void TripService_CompleteOrCancelTrip_Tripstatus_SubmittedForReview()
        {
            #region ARRANGE

            var ownerDept = PrepareDepartment();
            var owner = PrepareSystemUser(ownerDept);
            var ownerTrip = PrepareTripStatus(owner);
            var appointment = PrepareAppoinment();
            var tripActivity = CreateTripActivity(owner.Id, ownerTrip, appointment);

            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>()
            {
                ownerDept,
                owner,
                ownerTrip,
                tripActivity
            });
            var mockServiceProvider = InitializeMockService(xrmFakedContext, ownerTrip, Operation.Create);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            AddInputParameters(mockServiceProvider, "TripId", ownerTrip.Id.ToString());
            AddInputParameters(mockServiceProvider, "ActionType", "Complete");
            #endregion

            #region ACT

            var mockLogger = new Mock<ILogger>();
            var mockTripActivityService = new TripService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
            mockTripActivityService.CompleteOrCancelTrip(mockExecutionContext.Object);

            #endregion

            #region ASSERT

            var mockPluginExecutionContext = (IPluginExecutionContext)mockServiceProvider.Object.GetService(typeof(IPluginExecutionContext));
            var result = mockPluginExecutionContext.OutputParameters["Response"];
            Assert.AreEqual("Trip_Not_Approved", result);
            #endregion

        }
        [TestMethod]
        [TestCategory("Plugin"), TestCategory("Positive")]
        public void TripService_CompleteOrCancelTrip_Tripstatus_Completed()
        {
            #region ARRANGE

            var ownerDept = PrepareDepartment();
            var owner = PrepareSystemUser(ownerDept);
            var ownerTrip = PrepareTripStatus(owner);
            ownerTrip.cmc_Status = cmc_trip_cmc_status.Canceled;

            var appointment = PrepareAppoinment();
            var tripActivity = CreateTripActivity(owner.Id, ownerTrip, appointment);

            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>()
            {
                ownerDept,
                owner,
                ownerTrip,
                tripActivity
            });
            var mockServiceProvider = InitializeMockService(xrmFakedContext, ownerTrip, Operation.Create);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            AddInputParameters(mockServiceProvider, "TripId", ownerTrip.Id.ToString());
            AddInputParameters(mockServiceProvider, "ActionType", "Complete");
            #endregion

            #region ACT

            var mockLogger = new Mock<ILogger>();
            var mockTripActivityService = new TripService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
            mockTripActivityService.CompleteOrCancelTrip(mockExecutionContext.Object);

            #endregion

            #region ASSERT

            var mockPluginExecutionContext = (IPluginExecutionContext)mockServiceProvider.Object.GetService(typeof(IPluginExecutionContext));
            var result = mockPluginExecutionContext.OutputParameters["Response"];
            Assert.AreEqual("Trip_Completed_Cancelled", result);
            #endregion

        }

        [TestMethod]
        [TestCategory("Plugin"), TestCategory("Positive")]
        public void TripService_CompleteOrCancelTrip_TripActivitystatus_Completed()
        {
            #region ARRANGE

            var ownerDept = PrepareDepartment();
            var owner = PrepareSystemUser(ownerDept);
            var ownerTrip = PrepareTripStatus(owner);
            ownerTrip.cmc_Status = cmc_trip_cmc_status.Approved;
            var tripActivity = CreateTripActivityForStatus(owner.Id, ownerTrip);

            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>()
            {
                ownerDept,
                owner,
                ownerTrip,
                tripActivity
            });
            var mockServiceProvider = InitializeMockService(xrmFakedContext, ownerTrip, Operation.Create);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            AddInputParameters(mockServiceProvider, "TripId", ownerTrip.Id.ToString());
            AddInputParameters(mockServiceProvider, "ActionType", "InComplete");
            #endregion

            #region ACT

            var mockLogger = new Mock<ILogger>();
            var mockTripActivityService = new TripService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
            mockTripActivityService.CompleteOrCancelTrip(mockExecutionContext.Object);

            #endregion

            #region ASSERT

            var mockPluginExecutionContext = (IPluginExecutionContext)mockServiceProvider.Object.GetService(typeof(IPluginExecutionContext));
            var result = mockPluginExecutionContext.OutputParameters["Response"];
            Assert.AreEqual("TripActivity_Not_Cancelled", result);
            #endregion

        }

        [TestMethod]
        [TestCategory("Plugin"), TestCategory("Positive")]
        public void TripService_CompleteOrCancelTrip_TripActivitystatus_Planned()
        {
            #region ARRANGE

            var ownerDept = PrepareDepartment();
            var owner = PrepareSystemUser(ownerDept);
            var ownerTrip = PrepareTripStatus(owner);
            ownerTrip.cmc_Status = cmc_trip_cmc_status.Approved;
            var tripActivity = CreateTripActivityForStatus(owner.Id, ownerTrip);
            tripActivity.cmc_TripActivityStatus = cmc_tripactivity_cmc_tripactivitystatus.Planned;
            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>()
            {
                ownerDept,
                owner,
                ownerTrip,
                tripActivity
            });
            var mockServiceProvider = InitializeMockService(xrmFakedContext, ownerTrip, Operation.Create);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            AddInputParameters(mockServiceProvider, "TripId", ownerTrip.Id.ToString());
            AddInputParameters(mockServiceProvider, "ActionType", "Complete");
            #endregion

            #region ACT

            var mockLogger = new Mock<ILogger>();
            var mockTripActivityService = new TripService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
            mockTripActivityService.CompleteOrCancelTrip(mockExecutionContext.Object);

            #endregion

            #region ASSERT

            var mockPluginExecutionContext = (IPluginExecutionContext)mockServiceProvider.Object.GetService(typeof(IPluginExecutionContext));
            var result = mockPluginExecutionContext.OutputParameters["Response"];
            Assert.AreEqual("TripActivity_Not_Completed", result);
            #endregion

        }

        [TestMethod]
        [TestCategory("Plugin"), TestCategory("Positive")]
        public void TripService_CompleteOrCancelTrip_TripActivitystatus_Confirmed()
        {
            #region ARRANGE

            var ownerDept = PrepareDepartment();
            var owner = PrepareSystemUser(ownerDept);
            var ownerTrip = PrepareTripStatus(owner);
            ownerTrip.cmc_Status = cmc_trip_cmc_status.Approved;
            var tripActivity = CreateTripActivityForStatus(owner.Id, ownerTrip);
            tripActivity.cmc_TripActivityStatus = cmc_tripactivity_cmc_tripactivitystatus.Confirmed;
            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>()
            {
                ownerDept,
                owner,
                ownerTrip,
                tripActivity
            });
            var mockServiceProvider = InitializeMockService(xrmFakedContext, ownerTrip, Operation.Create);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            AddInputParameters(mockServiceProvider, "TripId", ownerTrip.Id.ToString());
            AddInputParameters(mockServiceProvider, "ActionType", "Complete");
            #endregion

            #region ACT

            var mockLogger = new Mock<ILogger>();
            var mockTripActivityService = new TripService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
            mockTripActivityService.CompleteOrCancelTrip(mockExecutionContext.Object);

            #endregion

            #region ASSERT

            var mockPluginExecutionContext = (IPluginExecutionContext)mockServiceProvider.Object.GetService(typeof(IPluginExecutionContext));
            var result = mockPluginExecutionContext.OutputParameters["Response"];
            Assert.AreEqual("TripActivity_Not_Completed", result);
            #endregion

        }

        [TestMethod]
        [TestCategory("Plugin"), TestCategory("Positive")]
        public void TripService_CompleteOrCancelTrip_TripActivitystatus_Requested()
        {
            #region ARRANGE

            var ownerDept = PrepareDepartment();
            var owner = PrepareSystemUser(ownerDept);
            var ownerTrip = PrepareTripStatus(owner);
            ownerTrip.cmc_Status = cmc_trip_cmc_status.Approved;
            var tripActivity = CreateTripActivityForStatus(owner.Id, ownerTrip);
            tripActivity.cmc_TripActivityStatus = cmc_tripactivity_cmc_tripactivitystatus.Requested;
            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>()
            {
                ownerDept,
                owner,
                ownerTrip,
                tripActivity
            });
            var mockServiceProvider = InitializeMockService(xrmFakedContext, ownerTrip, Operation.Create);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            AddInputParameters(mockServiceProvider, "TripId", ownerTrip.Id.ToString());
            AddInputParameters(mockServiceProvider, "ActionType", "Complete");
            #endregion

            #region ACT

            var mockLogger = new Mock<ILogger>();
            var mockTripActivityService = new TripService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
            mockTripActivityService.CompleteOrCancelTrip(mockExecutionContext.Object);

            #endregion

            #region ASSERT

            var mockPluginExecutionContext = (IPluginExecutionContext)mockServiceProvider.Object.GetService(typeof(IPluginExecutionContext));
            var result = mockPluginExecutionContext.OutputParameters["Response"];
            Assert.AreEqual("TripActivity_Not_Completed", result);
            #endregion

        }

        [TestMethod]
        [TestCategory("Plugin"), TestCategory("Positive")]
        public void TripService_CompleteOrCancelTrip_TripActivitystatus_Canceled()
        {
            #region ARRANGE

            var ownerDept = PrepareDepartment();
            var owner = PrepareSystemUser(ownerDept);
            var ownerTrip = PrepareTripStatus(owner);
            ownerTrip.cmc_Status = cmc_trip_cmc_status.Approved;
            var tripActivity = CreateTripActivityForStatus(owner.Id, ownerTrip);
            tripActivity.cmc_TripActivityStatus = cmc_tripactivity_cmc_tripactivitystatus.Canceled;
            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>()
            {
                ownerDept,
                owner,
                ownerTrip,
                tripActivity
            });
            var mockServiceProvider = InitializeMockService(xrmFakedContext, ownerTrip, Operation.Create);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            AddInputParameters(mockServiceProvider, "TripId", ownerTrip.Id.ToString());
            AddInputParameters(mockServiceProvider, "ActionType", "Complete");
            #endregion

            #region ACT

            var mockLogger = new Mock<ILogger>();
            var mockTripActivityService = new TripService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
            mockTripActivityService.CompleteOrCancelTrip(mockExecutionContext.Object);

            #endregion

            #region ASSERT

            var mockPluginExecutionContext = (IPluginExecutionContext)mockServiceProvider.Object.GetService(typeof(IPluginExecutionContext));
            var result = mockPluginExecutionContext.OutputParameters["Response"];
            Assert.AreEqual("", result);
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
        private Models.cmc_trip PrepareTripStatus(Models.SystemUser systemUser)
        {
            return new Models.cmc_trip()
            {
                Id = Guid.NewGuid(),
                cmc_tripname = "Unit Test Tirp",
                OwnerId = systemUser.ToEntityReference(),
                cmc_StartDate = DateTime.Now.AddHours(0),
                cmc_EndDate = DateTime.Now.AddHours(-2),
                cmc_Department = systemUser.cmc_departmentid,
                cmc_Status = Models.cmc_trip_cmc_status.SubmittedForReview
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
        private Models.cmc_tripactivity CreateTripActivityForStatus(Guid userId, Models.cmc_trip cmcTrip)
        {
            return new Models.cmc_tripactivity()
            {
                Id = Guid.NewGuid(),
                OwnerId = new EntityReference("systemuser", userId),
                cmc_name = "TripName Updated - Trip ACtivty",
                cmc_trip = cmcTrip.ToEntityReference(),
                cmc_TripActivityStatus = cmc_tripactivity_cmc_tripactivitystatus.Completed
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
        #endregion
    }
}
