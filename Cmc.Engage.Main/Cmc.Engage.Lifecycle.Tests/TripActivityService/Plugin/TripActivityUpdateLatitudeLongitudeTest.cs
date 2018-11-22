using System;
using System.Collections.Generic;
using System.Reflection;
using Cmc.Core.Xrm.ServerExtension.Logging;
using Cmc.Engage.Common;
using Cmc.Engage.Common.Plugins.Tests.Utilities;
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
using Appointment = Cmc.Engage.Models.Appointment;
using cmc_configuration = Cmc.Engage.Models.cmc_configuration;
using cmc_trip = Cmc.Engage.Models.cmc_trip;
using cmc_tripactivity = Cmc.Engage.Models.cmc_tripactivity;
using cmc_tripactivity_cmc_activitytype = Cmc.Engage.Models.cmc_tripactivity_cmc_activitytype;
using Contact = Cmc.Engage.Models.Contact;
using SystemUser = Cmc.Engage.Models.SystemUser;

namespace Cmc.Engage.Lifecycle.Tests.TripActivityService.Plugin
{
    [TestClass]
    public class TripActivityUpdateLatitudeLongitudeTest : XrmUnitTestBase
    {
        /// <summary>
        /// Create Latitude Longitude for Given Location 
        /// </summary>
        [TestMethod]
        [TestCategory("Plugin"), TestCategory("Positive")]
        public void TripActivityService_TripActivityUpdateLatitudeLongitude_Create_ActivityTypeAppointment_InsertLatitudeLongitudeForCorrectAddress()
        {
            #region ARRANGE
            var bingMapKeyConfigInstance = PrepareBingMapKeyConfigInstance();
            var owner = PrepareSystemUser();

            var appointment = CreateAppointment();
            var contact = CreateContact();
            var cmcTrip = CreateCmcTrip(owner.Id);
            var cmcTripActivity = CreateTripActivityAppointment(owner.Id, cmcTrip, appointment);
            var associateStaff = TripSystemUser();

            cmcTrip["cmc_estexpense"] = (Money)cmcTripActivity.Attributes["cmc_estimatedexpense"]; ;
            cmcTrip["cmc_actexpense"] = (Money)cmcTripActivity.Attributes["cmc_actualexpense"];

            var associateContactForTrip = new Entity("cmc_trip_contact", Guid.NewGuid())
            {

                ["contactid"] = contact.Id,
                ["cmc_tripid"] = cmcTrip.Id
            };
            var associateStaffMemberForTripActivity = new Entity("cmc_tripactivity_systemuser", Guid.NewGuid())
            {

                ["cmc_tripactivityid"] = cmcTripActivity.Id,
                ["systemuserid"] = owner.Id
            };
            var associateStaffMemberForTrip = new Entity("cmc_trip_systemuser", Guid.NewGuid())
            {

                ["cmc_tripid"] = cmcTrip.Id,
                ["systemuserid"] = owner.Id
            };
            var xrmFakedContext = new XrmFakedContext();

            xrmFakedContext.Initialize(new List<Entity>()
            {
                associateContactForTrip,
                associateStaffMemberForTripActivity,
                associateStaffMemberForTrip,

                associateStaff,
                bingMapKeyConfigInstance,
                owner,
                cmcTrip,
                cmcTripActivity,
                appointment
            });

            xrmFakedContext.AddRelationship("cmc_trip_contact", new XrmFakedRelationship
            {
                IntersectEntity = "cmc_trip_contact",
                RelationshipType = XrmFakedRelationship.enmFakeRelationshipType.ManyToMany,
                Entity1LogicalName = cmcTrip.LogicalName,
                Entity1Attribute = "cmc_tripid",
                Entity2LogicalName = contact.LogicalName,
                Entity2Attribute = "contactid"
            });

            xrmFakedContext.AddRelationship("cmc_trip_systemuser", new XrmFakedRelationship
            {
                IntersectEntity = "cmc_trip_systemuser",
                RelationshipType = XrmFakedRelationship.enmFakeRelationshipType.ManyToMany,
                Entity1LogicalName = cmcTrip.LogicalName,
                Entity1Attribute = "cmc_tripid",
                Entity2LogicalName = owner.LogicalName,
                Entity2Attribute = "systemuserid"
            });

            xrmFakedContext.AddRelationship("cmc_tripactivity_systemuser", new XrmFakedRelationship
            {
                IntersectEntity = "cmc_tripactivity_systemuser",
                RelationshipType = XrmFakedRelationship.enmFakeRelationshipType.ManyToMany,
                Entity1LogicalName = cmcTripActivity.LogicalName,
                Entity1Attribute = "cmc_tripactivityid",
                Entity2LogicalName = owner.LogicalName,
                Entity2Attribute = "systemuserid"
            });

            var result = new ParameterCollection()
            {
                {"Entity", cmcTrip}
            };
            var calculateRollupFieldResponse = new CalculateRollupFieldResponse()
            {
                ResponseName = "CalculateRollupField",
                Results = result
            };

            A.CallTo(() => xrmFakedContext.GetFakedOrganizationService().Execute(A<CalculateRollupFieldRequest>._)).Returns(calculateRollupFieldResponse);
            xrmFakedContext.ProxyTypesAssembly = Assembly.GetAssembly(typeof(cmc_trip));
            var mockServiceProvider = InitializeMockService(xrmFakedContext, cmcTripActivity, Operation.Create);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            #endregion ARRANGE

            #region ACT
            var mockLogger = new Mock<ILogger>();

            //Mock the IBingMap
            var mockBingServices = InitializeBingMapMockService();
            var mockTripActivityService = new Lifecycle.TripActivityService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService(), mockBingServices.Object, new Mock<ILanguageService>().Object);
            mockTripActivityService.TripActivityUpdateLatitudeLongitude(mockExecutionContext.Object);
            #endregion ACT

            #region ASSERT
            var latitude = cmcTripActivity.Attributes["cmc_tripactivitylatitude"];
            var longitude = cmcTripActivity.Attributes["cmc_tripactivitylongitude"];
            Assert.IsNotNull(latitude);
            Assert.IsNotNull(longitude);
            #endregion ASSERT
        }

        /// <summary>
        /// Create Latitude Longitude for Given Location with assoicated staff member
        /// </summary>
        [TestMethod]
        [TestCategory("Plugin"), TestCategory("Positive")]
        public void TripActivityService_TripActivityUpdateLatitudeLongitude_Create_ActivityTypeAppointment_InsertLatitudeLongitudeForCorrectAddress_AssociateStaffMember()
        {
            #region ARRANGE
            var bingMapKeyConfigInstance = PrepareBingMapKeyConfigInstance();
            var owner = PrepareSystemUser();

            var appointment = CreateAppointment();
            var contact = CreateContact();
            var cmcTrip = CreateCmcTrip(owner.Id);
            var cmcTripActivity = CreateTripActivityAppointment(owner.Id, cmcTrip, appointment);
            var associateStaff = TripSystemUser();

            cmcTrip["cmc_estexpense"] = (Money)cmcTripActivity.Attributes["cmc_estimatedexpense"]; ;
            cmcTrip["cmc_actexpense"] = (Money)cmcTripActivity.Attributes["cmc_actualexpense"];

            var associateContactForTrip = new Entity("cmc_trip_contact", Guid.NewGuid())
            {

                ["contactid"] = contact.Id,
                ["cmc_tripid"] = cmcTrip.Id
            };
            var associateStaffMemberForTripActivity = new Entity("cmc_tripactivity_systemuser", Guid.NewGuid())
            {

                ["cmc_tripactivityid"] = cmcTripActivity.Id,
                ["systemuserid"] = owner.Id
            };
            var associateStaffMemberForTrip = new Entity("cmc_trip_systemuser", Guid.NewGuid())
            {

                ["cmc_tripid"] = cmcTrip.Id,
                ["systemuserid"] = owner.Id
            };
            var xrmFakedContext = new XrmFakedContext();

            xrmFakedContext.Initialize(new List<Entity>()
            {
                associateContactForTrip,
                associateStaffMemberForTripActivity,
                associateStaffMemberForTrip,
                associateStaff,
                bingMapKeyConfigInstance,
                owner,
                cmcTrip,
                cmcTripActivity,
                appointment
            });

            xrmFakedContext.AddRelationship("cmc_trip_contact", new XrmFakedRelationship
            {
                IntersectEntity = "cmc_trip_contact",
                RelationshipType = XrmFakedRelationship.enmFakeRelationshipType.ManyToMany,
                Entity1LogicalName = cmcTrip.LogicalName,
                Entity1Attribute = "cmc_tripid",
                Entity2LogicalName = contact.LogicalName,
                Entity2Attribute = "contactid"
            });

            xrmFakedContext.AddRelationship("cmc_trip_systemuser", new XrmFakedRelationship
            {
                IntersectEntity = "cmc_trip_systemuser",
                RelationshipType = XrmFakedRelationship.enmFakeRelationshipType.ManyToMany,
                Entity1LogicalName = cmcTrip.LogicalName,
                Entity1Attribute = "cmc_tripid",
                Entity2LogicalName = owner.LogicalName,
                Entity2Attribute = "systemuserid"
            });

            xrmFakedContext.AddRelationship("cmc_tripactivity_systemuser", new XrmFakedRelationship
            {
                IntersectEntity = "cmc_tripactivity_systemuser",
                RelationshipType = XrmFakedRelationship.enmFakeRelationshipType.ManyToMany,
                Entity1LogicalName = cmcTripActivity.LogicalName,
                Entity1Attribute = "cmc_tripactivityid",
                Entity2LogicalName = owner.LogicalName,
                Entity2Attribute = "systemuserid"
            });

            var result = new ParameterCollection()
            {
                {"Entity", cmcTrip}
            };
            var calculateRollupFieldResponse = new CalculateRollupFieldResponse()
            {
                ResponseName = "CalculateRollupField",
                Results = result
            };

            A.CallTo(() => xrmFakedContext.GetFakedOrganizationService().Execute(A<CalculateRollupFieldRequest>._)).Returns(calculateRollupFieldResponse);
            xrmFakedContext.ProxyTypesAssembly = Assembly.GetAssembly(typeof(cmc_trip));
            var mockServiceProvider = InitializeMockService(xrmFakedContext, cmcTripActivity, Operation.Create);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            #endregion ARRANGE

            #region ACT
            var mockLogger = new Mock<ILogger>();

            //Mock the IBingMap
            var mockBingServices = InitializeBingMapMockService();
            var mockTripActivityService = new Lifecycle.TripActivityService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService(), mockBingServices.Object, new Mock<ILanguageService>().Object);
            mockTripActivityService.TripActivityUpdateLatitudeLongitude(mockExecutionContext.Object);
            #endregion ACT

            #region ASSERT
            var latitude = cmcTripActivity.Attributes["cmc_tripactivitylatitude"];
            var longitude = cmcTripActivity.Attributes["cmc_tripactivitylongitude"];
            Assert.IsNotNull(latitude);
            Assert.IsNotNull(longitude);
            #endregion ASSERT
        }

        [TestMethod]
        [TestCategory("Plugin"), TestCategory("Positive")]
        public void TripActivityService_TripActivityUpdateLatitudeLongitude_Create_ActivityTypeEvent_InsertLatitudeLongitudeForCorrectAddress_AssociateStaffMember()
        {
            #region ARRANGE
            var bingMapKeyConfigInstance = PrepareBingMapKeyConfigInstance();
            var owner = PrepareSystemUser();

            var listEvent = CreateMsEvent(owner.Id);
            var contact = CreateContact();
            var cmcTrip = CreateCmcTrip(owner.Id);
            var cmcTripActivity = CreateTripActivityEvent(owner.Id, cmcTrip, listEvent);

            var associateStaff = TripSystemUser();

            cmcTrip["cmc_estexpense"] = (Money)cmcTripActivity.Attributes["cmc_estimatedexpense"]; ;
            cmcTrip["cmc_actexpense"] = (Money)cmcTripActivity.Attributes["cmc_actualexpense"];

            var associateContactForTrip = new Entity("cmc_trip_contact", Guid.NewGuid())
            {

                ["contactid"] = contact.Id,
                ["cmc_tripid"] = cmcTrip.Id
            };
            var associateStaffMemberForTripActivity = new Entity("cmc_tripactivity_systemuser", Guid.NewGuid())
            {

                ["cmc_tripactivityid"] = cmcTripActivity.Id,
                ["systemuserid"] = owner.Id
            };
            var associateStaffMemberForTrip = new Entity("cmc_trip_systemuser", Guid.NewGuid())
            {

                ["cmc_tripid"] = cmcTrip.Id,
                ["systemuserid"] = owner.Id
            };
            var xrmFakedContext = new XrmFakedContext();

            xrmFakedContext.Initialize(new List<Entity>()
            {
                associateContactForTrip,
                associateStaffMemberForTripActivity,
                associateStaffMemberForTrip,
                associateStaff,
                bingMapKeyConfigInstance,
                owner,
                cmcTrip,
                cmcTripActivity,
                listEvent
            });

            xrmFakedContext.AddRelationship("cmc_trip_contact", new XrmFakedRelationship
            {
                IntersectEntity = "cmc_trip_contact",
                RelationshipType = XrmFakedRelationship.enmFakeRelationshipType.ManyToMany,
                Entity1LogicalName = cmcTrip.LogicalName,
                Entity1Attribute = "cmc_tripid",
                Entity2LogicalName = contact.LogicalName,
                Entity2Attribute = "contactid"
            });

            xrmFakedContext.AddRelationship("cmc_trip_systemuser", new XrmFakedRelationship
            {
                IntersectEntity = "cmc_trip_systemuser",
                RelationshipType = XrmFakedRelationship.enmFakeRelationshipType.ManyToMany,
                Entity1LogicalName = cmcTrip.LogicalName,
                Entity1Attribute = "cmc_tripid",
                Entity2LogicalName = owner.LogicalName,
                Entity2Attribute = "systemuserid"
            });

            xrmFakedContext.AddRelationship("cmc_tripactivity_systemuser", new XrmFakedRelationship
            {
                IntersectEntity = "cmc_tripactivity_systemuser",
                RelationshipType = XrmFakedRelationship.enmFakeRelationshipType.ManyToMany,
                Entity1LogicalName = cmcTripActivity.LogicalName,
                Entity1Attribute = "cmc_tripactivityid",
                Entity2LogicalName = owner.LogicalName,
                Entity2Attribute = "systemuserid"
            });

            var result = new ParameterCollection()
            {
                {"Entity", cmcTrip}
            };
            var calculateRollupFieldResponse = new CalculateRollupFieldResponse()
            {
                ResponseName = "CalculateRollupField",
                Results = result
            };

            A.CallTo(() => xrmFakedContext.GetFakedOrganizationService().Execute(A<CalculateRollupFieldRequest>._)).Returns(calculateRollupFieldResponse);

            var attributeMetadata = new LookupAttributeMetadata()
            {
                //LogicalName = "msevtmgt_event",
                Targets = new string[] { listEvent.LogicalName },
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

            //xrmFakedContext.ProxyTypesAssembly = Assembly.GetAssembly(typeof(cmc_trip));
            var mockServiceProvider = InitializeMockService(xrmFakedContext, cmcTripActivity, Operation.Create);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            xrmFakedContext.ProxyTypesAssembly = Assembly.GetAssembly(typeof(Cmc.Engage.Models.Tests.cmc_trip));

            #endregion ARRANGE

            #region ACT
            var mockLogger = new Mock<ILogger>();

            //Mock the IBingMap
            var mockBingServices = InitializeBingMapMockService();
            var mockTripActivityService = new Lifecycle.TripActivityService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService(), mockBingServices.Object, new Mock<ILanguageService>().Object);
            mockTripActivityService.TripActivityUpdateLatitudeLongitude(mockExecutionContext.Object);
            #endregion ACT

            #region ASSERT
            var latitude = cmcTripActivity.Attributes["cmc_tripactivitylatitude"];
            var longitude = cmcTripActivity.Attributes["cmc_tripactivitylongitude"];
            Assert.IsNotNull(latitude);
            Assert.IsNotNull(longitude);
            #endregion ASSERT
        }

        [TestMethod]
        [TestCategory("Plugin"), TestCategory("Negative")]
        public void TripActivityService_TripActivityUpdateLatitudeLongitude_Create_ActivityTypeEvent_InsertLatitudeLongitudeForCorrectAddress_LinkedToEventIsNotExist()
        {
            #region ARRANGE
            var bingMapKeyConfigInstance = PrepareBingMapKeyConfigInstance();
            var owner = PrepareSystemUser();

            var listEvent = CreateMsEvent(owner.Id);
            var contact = CreateContact();
            var cmcTrip = CreateCmcTrip(owner.Id);
            var cmcTripActivity = CreateTripActivityEvent(owner.Id, cmcTrip, listEvent);
            cmcTripActivity.cmc_linkedtoevent = null;

            var associateStaff = TripSystemUser();

            cmcTrip["cmc_estexpense"] = (Money)cmcTripActivity.Attributes["cmc_estimatedexpense"]; ;
            cmcTrip["cmc_actexpense"] = (Money)cmcTripActivity.Attributes["cmc_actualexpense"];

            var associateContactForTrip = new Entity("cmc_trip_contact", Guid.NewGuid())
            {

                ["contactid"] = contact.Id,
                ["cmc_tripid"] = cmcTrip.Id
            };
            var associateStaffMemberForTripActivity = new Entity("cmc_tripactivity_systemuser", Guid.NewGuid())
            {

                ["cmc_tripactivityid"] = cmcTripActivity.Id,
                ["systemuserid"] = owner.Id
            };
            var associateStaffMemberForTrip = new Entity("cmc_trip_systemuser", Guid.NewGuid())
            {

                ["cmc_tripid"] = cmcTrip.Id,
                ["systemuserid"] = owner.Id
            };
            var xrmFakedContext = new XrmFakedContext();

            xrmFakedContext.Initialize(new List<Entity>()
            {
                associateContactForTrip,
                associateStaffMemberForTripActivity,
                associateStaffMemberForTrip,
                associateStaff,
                bingMapKeyConfigInstance,
                owner,
                cmcTrip,
                cmcTripActivity,
                listEvent
            });

            xrmFakedContext.AddRelationship("cmc_trip_contact", new XrmFakedRelationship
            {
                IntersectEntity = "cmc_trip_contact",
                RelationshipType = XrmFakedRelationship.enmFakeRelationshipType.ManyToMany,
                Entity1LogicalName = cmcTrip.LogicalName,
                Entity1Attribute = "cmc_tripid",
                Entity2LogicalName = contact.LogicalName,
                Entity2Attribute = "contactid"
            });

            xrmFakedContext.AddRelationship("cmc_trip_systemuser", new XrmFakedRelationship
            {
                IntersectEntity = "cmc_trip_systemuser",
                RelationshipType = XrmFakedRelationship.enmFakeRelationshipType.ManyToMany,
                Entity1LogicalName = cmcTrip.LogicalName,
                Entity1Attribute = "cmc_tripid",
                Entity2LogicalName = owner.LogicalName,
                Entity2Attribute = "systemuserid"
            });

            xrmFakedContext.AddRelationship("cmc_tripactivity_systemuser", new XrmFakedRelationship
            {
                IntersectEntity = "cmc_tripactivity_systemuser",
                RelationshipType = XrmFakedRelationship.enmFakeRelationshipType.ManyToMany,
                Entity1LogicalName = cmcTripActivity.LogicalName,
                Entity1Attribute = "cmc_tripactivityid",
                Entity2LogicalName = owner.LogicalName,
                Entity2Attribute = "systemuserid"
            });

            var result = new ParameterCollection()
            {
                {"Entity", cmcTrip}
            };
            var calculateRollupFieldResponse = new CalculateRollupFieldResponse()
            {
                ResponseName = "CalculateRollupField",
                Results = result
            };

            A.CallTo(() => xrmFakedContext.GetFakedOrganizationService().Execute(A<CalculateRollupFieldRequest>._)).Returns(calculateRollupFieldResponse);

            var attributeMetadata = new LookupAttributeMetadata()
            {
                //LogicalName = "msevtmgt_event",
                Targets = new string[] { listEvent.LogicalName },
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

            //xrmFakedContext.ProxyTypesAssembly = Assembly.GetAssembly(typeof(cmc_trip));
            var mockServiceProvider = InitializeMockService(xrmFakedContext, cmcTripActivity, Operation.Create);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            xrmFakedContext.ProxyTypesAssembly = Assembly.GetAssembly(typeof(Cmc.Engage.Models.Tests.cmc_trip));

            #endregion ARRANGE

            #region ACT
            var mockLogger = new Mock<ILogger>();

            //Mock the IBingMap
            var mockBingServices = InitializeBingMapMockService();
            var mockTripActivityService = new Lifecycle.TripActivityService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService(), mockBingServices.Object, new Mock<ILanguageService>().Object);
            mockTripActivityService.TripActivityUpdateLatitudeLongitude(mockExecutionContext.Object);
            #endregion ACT

            #region ASSERT
            var latitude = cmcTripActivity.Attributes["cmc_tripactivitylatitude"];
            var longitude = cmcTripActivity.Attributes["cmc_tripactivitylongitude"];
            Assert.IsNotNull(latitude);
            Assert.IsNotNull(longitude);
            #endregion ASSERT
        }

        [TestMethod]
        [TestCategory("Plugin"), TestCategory("Positive")]
        public void TripActivityService_TripActivityUpdateLatitudeLongitude_Create_ActivityTypeEvent_InsertLatitudeLongitudeForCorrectAddress_LinkedToEventDetailsIsExist()
        {
            #region ARRANGE
            var bingMapKeyConfigInstance = PrepareBingMapKeyConfigInstance();
            var owner = PrepareSystemUser();

            var listEvent = CreateMsEvent(owner.Id);
            var contact = CreateContact();
            var cmcTrip = CreateCmcTrip(owner.Id);
            var cmcTripActivity = CreateTripActivityEvent(owner.Id, cmcTrip, listEvent);

            var associateStaff = TripSystemUser();

            cmcTrip["cmc_estexpense"] = (Money)cmcTripActivity.Attributes["cmc_estimatedexpense"]; ;
            cmcTrip["cmc_actexpense"] = (Money)cmcTripActivity.Attributes["cmc_actualexpense"];

            var associateContactForTrip = new Entity("cmc_trip_contact", Guid.NewGuid())
            {

                ["contactid"] = contact.Id,
                ["cmc_tripid"] = cmcTrip.Id
            };
            var associateStaffMemberForTripActivity = new Entity("cmc_tripactivity_systemuser", Guid.NewGuid())
            {

                ["cmc_tripactivityid"] = cmcTripActivity.Id,
                ["systemuserid"] = owner.Id
            };
            var associateStaffMemberForTrip = new Entity("cmc_trip_systemuser", Guid.NewGuid())
            {

                ["cmc_tripid"] = cmcTrip.Id,
                ["systemuserid"] = owner.Id
            };
            var xrmFakedContext = new XrmFakedContext();

            xrmFakedContext.Initialize(new List<Entity>()
            {
                associateContactForTrip,
                associateStaffMemberForTripActivity,
                associateStaffMemberForTrip,
                associateStaff,
                bingMapKeyConfigInstance,
                owner,
                cmcTrip,
                cmcTripActivity,
                listEvent
            });

            xrmFakedContext.AddRelationship("cmc_trip_contact", new XrmFakedRelationship
            {
                IntersectEntity = "cmc_trip_contact",
                RelationshipType = XrmFakedRelationship.enmFakeRelationshipType.ManyToMany,
                Entity1LogicalName = cmcTrip.LogicalName,
                Entity1Attribute = "cmc_tripid",
                Entity2LogicalName = contact.LogicalName,
                Entity2Attribute = "contactid"
            });

            xrmFakedContext.AddRelationship("cmc_trip_systemuser", new XrmFakedRelationship
            {
                IntersectEntity = "cmc_trip_systemuser",
                RelationshipType = XrmFakedRelationship.enmFakeRelationshipType.ManyToMany,
                Entity1LogicalName = cmcTrip.LogicalName,
                Entity1Attribute = "cmc_tripid",
                Entity2LogicalName = owner.LogicalName,
                Entity2Attribute = "systemuserid"
            });

            xrmFakedContext.AddRelationship("cmc_tripactivity_systemuser", new XrmFakedRelationship
            {
                IntersectEntity = "cmc_tripactivity_systemuser",
                RelationshipType = XrmFakedRelationship.enmFakeRelationshipType.ManyToMany,
                Entity1LogicalName = cmcTripActivity.LogicalName,
                Entity1Attribute = "cmc_tripactivityid",
                Entity2LogicalName = owner.LogicalName,
                Entity2Attribute = "systemuserid"
            });

            var result = new ParameterCollection()
            {
                {"Entity", cmcTrip}
            };
            var calculateRollupFieldResponse = new CalculateRollupFieldResponse()
            {
                ResponseName = "CalculateRollupField",
                Results = result
            };

            A.CallTo(() => xrmFakedContext.GetFakedOrganizationService().Execute(A<CalculateRollupFieldRequest>._)).Returns(calculateRollupFieldResponse);

            var attributeMetadata = new LookupAttributeMetadata()
            {
                //LogicalName = "msevtmgt_event",
                Targets = new string[] { listEvent.LogicalName },
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

            //xrmFakedContext.ProxyTypesAssembly = Assembly.GetAssembly(typeof(cmc_trip));
            var mockServiceProvider = InitializeMockService(xrmFakedContext, cmcTripActivity, Operation.Create);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            xrmFakedContext.ProxyTypesAssembly = Assembly.GetAssembly(typeof(Cmc.Engage.Models.Tests.cmc_trip));

            #endregion ARRANGE

            #region ACT
            var mockLogger = new Mock<ILogger>();

            //Mock the IBingMap
            var mockBingServices = InitializeBingMapMockService();
            var mockTripActivityService = new Lifecycle.TripActivityService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService(), mockBingServices.Object, new Mock<ILanguageService>().Object);
            mockTripActivityService.TripActivityUpdateLatitudeLongitude(mockExecutionContext.Object);
            #endregion ACT

            #region ASSERT
            var latitude = cmcTripActivity.Attributes["cmc_tripactivitylatitude"];
            var longitude = cmcTripActivity.Attributes["cmc_tripactivitylongitude"];
            Assert.IsNotNull(latitude);
            Assert.IsNotNull(longitude);
            #endregion ASSERT
        }

        [TestMethod]
        [TestCategory("Plugin"), TestCategory("Negative")]
        public void TripActivityService_TripActivityUpdateLatitudeLongitude_Create_ActivityTypeEvent_WhenEventIsNull()
        {
            #region ARRANGE
            var bingMapKeyConfigInstance = PrepareBingMapKeyConfigInstance();
            var owner = PrepareSystemUser();

            var listEvent = CreateMsEvent(owner.Id);
            var contact = CreateContact();
            var cmcTrip = CreateCmcTrip(owner.Id);
            var cmcTripActivity = CreateTripActivityEvent(owner.Id, cmcTrip, listEvent);

            var associateStaff = TripSystemUser();

            cmcTrip["cmc_estexpense"] = (Money)cmcTripActivity.Attributes["cmc_estimatedexpense"]; ;
            cmcTrip["cmc_actexpense"] = (Money)cmcTripActivity.Attributes["cmc_actualexpense"];

            var associateContactForTrip = new Entity("cmc_trip_contact", Guid.NewGuid())
            {

                ["contactid"] = contact.Id,
                ["cmc_tripid"] = cmcTrip.Id
            };
            var associateStaffMemberForTripActivity = new Entity("cmc_tripactivity_systemuser", Guid.NewGuid())
            {

                ["cmc_tripactivityid"] = cmcTripActivity.Id,
                ["systemuserid"] = owner.Id
            };
            var associateStaffMemberForTrip = new Entity("cmc_trip_systemuser", Guid.NewGuid())
            {

                ["cmc_tripid"] = cmcTrip.Id,
                ["systemuserid"] = owner.Id
            };
            var xrmFakedContext = new XrmFakedContext();

            xrmFakedContext.Initialize(new List<Entity>()
            {
                associateContactForTrip,
                associateStaffMemberForTripActivity,
                associateStaffMemberForTrip,
                associateStaff,
                bingMapKeyConfigInstance,
                owner,
                cmcTrip,
                cmcTripActivity,
                listEvent
            });

            xrmFakedContext.AddRelationship("cmc_trip_contact", new XrmFakedRelationship
            {
                IntersectEntity = "cmc_trip_contact",
                RelationshipType = XrmFakedRelationship.enmFakeRelationshipType.ManyToMany,
                Entity1LogicalName = cmcTrip.LogicalName,
                Entity1Attribute = "cmc_tripid",
                Entity2LogicalName = contact.LogicalName,
                Entity2Attribute = "contactid"
            });

            xrmFakedContext.AddRelationship("cmc_trip_systemuser", new XrmFakedRelationship
            {
                IntersectEntity = "cmc_trip_systemuser",
                RelationshipType = XrmFakedRelationship.enmFakeRelationshipType.ManyToMany,
                Entity1LogicalName = cmcTrip.LogicalName,
                Entity1Attribute = "cmc_tripid",
                Entity2LogicalName = owner.LogicalName,
                Entity2Attribute = "systemuserid"
            });

            xrmFakedContext.AddRelationship("cmc_tripactivity_systemuser", new XrmFakedRelationship
            {
                IntersectEntity = "cmc_tripactivity_systemuser",
                RelationshipType = XrmFakedRelationship.enmFakeRelationshipType.ManyToMany,
                Entity1LogicalName = cmcTripActivity.LogicalName,
                Entity1Attribute = "cmc_tripactivityid",
                Entity2LogicalName = owner.LogicalName,
                Entity2Attribute = "systemuserid"
            });

            var result = new ParameterCollection()
            {
                {"Entity", cmcTrip}
            };
            var calculateRollupFieldResponse = new CalculateRollupFieldResponse()
            {
                ResponseName = "CalculateRollupField",
                Results = result
            };

            A.CallTo(() => xrmFakedContext.GetFakedOrganizationService().Execute(A<CalculateRollupFieldRequest>._)).Returns(calculateRollupFieldResponse);

            var attributeMetadata = new LookupAttributeMetadata()
            {
                //LogicalName = "msevtmgt_event",
                Targets = new string[] { cmcTrip.LogicalName },
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

            xrmFakedContext.ProxyTypesAssembly = Assembly.GetAssembly(typeof(cmc_trip));
            var mockServiceProvider = InitializeMockService(xrmFakedContext, cmcTripActivity, Operation.Create);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            #endregion ARRANGE

            #region ACT
            var mockLogger = new Mock<ILogger>();

            //Mock the IBingMap
            var mockBingServices = InitializeBingMapMockService();
            var mockTripActivityService = new Lifecycle.TripActivityService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService(), mockBingServices.Object, new Mock<ILanguageService>().Object);
            mockTripActivityService.TripActivityUpdateLatitudeLongitude(mockExecutionContext.Object);
            #endregion ACT

            #region ASSERT
            var latitude = cmcTripActivity.Attributes["cmc_tripactivitylatitude"];
            var longitude = cmcTripActivity.Attributes["cmc_tripactivitylongitude"];
            Assert.IsNotNull(latitude);
            Assert.IsNotNull(longitude);
            #endregion ASSERT
        }

        [TestMethod]
        [TestCategory("Plugin"), TestCategory("Positive")]
        public void TripActivityService_TripActivityUpdateLatitudeLongitude_Create_ActivityTypeOther_InsertLatitudeLongitudeForCorrectAddress_AssociateStaffMember()
        {
            #region ARRANGE
            var bingMapKeyConfigInstance = PrepareBingMapKeyConfigInstance();
            var owner = PrepareSystemUser();

            var contact = CreateContact();
            var cmcTrip = CreateCmcTrip(owner.Id);
            var cmcTripActivity = CreateTripActivityOthers(contact, owner.Id, cmcTrip);

            cmcTrip["cmc_estexpense"] = (Money)cmcTripActivity.Attributes["cmc_estimatedexpense"]; ;
            cmcTrip["cmc_actexpense"] = (Money)cmcTripActivity.Attributes["cmc_actualexpense"];

            var associateContactForTrip = new Entity("cmc_trip_contact", Guid.NewGuid())
            {

                ["contactid"] = contact.Id,
                ["cmc_tripid"] = cmcTrip.Id
            };
            var associateStaffMemberForTripActivity = new Entity("cmc_tripactivity_systemuser", Guid.NewGuid())
            {

                ["cmc_tripactivityid"] = cmcTripActivity.Id,
                ["systemuserid"] = owner.Id
            };
            var associateStaffMemberForTrip = new Entity("cmc_trip_systemuser", Guid.NewGuid())
            {

                ["cmc_tripid"] = cmcTrip.Id,
                ["systemuserid"] = owner.Id
            };
            var xrmFakedContext = new XrmFakedContext();

            xrmFakedContext.Initialize(new List<Entity>()
            {
                associateContactForTrip,
                associateStaffMemberForTripActivity,
                associateStaffMemberForTrip,
                bingMapKeyConfigInstance,
                owner,
                cmcTrip,
                cmcTripActivity,
                contact
            });

            xrmFakedContext.AddRelationship("cmc_trip_contact", new XrmFakedRelationship
            {
                IntersectEntity = "cmc_trip_contact",
                RelationshipType = XrmFakedRelationship.enmFakeRelationshipType.ManyToMany,
                Entity1LogicalName = cmcTrip.LogicalName,
                Entity1Attribute = "cmc_tripid",
                Entity2LogicalName = contact.LogicalName,
                Entity2Attribute = "contactid"
            });

            xrmFakedContext.AddRelationship("cmc_trip_systemuser", new XrmFakedRelationship
            {
                IntersectEntity = "cmc_trip_systemuser",
                RelationshipType = XrmFakedRelationship.enmFakeRelationshipType.ManyToMany,
                Entity1LogicalName = cmcTrip.LogicalName,
                Entity1Attribute = "cmc_tripid",
                Entity2LogicalName = owner.LogicalName,
                Entity2Attribute = "systemuserid"
            });

            xrmFakedContext.AddRelationship("cmc_tripactivity_systemuser", new XrmFakedRelationship
            {
                IntersectEntity = "cmc_tripactivity_systemuser",
                RelationshipType = XrmFakedRelationship.enmFakeRelationshipType.ManyToMany,
                Entity1LogicalName = cmcTripActivity.LogicalName,
                Entity1Attribute = "cmc_tripactivityid",
                Entity2LogicalName = owner.LogicalName,
                Entity2Attribute = "systemuserid"
            });

            var result = new ParameterCollection()
            {
                {"Entity", cmcTrip}
            };
            var calculateRollupFieldResponse = new CalculateRollupFieldResponse()
            {
                ResponseName = "CalculateRollupField",
                Results = result
            };

            A.CallTo(() => xrmFakedContext.GetFakedOrganizationService().Execute(A<CalculateRollupFieldRequest>._)).Returns(calculateRollupFieldResponse);

            xrmFakedContext.ProxyTypesAssembly = Assembly.GetAssembly(typeof(cmc_trip));
            var mockServiceProvider = InitializeMockService(xrmFakedContext, cmcTripActivity, Operation.Create);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            #endregion ARRANGE

            #region ACT
            var mockLogger = new Mock<ILogger>();

            //Mock the IBingMap
            var mockBingServices = InitializeBingMapMockService();
            var mockTripActivityService = new Lifecycle.TripActivityService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService(), mockBingServices.Object, new Mock<ILanguageService>().Object);
            mockTripActivityService.TripActivityUpdateLatitudeLongitude(mockExecutionContext.Object);
            #endregion ACT

            #region ASSERT
            var latitude = cmcTripActivity.Attributes["cmc_tripactivitylatitude"];
            var longitude = cmcTripActivity.Attributes["cmc_tripactivitylongitude"];
            Assert.IsNotNull(latitude);
            Assert.IsNotNull(longitude);
            #endregion ASSERT
        }

        /// <summary>
        /// Update Latitude Longitude for Given Location 
        /// </summary>
        [TestMethod]
        [TestCategory("Plugin"), TestCategory("Positive")]
        public void TripActivityService_TripActivityUpdateLatitudeLongitude_Update()
        {
            #region ARRANGE

            var bingMapKeyConfigInstance = PrepareBingMapKeyConfigInstance();
            var owner = PrepareSystemUser();

            var appointment = CreateAppointment();
            var cmcTrip = CreateCmcTrip(owner.Id);
            var cmcTripActivity = CreateTripActivityAppointment(owner.Id, cmcTrip, appointment);

            var preImageEntity = PreparePreImage(cmcTripActivity);
            var xrmFakedContext = new XrmFakedContext();

            //xrmFakedContext.ProxyTypesAssembly = Assembly.Load("Cmc.Engage.Contracts.Tests");
            xrmFakedContext.Initialize(new List<Entity>()
            {
                bingMapKeyConfigInstance,
                owner,
                cmcTrip,
                appointment,
                cmcTripActivity,
            });

            var mockServiceProvider = InitializeMockService(xrmFakedContext, cmcTripActivity, Operation.Update);
            AddPreEntityImage(mockServiceProvider, "PreImage", preImageEntity);

            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);

            var calculateRollupFieldRequest = new CalculateRollupFieldRequest()
            {
            };

            var calculateRollupFieldResponse = new CalculateRollupFieldResponse()
            {
            };
            A.CallTo(() => xrmFakedContext.GetFakedOrganizationService().Execute(A<CalculateRollupFieldRequest>._)).Returns(calculateRollupFieldResponse);
            #endregion ARRANGE

            #region ACT
            var mockLogger = new Mock<ILogger>();

            //Mock the IBingMap
            var mockConfigurationService = new ConfigurationService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
            var mockBingServices = InitializeBingMapMockService();
            //var mockTripActivityService = new TripActivityService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService(), mockBingServices);
            var mockTripActivityService = new Lifecycle.TripActivityService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService(), mockBingServices.Object, new Mock<ILanguageService>().Object);
            mockTripActivityService.TripActivityUpdateLatitudeLongitude(mockExecutionContext.Object);
            #endregion ACT

            #region ASSERT

            var updatedLatitude = cmcTripActivity.Attributes["cmc_tripactivitylatitude"];
            var updatedLongitude = cmcTripActivity.Attributes["cmc_tripactivitylongitude"];
            var latitude = preImageEntity.Attributes["cmc_tripactivitylatitude"];
            var longitude = preImageEntity.Attributes["cmc_tripactivitylongitude"];
            Assert.AreNotEqual(latitude, updatedLatitude);
            Assert.AreNotEqual(longitude, updatedLongitude);
            #endregion ASSERT
        }

        [TestMethod]
        [TestCategory("Plugin"), TestCategory("Positive")]
        public void TripActivityService_TripActivityUpdateLatitudeLongitude_Create_TripUpdateRollupFields()
        {
            #region ARRANGE
            var bingMapKeyConfigInstance = PrepareBingMapKeyConfigInstance();
            var owner = PrepareSystemUser();

            var appointment = CreateAppointment();
            var contact = CreateContact();
            var cmcTrip = CreateCmcTrip(owner.Id);
            var cmcTripActivity = CreateTripActivityAppointment(owner.Id, cmcTrip, appointment);

            cmcTrip["cmc_estexpense"] = (Money)cmcTripActivity.Attributes["cmc_estimatedexpense"]; ;
            cmcTrip["cmc_actexpense"] = (Money)cmcTripActivity.Attributes["cmc_actualexpense"];

            var associateStaff = TripSystemUser();
            var associateContactForTrip = new Entity("cmc_trip_contact", Guid.NewGuid())
            {

                ["contactid"] = contact.Id,
                ["cmc_tripid"] = cmcTrip.Id
            };
            var associateStaffMemberForTripActivity = new Entity("cmc_tripactivity_systemuser", Guid.NewGuid())
            {

                ["cmc_tripactivityid"] = cmcTripActivity.Id,
                ["systemuserid"] = owner.Id
            };
            var associateStaffMemberForTrip = new Entity("cmc_trip_systemuser", Guid.NewGuid())
            {

                ["cmc_tripid"] = cmcTrip.Id,
                ["systemuserid"] = owner.Id
            };
            var xrmFakedContext = new XrmFakedContext();

            xrmFakedContext.Initialize(new List<Entity>()
            {
                associateContactForTrip,
                associateStaffMemberForTripActivity,
                associateStaffMemberForTrip,

                associateStaff,
                bingMapKeyConfigInstance,
                owner,
                cmcTrip,
                cmcTripActivity,
                appointment
            });

            xrmFakedContext.AddRelationship("cmc_trip_contact", new XrmFakedRelationship
            {
                IntersectEntity = "cmc_trip_contact",
                RelationshipType = XrmFakedRelationship.enmFakeRelationshipType.ManyToMany,
                Entity1LogicalName = cmcTrip.LogicalName,
                Entity1Attribute = "cmc_tripid",
                Entity2LogicalName = contact.LogicalName,
                Entity2Attribute = "contactid"
            });

            xrmFakedContext.AddRelationship("cmc_trip_systemuser", new XrmFakedRelationship
            {
                IntersectEntity = "cmc_trip_systemuser",
                RelationshipType = XrmFakedRelationship.enmFakeRelationshipType.ManyToMany,
                Entity1LogicalName = cmcTrip.LogicalName,
                Entity1Attribute = "cmc_tripid",
                Entity2LogicalName = owner.LogicalName,
                Entity2Attribute = "systemuserid"
            });

            xrmFakedContext.AddRelationship("cmc_tripactivity_systemuser", new XrmFakedRelationship
            {
                IntersectEntity = "cmc_tripactivity_systemuser",
                RelationshipType = XrmFakedRelationship.enmFakeRelationshipType.ManyToMany,
                Entity1LogicalName = cmcTripActivity.LogicalName,
                Entity1Attribute = "cmc_tripactivityid",
                Entity2LogicalName = owner.LogicalName,
                Entity2Attribute = "systemuserid"
            });

            var result = new ParameterCollection()
            {
                {"Entity", cmcTrip}
            };
            var calculateRollupFieldResponse = new CalculateRollupFieldResponse()
            {
                ResponseName = "CalculateRollupField",
                Results = result
            };

            A.CallTo(() => xrmFakedContext.GetFakedOrganizationService().Execute(A<CalculateRollupFieldRequest>._)).Returns(calculateRollupFieldResponse);
            xrmFakedContext.ProxyTypesAssembly = Assembly.GetAssembly(typeof(cmc_trip));
            var mockServiceProvider = InitializeMockService(xrmFakedContext, cmcTripActivity, Operation.Create);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            #endregion ARRANGE

            #region ACT
            var mockLogger = new Mock<ILogger>();

            //Mock the IBingMap
            var mockBingServices = InitializeBingMapMockService();
            var mockTripActivityService = new Lifecycle.TripActivityService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService(), mockBingServices.Object, new Mock<ILanguageService>().Object);
            mockTripActivityService.TripActivityUpdateLatitudeLongitude(mockExecutionContext.Object);
            #endregion ACT

            #region ASSERT
            var latitude = cmcTripActivity.Attributes["cmc_tripactivitylatitude"];
            var longitude = cmcTripActivity.Attributes["cmc_tripactivitylongitude"];
            Assert.IsNotNull(latitude);
            Assert.IsNotNull(longitude);
            #endregion ASSERT
        }

        /// <summary>
        /// Update Latitude Longitude for Given Location 
        /// </summary>
        [TestMethod]
        [TestCategory("Plugin"), TestCategory("Positive")]
        public void TripActivityService_TripActivityUpdateLatitudeLongitude_Update_ActivitytypeAppointmentIsChanged()
        {
            #region ARRANGE

            var bingMapKeyConfigInstance = PrepareBingMapKeyConfigInstance();
            var owner = PrepareSystemUser();

            var appointment = CreateAppointment();
            var appointmentOnChange = CreateAppointment();

            var cmcTrip = CreateCmcTrip(owner.Id);
            var cmcTripActivity = CreateTripActivityAppointment(owner.Id, cmcTrip, appointment);
            var cmcTripActivityOnChange = CreateTripActivityAppointment(owner.Id, cmcTrip, appointmentOnChange);


            var preImageEntity = PreparePreImageActivityTypeAppointment(cmcTripActivityOnChange);
            var xrmFakedContext = new XrmFakedContext();

            //xrmFakedContext.ProxyTypesAssembly = Assembly.Load("Cmc.Engage.Contracts.Tests");
            xrmFakedContext.Initialize(new List<Entity>()
            {
                bingMapKeyConfigInstance,
                owner,
                cmcTrip,
                appointment,
                cmcTripActivity,
                appointmentOnChange,
                cmcTripActivityOnChange
            });

            var mockServiceProvider = InitializeMockService(xrmFakedContext, cmcTripActivity, Operation.Update);
            AddPreEntityImage(mockServiceProvider, "PreImage", preImageEntity);

            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);

            var calculateRollupFieldRequest = new CalculateRollupFieldRequest()
            {
            };

            var calculateRollupFieldResponse = new CalculateRollupFieldResponse()
            {
            };
            A.CallTo(() => xrmFakedContext.GetFakedOrganizationService().Execute(A<CalculateRollupFieldRequest>._)).Returns(calculateRollupFieldResponse);
            #endregion ARRANGE

            #region ACT
            var mockLogger = new Mock<ILogger>();

            //Mock the IBingMap
            var mockConfigurationService = new ConfigurationService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
            var mockBingServices = InitializeBingMapMockService();
            //var mockTripActivityService = new TripActivityService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService(), mockBingServices);
            var mockTripActivityService = new Lifecycle.TripActivityService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService(), mockBingServices.Object, new Mock<ILanguageService>().Object);
            mockTripActivityService.TripActivityUpdateLatitudeLongitude(mockExecutionContext.Object);
            #endregion ACT

            #region ASSERT

            var updatedLatitude = cmcTripActivity.Attributes["cmc_tripactivitylatitude"];
            var updatedLongitude = cmcTripActivity.Attributes["cmc_tripactivitylongitude"];
            var latitude = preImageEntity.Attributes["cmc_tripactivitylatitude"];
            var longitude = preImageEntity.Attributes["cmc_tripactivitylongitude"];
            Assert.AreNotEqual(latitude, updatedLatitude);
            Assert.AreNotEqual(longitude, updatedLongitude);
            #endregion ASSERT
        }

        [TestMethod]
        [TestCategory("Plugin"), TestCategory("Positive")]
        public void TripActivityService_TripActivityUpdateLatitudeLongitude_Update_ActivitytypeEventIsChanged()
        {
            #region ARRANGE

            var bingMapKeyConfigInstance = PrepareBingMapKeyConfigInstance();
            var owner = PrepareSystemUser();

            var listEvent = CreateMsEvent(owner.Id);
            var listEventOnChange = CreateMsEvent(owner.Id);

            var cmcTrip = CreateCmcTrip(owner.Id);
            var cmcTripActivity = CreateTripActivityEvent(owner.Id, cmcTrip, listEvent);
            var cmcTripActivityOnChange = CreateTripActivityEvent(owner.Id, cmcTrip, listEventOnChange);


            var preImageEntity = PreparePreImageActivityTypeEvent(cmcTripActivityOnChange);
            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.ProxyTypesAssembly = Assembly.GetAssembly(typeof(Cmc.Engage.Models.Tests.cmc_trip));

            //xrmFakedContext.ProxyTypesAssembly = Assembly.Load("Cmc.Engage.Contracts.Tests");
            xrmFakedContext.Initialize(new List<Entity>()
            {
                bingMapKeyConfigInstance,
                owner,
                cmcTrip,
                listEvent,
                listEventOnChange,
                cmcTripActivity,
                cmcTripActivityOnChange
            });

            var mockServiceProvider = InitializeMockService(xrmFakedContext, cmcTripActivity, Operation.Update);
            AddPreEntityImage(mockServiceProvider, "PreImage", preImageEntity);

            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            var attributeMetadata = new LookupAttributeMetadata()
            {
                //LogicalName = "msevtmgt_event",
                Targets = new string[] { listEvent.LogicalName },
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

            var calculateRollupFieldResponse = new CalculateRollupFieldResponse()
            {
            };
            A.CallTo(() => xrmFakedContext.GetFakedOrganizationService().Execute(A<CalculateRollupFieldRequest>._)).Returns(calculateRollupFieldResponse);

            #endregion ARRANGE

            #region ACT
            var mockLogger = new Mock<ILogger>();

            //Mock the IBingMap
            var mockConfigurationService = new ConfigurationService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
            var mockBingServices = InitializeBingMapMockService();
            //var mockTripActivityService = new TripActivityService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService(), mockBingServices);
            var mockTripActivityService = new Lifecycle.TripActivityService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService(), mockBingServices.Object, new Mock<ILanguageService>().Object);
            mockTripActivityService.TripActivityUpdateLatitudeLongitude(mockExecutionContext.Object);
            #endregion ACT

            #region ASSERT

            var updatedLatitude = cmcTripActivity.Attributes["cmc_tripactivitylatitude"];
            var updatedLongitude = cmcTripActivity.Attributes["cmc_tripactivitylongitude"];
            var latitude = preImageEntity.Attributes["cmc_tripactivitylatitude"];
            var longitude = preImageEntity.Attributes["cmc_tripactivitylongitude"];
            Assert.AreNotEqual(latitude, updatedLatitude);
            Assert.AreNotEqual(longitude, updatedLongitude);
            #endregion ASSERT
        }

        [TestMethod]
        [TestCategory("Plugin"), TestCategory("Negative")]
        public void TripActivityService_TripActivityUpdateLatitudeLongitude_Update_ActivityTypeAppointmentIsChangedToOther()
        {
            #region ARRANGE

            var bingMapKeyConfigInstance = PrepareBingMapKeyConfigInstance();
            var owner = PrepareSystemUser();

            var appointment = CreateAppointment();
            var contact = CreateContact();
            var cmcTrip = CreateCmcTrip(owner.Id);
            var cmcTripActivity = CreateTripActivityAppointment(owner.Id, cmcTrip, appointment);
            var cmcTripActivityOthers = CreateTripActivityOthers1(contact, owner.Id, cmcTrip, appointment);

            var preImageEntity = PreparePreImageActivityTypeAppointment(cmcTripActivityOthers);
            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.ProxyTypesAssembly = Assembly.GetAssembly(typeof(cmc_trip));

            //xrmFakedContext.ProxyTypesAssembly = Assembly.Load("Cmc.Engage.Contracts.Tests");
            xrmFakedContext.Initialize(new List<Entity>()
            {
                bingMapKeyConfigInstance,
                owner,
                cmcTrip,
                appointment,
                cmcTripActivity,
            });

            var mockServiceProvider = InitializeMockService(xrmFakedContext, cmcTripActivity, Operation.Update);
            AddPreEntityImage(mockServiceProvider, "PreImage", preImageEntity);

            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            var calculateRollupFieldRequest = new CalculateRollupFieldRequest()
            {
            };

            var calculateRollupFieldResponse = new CalculateRollupFieldResponse()
            {
            };
            A.CallTo(() => xrmFakedContext.GetFakedOrganizationService().Execute(A<CalculateRollupFieldRequest>._)).Returns(calculateRollupFieldResponse);
            #endregion ARRANGE

            #region ACT
            var mockLogger = new Mock<ILogger>();

            //Mock the IBingMap
            var mockConfigurationService = new ConfigurationService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
            var mockBingServices = InitializeBingMapMockService();
            //var mockTripActivityService = new TripActivityService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService(), mockBingServices);
            var mockTripActivityService = new Lifecycle.TripActivityService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService(), mockBingServices.Object, new Mock<ILanguageService>().Object);
            mockTripActivityService.TripActivityUpdateLatitudeLongitude(mockExecutionContext.Object);
            #endregion ACT

            #region ASSERT

            var updatedLatitude = cmcTripActivity.Attributes["cmc_tripactivitylatitude"];
            var updatedLongitude = cmcTripActivity.Attributes["cmc_tripactivitylongitude"];
            var latitude = preImageEntity.Attributes["cmc_tripactivitylatitude"];
            var longitude = preImageEntity.Attributes["cmc_tripactivitylongitude"];
            Assert.AreNotEqual(latitude, updatedLatitude);
            Assert.AreNotEqual(longitude, updatedLongitude);
            #endregion ASSERT
        }

        [TestMethod]
        [TestCategory("Plugin"), TestCategory("Negative")]
        public void TripActivityService_TripActivityUpdateLatitudeLongitude_Update_ActivityTypeEventIsChangedToOther()
        {
            #region ARRANGE

            var bingMapKeyConfigInstance = PrepareBingMapKeyConfigInstance();
            var owner = PrepareSystemUser();

            var listEvent = CreateMsEvent(owner.Id);
            var contact = CreateContact1();

            var cmcTrip = CreateCmcTrip(owner.Id);
            var cmcTripActivity = CreateTripActivityEvent(owner.Id, cmcTrip, listEvent);
            var cmcTripActivityOthers = CreateTripActivityOthers2(contact, owner.Id, cmcTrip, listEvent);

            var preImageEntity = PreparePreImageActivityTypeEvent(cmcTripActivityOthers);
            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.ProxyTypesAssembly = Assembly.GetAssembly(typeof(Cmc.Engage.Models.Tests.cmc_trip));

            //xrmFakedContext.ProxyTypesAssembly = Assembly.Load("Cmc.Engage.Contracts.Tests");
            xrmFakedContext.Initialize(new List<Entity>()
            {
                bingMapKeyConfigInstance,
                owner,
                cmcTrip,
                listEvent,
                cmcTripActivity,
            });

            var mockServiceProvider = InitializeMockService(xrmFakedContext, cmcTripActivity, Operation.Update);
            AddPreEntityImage(mockServiceProvider, "PreImage", preImageEntity);

            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            var attributeMetadata = new LookupAttributeMetadata()
            {
                //LogicalName = "msevtmgt_event",
                Targets = new string[] { listEvent.LogicalName },
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

            var calculateRollupFieldRequest = new CalculateRollupFieldRequest()
            {
            };

            var calculateRollupFieldResponse = new CalculateRollupFieldResponse()
            {
            };
            A.CallTo(() => xrmFakedContext.GetFakedOrganizationService().Execute(A<CalculateRollupFieldRequest>._)).Returns(calculateRollupFieldResponse);
            #endregion ARRANGE

            #region ACT
            var mockLogger = new Mock<ILogger>();

            //Mock the IBingMap
            var mockConfigurationService = new ConfigurationService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
            var mockBingServices = InitializeBingMapMockService();
            //var mockTripActivityService = new TripActivityService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService(), mockBingServices);
            var mockTripActivityService = new Lifecycle.TripActivityService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService(), mockBingServices.Object, new Mock<ILanguageService>().Object);
            mockTripActivityService.TripActivityUpdateLatitudeLongitude(mockExecutionContext.Object);
            #endregion ACT

            #region ASSERT

            var updatedLatitude = cmcTripActivity.Attributes["cmc_tripactivitylatitude"];
            var updatedLongitude = cmcTripActivity.Attributes["cmc_tripactivitylongitude"];
            var latitude = preImageEntity.Attributes["cmc_tripactivitylatitude"];
            var longitude = preImageEntity.Attributes["cmc_tripactivitylongitude"];
            Assert.AreNotEqual(latitude, updatedLatitude);
            Assert.AreNotEqual(longitude, updatedLongitude);
            #endregion ASSERT
        }

        [TestMethod]
        [TestCategory("Plugin"), TestCategory("Negative")]
        public void TripActivityService_TripActivityUpdateLatitudeLongitude_Update_ActivityTypeAOtherIsChangedToAppoinment()
        {
            #region ARRANGE

            var bingMapKeyConfigInstance = PrepareBingMapKeyConfigInstance();
            var owner = PrepareSystemUser();

            var appointment = CreateAppointment();
            var contact = CreateContact();
            var cmcTrip = CreateCmcTrip(owner.Id);
            var cmcTripActivity = CreateTripActivityAppointment(owner.Id, cmcTrip, appointment);
            var cmcTripActivityOthers = CreateTripActivityOthers1(contact, owner.Id, cmcTrip, appointment);

            var preImageEntity = PreparePreImageActivityTypeAppointment(cmcTripActivity);
            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.ProxyTypesAssembly = Assembly.GetAssembly(typeof(cmc_trip));

            //xrmFakedContext.ProxyTypesAssembly = Assembly.Load("Cmc.Engage.Contracts.Tests");
            xrmFakedContext.Initialize(new List<Entity>()
            {
                bingMapKeyConfigInstance,
                owner,
                cmcTrip,
                appointment,
                cmcTripActivityOthers,
            });

            var mockServiceProvider = InitializeMockService(xrmFakedContext, cmcTripActivityOthers, Operation.Update);
            AddPreEntityImage(mockServiceProvider, "PreImage", preImageEntity);

            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            var calculateRollupFieldRequest = new CalculateRollupFieldRequest()
            {
            };

            var calculateRollupFieldResponse = new CalculateRollupFieldResponse()
            {
            };
            A.CallTo(() => xrmFakedContext.GetFakedOrganizationService().Execute(A<CalculateRollupFieldRequest>._)).Returns(calculateRollupFieldResponse);
            #endregion ARRANGE

            #region ACT
            var mockLogger = new Mock<ILogger>();

            //Mock the IBingMap
            var mockConfigurationService = new ConfigurationService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
            var mockBingServices = InitializeBingMapMockService();
            //var mockTripActivityService = new TripActivityService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService(), mockBingServices);
            var mockTripActivityService = new Lifecycle.TripActivityService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService(), mockBingServices.Object, new Mock<ILanguageService>().Object);
            mockTripActivityService.TripActivityUpdateLatitudeLongitude(mockExecutionContext.Object);
            #endregion ACT

            #region ASSERT

            var latitude = cmcTripActivityOthers.Attributes["cmc_tripactivitylatitude"];
            var longitude = cmcTripActivityOthers.Attributes["cmc_tripactivitylongitude"];
            Assert.IsNotNull(latitude);
            Assert.IsNotNull(longitude);
            #endregion ASSERT
        }
        [TestMethod]
        [TestCategory("Plugin"), TestCategory("Negative")]
        public void TripActivityService_TripActivityUpdateLatitudeLongitude_Update_TripActivityOwnerIsChanged()
        {
            #region ARRANGE

            var bingMapKeyConfigInstance = PrepareBingMapKeyConfigInstance();
            var owner = PrepareSystemUser();
            var owner1 = PrepareSystemUser();

            var contact = CreateContact1();

            var appointment = CreateAppointment();
            var cmcTrip = CreateCmcTripwithContact(owner.Id, contact);
            var cmcTripActivity = CreateTripActivityAppointment1(owner.Id, cmcTrip, appointment);

            var preImageEntity = PreparePreImageActivityTypeAppointmentOwnerChanged(owner1, cmcTripActivity);

            var associateContactForTrip = new Entity("cmc_trip_contact", Guid.NewGuid())
            {

                ["contactid"] = contact.Id,
                ["cmc_tripid"] = cmcTrip.Id
            };
            var associateStaffMemberForTripActivity = new Entity("cmc_tripactivity_systemuser", Guid.NewGuid())
            {

                ["cmc_tripactivityid"] = cmcTripActivity.Id,
                ["systemuserid"] = owner.Id
            };
            var associateStaffMemberForTrip = new Entity("cmc_trip_systemuser", Guid.NewGuid())
            {

                ["cmc_tripid"] = cmcTrip.Id,
                ["systemuserid"] = owner.Id
            };
            var xrmFakedContext = new XrmFakedContext();

            xrmFakedContext.Initialize(new List<Entity>()
            {
                associateContactForTrip,
                associateStaffMemberForTripActivity,
                associateStaffMemberForTrip,

                bingMapKeyConfigInstance,
                owner,
                cmcTrip,
                cmcTripActivity,
                contact
            });

            xrmFakedContext.AddRelationship("cmc_trip_contact", new XrmFakedRelationship
            {
                IntersectEntity = "cmc_trip_contact",
                RelationshipType = XrmFakedRelationship.enmFakeRelationshipType.ManyToMany,
                Entity1LogicalName = cmcTrip.LogicalName,
                Entity1Attribute = "cmc_tripid",
                Entity2LogicalName = contact.LogicalName,
                Entity2Attribute = "contactid"
            });

            xrmFakedContext.AddRelationship("cmc_trip_systemuser", new XrmFakedRelationship
            {
                IntersectEntity = "cmc_trip_systemuser",
                RelationshipType = XrmFakedRelationship.enmFakeRelationshipType.ManyToMany,
                Entity1LogicalName = cmcTrip.LogicalName,
                Entity1Attribute = "cmc_tripid",
                Entity2LogicalName = owner.LogicalName,
                Entity2Attribute = "systemuserid"
            });

            xrmFakedContext.AddRelationship("cmc_tripactivity_systemuser", new XrmFakedRelationship
            {
                IntersectEntity = "cmc_tripactivity_systemuser",
                RelationshipType = XrmFakedRelationship.enmFakeRelationshipType.ManyToMany,
                Entity1LogicalName = cmcTripActivity.LogicalName,
                Entity1Attribute = "cmc_tripactivityid",
                Entity2LogicalName = owner.LogicalName,
                Entity2Attribute = "systemuserid"
            });

            xrmFakedContext.ProxyTypesAssembly = Assembly.GetAssembly(typeof(cmc_trip));
            var mockServiceProvider = InitializeMockService(xrmFakedContext, cmcTripActivity, Operation.Update);
            AddPreEntityImage(mockServiceProvider, "PreImage", preImageEntity);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);

            var calculateRollupFieldRequest = new CalculateRollupFieldRequest()
            {
            };

            var calculateRollupFieldResponse = new CalculateRollupFieldResponse()
            {
            };
            A.CallTo(() => xrmFakedContext.GetFakedOrganizationService().Execute(A<CalculateRollupFieldRequest>._)).Returns(calculateRollupFieldResponse);
            #endregion ARRANGE

            #region ACT
            var mockLogger = new Mock<ILogger>();

            //Mock the IBingMap
            var mockConfigurationService = new ConfigurationService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
            var mockBingServices = InitializeBingMapMockService();
            //var mockTripActivityService = new TripActivityService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService(), mockBingServices);
            var mockTripActivityService = new Lifecycle.TripActivityService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService(), mockBingServices.Object, new Mock<ILanguageService>().Object);
            mockTripActivityService.TripActivityUpdateLatitudeLongitude(mockExecutionContext.Object);
            #endregion ACT

            #region ASSERT

            var updatedLatitude = cmcTripActivity.Attributes["cmc_tripactivitylatitude"];
            var updatedLongitude = cmcTripActivity.Attributes["cmc_tripactivitylongitude"];
            var latitude = preImageEntity.Attributes["cmc_tripactivitylatitude"];
            var longitude = preImageEntity.Attributes["cmc_tripactivitylongitude"];
            Assert.AreNotEqual(latitude, updatedLatitude);
            Assert.AreNotEqual(longitude, updatedLongitude);
            #endregion ASSERT
        }

        [TestMethod]
        [TestCategory("Plugin"), TestCategory("Positive")]
        public void TripActivityService_TripActivityUpdateLatitudeLongitude_Delete()
        {
            #region ARRANGE

            var bingMapKeyConfigInstance = PrepareBingMapKeyConfigInstance();
            var owner = PrepareSystemUser();

            var appointment = CreateAppointment();

            var cmcTrip = CreateCmcTrip(owner.Id);
            var cmcTripActivity = CreateTripActivityAppointment(owner.Id, cmcTrip, appointment);

            cmcTrip["cmc_estexpense"] = (Money)cmcTripActivity.Attributes["cmc_estimatedexpense"]; ;
            cmcTrip["cmc_actexpense"] = (Money)cmcTripActivity.Attributes["cmc_actualexpense"];


            var preImageEntity = PreparePreImage(cmcTripActivity);
            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.ProxyTypesAssembly = Assembly.GetAssembly(typeof(cmc_trip));

            //xrmFakedContext.ProxyTypesAssembly = Assembly.Load("Cmc.Engage.Contracts.Tests");
            xrmFakedContext.Initialize(new List<Entity>()
            {
                bingMapKeyConfigInstance,
                owner,
                cmcTrip,
                appointment,
                cmcTripActivity,
            });

            var mockServiceProvider = InitializeMockService(xrmFakedContext, cmcTripActivity, Operation.Delete);
            AddPreEntityImage(mockServiceProvider, "PreImage", preImageEntity);

            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);

            var calculateRollupFieldRequest = new CalculateRollupFieldRequest()
            {
            };

            var calculateRollupFieldResponse = new CalculateRollupFieldResponse()
            {
            };
            A.CallTo(() => xrmFakedContext.GetFakedOrganizationService().Execute(A<CalculateRollupFieldRequest>._)).Returns(calculateRollupFieldResponse);
            #endregion ARRANGE

            #region ACT
            var mockLogger = new Mock<ILogger>();

            //Mock the IBingMap
            var mockConfigurationService = new ConfigurationService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
            var mockBingServices = InitializeBingMapMockService();
            //var mockTripActivityService = new TripActivityService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService(), mockBingServices);
            var mockTripActivityService = new Lifecycle.TripActivityService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService(), mockBingServices.Object, new Mock<ILanguageService>().Object);
            mockTripActivityService.TripActivityUpdateLatitudeLongitude(mockExecutionContext.Object);
            #endregion ACT

            #region ASSERT

            var updatedTripActExpense = cmcTrip.GetAttributeValue<Money>("cmc_actexpense");
            var updatedTripExpExpense = cmcTrip.GetAttributeValue<Money>("cmc_estexpense");

            Assert.AreEqual(updatedTripActExpense.Value, 123);
            Assert.AreEqual(updatedTripExpExpense.Value, 123);
            #endregion ASSERT
        }

        [TestMethod]
        [TestCategory("Plugin"), TestCategory("Positive")]
        public void TripActivityService_TripActivityUpdateLatitudeLongitude_TripActivityLocationIsNull()
        {
            #region ARRANGE

            var bingMapKeyConfigInstance = PrepareBingMapKeyConfigInstance();
            var owner = PrepareSystemUser();

            var appointment = CreateAppointment();
            var cmcTrip = CreateCmcTrip(owner.Id);
            var cmcTripActivity = CreateTripActivityAppointment(owner.Id, cmcTrip, appointment);
            cmcTripActivity["cmc_location"] = null;
            var preImageEntity = PreparePreImage(cmcTripActivity);
            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.ProxyTypesAssembly = Assembly.GetAssembly(typeof(cmc_trip));

            //xrmFakedContext.ProxyTypesAssembly = Assembly.Load("Cmc.Engage.Contracts.Tests");
            xrmFakedContext.Initialize(new List<Entity>()
            {
                bingMapKeyConfigInstance,
                owner,
                cmcTrip,
                appointment,
                cmcTripActivity,
            });

            var mockServiceProvider = InitializeMockService(xrmFakedContext, cmcTripActivity, Operation.Update);
            AddPreEntityImage(mockServiceProvider, "PreImage", preImageEntity);

            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);

            var calculateRollupFieldRequest = new CalculateRollupFieldRequest()
            {
            };

            var calculateRollupFieldResponse = new CalculateRollupFieldResponse()
            {
            };
            A.CallTo(() => xrmFakedContext.GetFakedOrganizationService().Execute(A<CalculateRollupFieldRequest>._)).Returns(calculateRollupFieldResponse);
            #endregion ARRANGE

            #region ACT
            var mockLogger = new Mock<ILogger>();

            //Mock the IBingMap
            var mockConfigurationService = new ConfigurationService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
            var mockBingServices = InitializeBingMapMockService();
            //var mockTripActivityService = new TripActivityService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService(), mockBingServices);
            var mockTripActivityService = new Lifecycle.TripActivityService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService(), mockBingServices.Object, new Mock<ILanguageService>().Object);
            mockTripActivityService.TripActivityUpdateLatitudeLongitude(mockExecutionContext.Object);
            #endregion ACT

            #region ASSERT

            var location = cmcTripActivity.Attributes["cmc_location"];
            Assert.IsNull(location);
            #endregion ASSERT
        }

        #region DATA PREPARATION

        private cmc_trip_systemuser TripSystemUser()
        {
            return new Models.Tests.cmc_trip_systemuser()
            {
                Id = Guid.NewGuid(),

            };
        }
        private SystemUser PrepareSystemUser()
        {
            return new SystemUser()
            {
                Id = Guid.NewGuid()
            };
        }

        private Appointment CreateAppointment()
        {
            return new Appointment()
            {
                Id = Guid.NewGuid(),
                Subject = "AppTest",
                Location = "Malur, Kolar, Karnataka, India",
                ScheduledStart = DateTime.Now,
                ScheduledEnd = DateTime.Now.AddDays(5)
            };
        }
        private Contact CreateContact()
        {
            return new Contact()
            {
                Id = Guid.NewGuid()
            };
        }
        private Models.Tests.Contact CreateContact1()
        {
            return new Models.Tests.Contact()
            {
                Id = Guid.NewGuid()
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
                //cmc_trip_contact = new List<Contact>()
                //{
                //   CreateContact(),CreateContact()
                //},
            };
        }
        private Models.Tests.cmc_trip CreateCmcTripwithContact(Guid userId, Models.Tests.Contact contactEntity)
        {
            return new Models.Tests.cmc_trip()
            {
                Id = Guid.NewGuid(),
                OwnerId = new EntityReference("systemuser", userId),
                cmc_tripname = "Unit Test Trip",
                cmc_StartDate = DateTime.Now,
                cmc_EndDate = DateTime.Now.AddDays(10),
                cmc_trip_contact = new List<Models.Tests.Contact>()
                {
                    contactEntity
                }
            };
        }

        private cmc_tripactivity CreateTripActivityAppointment1(Guid userId, Models.Tests.cmc_trip cmcTrip, Appointment appointment)
        {
            return new cmc_tripactivity()
            {
                Id = Guid.NewGuid(),
                OwnerId = new EntityReference("systemuser", userId),
                cmc_trip = new EntityReference("cmc_trip", cmcTrip.Id),
                cmc_activitytype = cmc_tripactivity_cmc_activitytype.Appointment,
                cmc_LinkedToAppointment = appointment.ToEntityReference(),
                cmc_StartDateTime = appointment.GetAttributeValue<DateTime>("scheduledstart"),
                cmc_EndDateTime = appointment.GetAttributeValue<DateTime>("scheduledend"),
                cmc_location = appointment.GetAttributeValue<String>("location"),
                ["cmc_estimatedexpense"] = new Money(123),
                ["cmc_actualexpense"] = new Money(123)
            };
        }
        private cmc_tripactivity CreateTripActivityAppointment(Guid userId, cmc_trip cmcTrip, Appointment appointment)
        {
            return new cmc_tripactivity()
            {
                Id = Guid.NewGuid(),
                OwnerId = new EntityReference("systemuser", userId),
                cmc_trip = new EntityReference("cmc_trip", cmcTrip.Id),
                cmc_activitytype = cmc_tripactivity_cmc_activitytype.Appointment,
                cmc_LinkedToAppointment = appointment.ToEntityReference(),
                cmc_StartDateTime = appointment.GetAttributeValue<DateTime>("scheduledstart"),
                cmc_EndDateTime = appointment.GetAttributeValue<DateTime>("scheduledend"),
                cmc_location = appointment.GetAttributeValue<String>("location"),
                ["cmc_estimatedexpense"] = new Money(123),
                ["cmc_actualexpense"] = new Money(123),
            };
        }
        private cmc_tripactivity CreateTripActivityOthers(Contact contactEntity, Guid userId, cmc_trip cmcTrip)
        {
            return new cmc_tripactivity()
            {
                Id = Guid.NewGuid(),
                OwnerId = new EntityReference("systemuser", userId),
                cmc_trip = new EntityReference("cmc_trip", cmcTrip.Id),
                cmc_activitytype = cmc_tripactivity_cmc_activitytype.Other,
                cmc_tripactivity_contact = new List<Contact>()
                {
                    contactEntity
                },
                cmc_StartDateTime = DateTime.Now,
                cmc_EndDateTime = DateTime.Now.AddDays(5),
                cmc_location = "Malur,Kolar,Karnataka,India",
                ["cmc_estimatedexpense"] = new Money(123),
                ["cmc_actualexpense"] = new Money(123),
            };
        }
        private Models.Tests.cmc_tripactivity CreateTripActivityOthers2(Models.Tests.Contact contactEntity, Guid userId, cmc_trip cmcTrip, msevtmgt_Event eventList)
        {
            return new Models.Tests.cmc_tripactivity()
            {
                Id = Guid.NewGuid(),
                OwnerId = new EntityReference("systemuser", userId),
                cmc_trip = new EntityReference("cmc_trip", cmcTrip.Id),
                cmc_activitytype = Models.Tests.cmc_tripactivity_cmc_activitytype.Other,
                cmc_tripactivity_contact = new List<Models.Tests.Contact>()
                {
                    contactEntity
                },
                cmc_linkedtoevent = eventList.ToEntityReference(),
                cmc_StartDateTime = DateTime.Now,
                cmc_EndDateTime = DateTime.Now.AddDays(5),
                cmc_location = "Malur,Kolar,Karnataka,India",
                ["cmc_estimatedexpense"] = new Money(123),
                ["cmc_actualexpense"] = new Money(123),
            };
        }
        private cmc_tripactivity CreateTripActivityOthers1(Contact contactEntity, Guid userId, cmc_trip cmcTrip, Appointment appointment)
        {
            return new cmc_tripactivity()
            {
                Id = Guid.NewGuid(),
                OwnerId = new EntityReference("systemuser", userId),
                cmc_trip = new EntityReference("cmc_trip", cmcTrip.Id),
                cmc_activitytype = cmc_tripactivity_cmc_activitytype.Other,
                cmc_tripactivity_contact = new List<Contact>()
                {
                    contactEntity
                },
                cmc_LinkedToAppointment = appointment.ToEntityReference(),
                cmc_StartDateTime = DateTime.Now,
                cmc_EndDateTime = DateTime.Now.AddDays(5),
                cmc_location = "Malur,Kolar,Karnataka,India",
                ["cmc_estimatedexpense"] = new Money(123),
                ["cmc_actualexpense"] = new Money(123),
            };
        }
        private Models.Tests.cmc_tripactivity CreateTripActivityEvent(Guid userId, cmc_trip cmcTrip, Entity marketingList)
        {
            return new Models.Tests.cmc_tripactivity()
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
        private cmc_configuration PrepareBingMapKeyConfigInstance()
        {
            var bingMapKeyConfigInstance = new cmc_configuration()
            {
                Id = Guid.NewGuid(),
                cmc_configurationname = "BingMapApiKey",
                cmc_Value = "ApHcQX8UM8ulfNCQgjrGRFu5-He1C0BC2cFh9VtoJbKQG7FNzOT-0t_zau-a_LBh"
            };
            return bingMapKeyConfigInstance;
        }
        private Entity PreparePreImage(Entity cmcTripActivity)
        {
            var preImage = new Entity("cmc_tripactivity", cmcTripActivity.Id)
            {
                ["ownerid"] = cmcTripActivity.GetAttributeValue<EntityReference>("ownerid"),
                ["cmc_location"] = "Banglore, India",
                ["cmc_trip"] = cmcTripActivity.GetAttributeValue<EntityReference>("cmc_trip"),
                ["cmc_activitytype"] = cmcTripActivity.GetAttributeValue<OptionSetValue>("cmc_activitytype"),
                ["cmc_linkedtoappointment"] = cmcTripActivity.GetAttributeValue<EntityReference>("cmc_linkedtoappointment"),
                ["cmc_tripactivitylatitude"] = 56.3,
                ["cmc_tripactivitylongitude"] = 85.2
            };
            return preImage;
        }
        private Entity PreparePreImageActivityTypeAppointment(Entity cmcTripActivity)
        {
            var preImage = new Entity("cmc_tripactivity", cmcTripActivity.Id)
            {
                ["ownerid"] = cmcTripActivity.GetAttributeValue<EntityReference>("ownerid"),
                ["cmc_location"] = "Banglore, India",
                ["cmc_trip"] = cmcTripActivity.GetAttributeValue<EntityReference>("cmc_trip"),
                ["cmc_activitytype"] = cmcTripActivity.GetAttributeValue<OptionSetValue>("cmc_activitytype"),
                ["cmc_linkedtoappointment"] = cmcTripActivity.GetAttributeValue<EntityReference>("cmc_linkedtoappointment"),
                ["cmc_tripactivitylatitude"] = 56.3,
                ["cmc_tripactivitylongitude"] = 85.2
            };
            return preImage;
        }
        private Entity PreparePreImageActivityTypeAppointmentOwnerChanged(Entity userId, Entity cmcTripActivity)
        {
            var preImage = new Entity("cmc_tripactivity", cmcTripActivity.Id)
            {
                ["ownerid"] = userId.ToEntityReference(),
                ["cmc_location"] = "Banglore, India",
                ["cmc_trip"] = cmcTripActivity.GetAttributeValue<EntityReference>("cmc_trip"),
                ["cmc_activitytype"] = cmcTripActivity.GetAttributeValue<OptionSetValue>("cmc_activitytype"),
                ["cmc_linkedtoappointment"] = cmcTripActivity.GetAttributeValue<EntityReference>("cmc_linkedtoappointment"),
                ["cmc_tripactivitylatitude"] = 56.3,
                ["cmc_tripactivitylongitude"] = 85.2
            };
            return preImage;
        }

        private Entity PreparePreImageActivityTypeEvent(Entity cmcTripActivity)
        {
            var preImage = new Entity("cmc_tripactivity", cmcTripActivity.Id)
            {
                ["ownerid"] = cmcTripActivity.GetAttributeValue<EntityReference>("ownerid"),
                ["cmc_location"] = "Banglore, India",
                ["cmc_trip"] = cmcTripActivity.GetAttributeValue<EntityReference>("cmc_trip"),
                ["cmc_activitytype"] = cmcTripActivity.GetAttributeValue<OptionSetValue>("cmc_activitytype"),
                ["cmc_linkedtoevent"] = cmcTripActivity.GetAttributeValue<EntityReference>("cmc_linkedtoevent"),
                ["cmc_tripactivitylatitude"] = 56.3,
                ["cmc_tripactivitylongitude"] = 85.2
            };
            return preImage;
        }

        #endregion DATA PREPARATION

    }
}
