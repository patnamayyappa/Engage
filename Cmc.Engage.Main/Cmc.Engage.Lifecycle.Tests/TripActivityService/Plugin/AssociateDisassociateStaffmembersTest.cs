using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Cmc.Core.Xrm.ServerExtension.Core;
using Cmc.Core.Xrm.ServerExtension.Logging;
using Cmc.Engage.Common;
using Cmc.Engage.Common.Plugins.Tests.Utilities;
using Cmc.Engage.Models.Tests;
using FakeXrmEasy;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Moq;
using Appointment = Cmc.Engage.Models.Appointment;
using cmc_configuration = Cmc.Engage.Models.cmc_configuration;
using cmc_trip = Cmc.Engage.Models.cmc_trip;
using cmc_tripactivity = Cmc.Engage.Models.cmc_tripactivity;
using cmc_tripactivity_cmc_activitytype = Cmc.Engage.Models.cmc_tripactivity_cmc_activitytype;
using Contact = Cmc.Engage.Models.Contact;
using SystemUser = Cmc.Engage.Models.SystemUser;

namespace Cmc.Engage.Lifecycle.Tests.Trips.Plugin
{
    [TestClass]
    public class AssociateDisassociateStaffmembersTest : XrmUnitTestBase
    {
        [TestMethod]
        [TestCategory("Plugin"), TestCategory("Positive")]
        public void AssociateDisassociateStaffmembers_Associate_StaffmembersTripActivity()
        {
            #region ACT

            var bingMapKeyConfigInstance = PrepareBingMapKeyConfigInstance();
            var owner = PrepareSystemUser();
            var staffMember = PrepareSystemUser();
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
                ["systemuserid"] = staffMember.Id
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
                staffMember,
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
                Entity2LogicalName = staffMember.LogicalName,
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
            //xrmFakedContext.ProxyTypesAssembly = Assembly.GetAssembly(typeof(cmc_trip));
            xrmFakedContext.ProxyTypesAssembly = Assembly.GetAssembly(typeof(Models.Tests.msevtmgt_Event));

            var mockServiceProvider = InitializeMockService(xrmFakedContext, null, Operation.Associate);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            AddInputParameters(mockServiceProvider, "Target", cmcTripActivity.ToEntityReference());
            AddInputParameters(mockServiceProvider, "Relationship", new Relationship("cmc_tripactivity_systemuser"));
            AddInputParameters(mockServiceProvider, "RelatedEntities", new EntityReferenceCollection(new List<EntityReference>() { staffMember.ToEntityReference() }));

            #endregion

            #region ARRANGE

            var mockLogger = new Mock<ILogger>();

            //Mock the IBingMap
            var mockBingServices = InitializeBingMapMockService();
            var mockTripActivityService = new Lifecycle.TripActivityService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService(), mockBingServices.Object, new Mock<ILanguageService>().Object);
            mockTripActivityService.AssociateDisassociateStaffmembers(mockExecutionContext.Object);

            #endregion

            #region ASSERT
            //Check the staff member is added or not
            var fetchXml = $@"
                            <fetch>
                              <entity name='cmc_trip'>
                                <all-attributes />
                                <link-entity name='cmc_trip_contact' from='cmc_tripid' to='cmc_tripid' link-type='outer' intersect='true' alias='student' visible='true'>
                                  <attribute name='contactid' />
                                </link-entity>
                                <link-entity name='cmc_trip_systemuser' from='cmc_tripid' to='cmc_tripid' link-type='outer' alias='staff' visible='true'>
                                  <attribute name='systemuserid' />
                                </link-entity>
                               <link-entity name='cmc_tripactivity' from='cmc_trip' to='cmc_tripid'>
                                  <filter>
                                    <condition attribute='cmc_tripactivityid' operator='eq' value='{cmcTripActivity.Id}'/>
                                  </filter>
                                </link-entity>
                              </entity>
                            </fetch>";
            var lstCmcTrip = xrmFakedContext.GetFakedOrganizationService().RetrieveMultiple(new FetchExpression(fetchXml))?.Entities?.Select(r => r.ToEntity<cmc_trip>()).ToList();
            var lstTripuser = lstCmcTrip?.Select(r => r.GetAliasedAttributeValue<Guid>("staff.cmc_trip_systemuserid")).FirstOrDefault();
            Assert.AreEqual(lstTripuser.Value, associateStaffMemberForTrip.Id);

            #endregion
        }

        [TestMethod]
        [TestCategory("Plugin"), TestCategory("Positive")]
        public void AssociateDisassociateStaffmembers_Associate_ContactTripActivity_AlreadyRelated()
        {
            #region ACT

            var bingMapKeyConfigInstance = PrepareBingMapKeyConfigInstance();
            var owner = PrepareSystemUser();
            var staffMember = PrepareSystemUser();
            var appointment = CreateAppointment();
            var contact = CreateContact();
            var contact1 = CreateContact();
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
                ["systemuserid"] = staffMember.Id
            };
            var associateStaffMemberForTrip = new Entity("cmc_trip_systemuser", Guid.NewGuid())
            {

                ["cmc_tripid"] = cmcTrip.Id,
                ["systemuserid"] = owner.Id
            };
            var associateContactForTripActivity = new Entity("cmc_tripactivity_contact", Guid.NewGuid())
            {

                ["cmc_tripactivityid"] = cmcTripActivity.Id,
                ["contactid"] = contact1.Id
            };
            var xrmFakedContext = new XrmFakedContext();

            xrmFakedContext.Initialize(new List<Entity>()
            {
                associateContactForTrip,
                associateStaffMemberForTripActivity,
                associateStaffMemberForTrip,
                associateContactForTripActivity,

                associateStaff,
                bingMapKeyConfigInstance,
                owner,
                staffMember,
                cmcTrip,
                cmcTripActivity,
                appointment,
                contact1
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
                Entity2LogicalName = staffMember.LogicalName,
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
            //xrmFakedContext.ProxyTypesAssembly = Assembly.GetAssembly(typeof(cmc_trip));
            xrmFakedContext.ProxyTypesAssembly = Assembly.GetAssembly(typeof(Models.Tests.msevtmgt_Event));

            var mockServiceProvider = InitializeMockService(xrmFakedContext, null, Operation.Associate);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            AddInputParameters(mockServiceProvider, "Target", cmcTripActivity.ToEntityReference());
            AddInputParameters(mockServiceProvider, "Relationship", new Relationship("cmc_tripactivity_contact"));
            AddInputParameters(mockServiceProvider, "RelatedEntities", new EntityReferenceCollection(new List<EntityReference>() { contact.ToEntityReference() }));

            #endregion

            #region ARRANGE

            var mockLogger = new Mock<ILogger>();

            //Mock the IBingMap
            var mockBingServices = InitializeBingMapMockService();
            var mockTripActivityService = new Lifecycle.TripActivityService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService(), mockBingServices.Object, new Mock<ILanguageService>().Object);
            mockTripActivityService.AssociateDisassociateStaffmembers(mockExecutionContext.Object);

            #endregion

            #region ASSERT
            //Check the staff member is added or not
            var fetchXml = $@"
                            <fetch>
                              <entity name='cmc_trip'>
                                <all-attributes />
                                <link-entity name='cmc_trip_contact' from='cmc_tripid' to='cmc_tripid' link-type='outer' intersect='true' alias='student' visible='true'>
                                  <attribute name='contactid' />
                                </link-entity>
                                <link-entity name='cmc_trip_systemuser' from='cmc_tripid' to='cmc_tripid' link-type='outer' alias='staff' visible='true'>
                                  <attribute name='systemuserid' />
                                </link-entity>
                               <link-entity name='cmc_tripactivity' from='cmc_trip' to='cmc_tripid'>
                                  <filter>
                                    <condition attribute='cmc_tripactivityid' operator='eq' value='{cmcTripActivity.Id}'/>
                                  </filter>
                                </link-entity>
                              </entity>
                            </fetch>";
            var lstCmcTrip = xrmFakedContext.GetFakedOrganizationService().RetrieveMultiple(new FetchExpression(fetchXml))?.Entities?.Select(r => r.ToEntity<cmc_trip>()).ToList();
            var lstTripcontact = lstCmcTrip?.Select(r => r.GetAliasedAttributeValue<Guid>("student.cmc_trip_contactid")).FirstOrDefault();
            Assert.AreEqual(lstTripcontact.Value, associateContactForTrip.Id);

            #endregion
        }

        [TestMethod]
        [TestCategory("Plugin"), TestCategory("Positive")]
        public void AssociateDisassociateStaffmembers_Associate_ContactTripActivity()
        {
            #region ACT

            var bingMapKeyConfigInstance = PrepareBingMapKeyConfigInstance();
            var owner = PrepareSystemUser();
            var staffMember = PrepareSystemUser();
            var appointment = CreateAppointment();
            var contact = CreateContact();
            var contact1 = CreateContact();
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
                ["systemuserid"] = staffMember.Id
            };
            var associateStaffMemberForTrip = new Entity("cmc_trip_systemuser", Guid.NewGuid())
            {

                ["cmc_tripid"] = cmcTrip.Id,
                ["systemuserid"] = owner.Id
            };
            var associateContactForTripActivity = new Entity("cmc_tripactivity_contact", Guid.NewGuid())
            {

                ["cmc_tripactivityid"] = cmcTripActivity.Id,
                ["contactid"] = contact1.Id
            };
            var xrmFakedContext = new XrmFakedContext();

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
                Entity2LogicalName = staffMember.LogicalName,
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


            xrmFakedContext.Initialize(new List<Entity>()
            {
                associateContactForTrip,
                associateStaffMemberForTripActivity,
                associateStaffMemberForTrip,
                associateContactForTripActivity,

                associateStaff,
                bingMapKeyConfigInstance,
                owner,
                staffMember,
                cmcTrip,
                cmcTripActivity,
                appointment,
                contact1
            });


            //xrmFakedContext.ProxyTypesAssembly = Assembly.GetAssembly(typeof(cmc_trip));
            xrmFakedContext.ProxyTypesAssembly = Assembly.GetAssembly(typeof(Models.Tests.msevtmgt_Event));

            var mockServiceProvider = InitializeMockService(xrmFakedContext, null, Operation.Associate);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);
            AddInputParameters(mockServiceProvider, "Target", cmcTripActivity.ToEntityReference());
            AddInputParameters(mockServiceProvider, "Relationship", new Relationship("cmc_tripactivity_contact"));
            AddInputParameters(mockServiceProvider, "RelatedEntities", new EntityReferenceCollection(new List<EntityReference>() { contact1.ToEntityReference() }));

            #endregion

            #region ARRANGE

            var mockLogger = new Mock<ILogger>();

            //Mock the IBingMap
            var mockBingServices = InitializeBingMapMockService();
            var mockTripActivityService = new Lifecycle.TripActivityService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService(), mockBingServices.Object, new Mock<ILanguageService>().Object);
            mockTripActivityService.AssociateDisassociateStaffmembers(mockExecutionContext.Object);

            #endregion

            #region ASSERT
            //Check the staff member is added or not
            var fetchXml = $@"
                            <fetch>
                              <entity name='cmc_trip'>
                                <all-attributes />
                                <link-entity name='cmc_trip_contact' from='cmc_tripid' to='cmc_tripid' link-type='outer' intersect='true' alias='student' visible='true'>
                                  <attribute name='contactid' />
                                </link-entity>
                                <link-entity name='cmc_trip_systemuser' from='cmc_tripid' to='cmc_tripid' link-type='outer' alias='staff' visible='true'>
                                  <attribute name='systemuserid' />
                                </link-entity>
                               <link-entity name='cmc_tripactivity' from='cmc_trip' to='cmc_tripid'>
                                  <filter>
                                    <condition attribute='cmc_tripactivityid' operator='eq' value='{cmcTripActivity.Id}'/>
                                  </filter>
                                </link-entity>
                              </entity>
                            </fetch>";
            var lstCmcTrip = xrmFakedContext.GetFakedOrganizationService().RetrieveMultiple(new FetchExpression(fetchXml))?.Entities?.Select(r => r.ToEntity<cmc_trip>()).ToList();
            var lstTripcontact = lstCmcTrip?.Select(r => r.GetAliasedAttributeValue<Guid>("student.cmc_trip_contactid")).FirstOrDefault();
            Assert.AreEqual(lstTripcontact.Value, associateContactForTrip.Id);

            #endregion
        }

        [TestMethod]
        [TestCategory("Plugin"), TestCategory("Positive")]
        public void AssociateDisassociateStaffmembers_Dissociate_CmcTripContact_StaffmembersTripActivity()
        {
            #region ACT
            var owner = PrepareSystemUser();
            var appointment = CreateAppointment();
            var cmcTrip = CreateCmcTrip(owner.Id);
            var cmcTripActivity = CreateTripActivityAppointment(owner.Id, cmcTrip, appointment);
            var contact = CreateContact();
            var associateContact = CreateContact();

            var associateContactForTrip = new Entity("cmc_trip_contact", Guid.NewGuid())
            {

                ["contactid"] = contact.Id,
                ["cmc_tripid"] = cmcTrip.Id
            };
            var associateStaffMemberForTrip = new Entity("cmc_trip_systemuser", Guid.NewGuid())
            {

                ["cmc_tripid"] = cmcTrip.Id,
                ["systemuserid"] = owner.Id
            };

            var associateContactForTripActivity = new Entity("cmc_tripactivity_contact", Guid.NewGuid())
            {

                ["cmc_tripactivityid"] = cmcTripActivity.Id,
                ["contactid"] = associateContact.Id
            };
            var associateSystemUserForTripActivity = new Entity("cmc_tripactivity_systemuser", Guid.NewGuid())
            {

                ["cmc_tripactivityid"] = cmcTripActivity.Id,
                ["systemuserid"] = owner.Id
            };
            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>()
            {
                associateContactForTrip,
                associateStaffMemberForTrip,
                associateContactForTripActivity,
                associateSystemUserForTripActivity,

                owner,
                associateContact,
                contact,
                appointment,
                cmcTrip,
                cmcTripActivity
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
            xrmFakedContext.AddRelationship("cmc_trip_contact", new XrmFakedRelationship
            {
                IntersectEntity = "cmc_trip_contact",
                RelationshipType = XrmFakedRelationship.enmFakeRelationshipType.ManyToMany,
                Entity1LogicalName = cmcTrip.LogicalName,
                Entity1Attribute = "cmc_tripid",
                Entity2LogicalName = contact.LogicalName,
                Entity2Attribute = "contactid"
            });
            xrmFakedContext.AddRelationship("cmc_tripactivity_contact", new XrmFakedRelationship
            {
                IntersectEntity = "cmc_tripactivity_contact",
                RelationshipType = XrmFakedRelationship.enmFakeRelationshipType.ManyToMany,
                Entity1LogicalName = cmcTripActivity.LogicalName,
                Entity1Attribute = "cmc_tripactivityid",
                Entity2LogicalName = associateContact.LogicalName,
                Entity2Attribute = "contactid"
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
            var mockServiceProvider = InitializeMockService(xrmFakedContext, null, Operation.Disassociate);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);

            #endregion

            #region ARRANGE

            var mockLogger = new Mock<ILogger>();
            AddInputParameters(mockServiceProvider, "Target", cmcTripActivity.ToEntityReference());
            AddInputParameters(mockServiceProvider, "Relationship", new Relationship("cmc_tripactivity_contact"));
            AddInputParameters(mockServiceProvider, "RelatedEntities", new EntityReferenceCollection(new List<EntityReference>() { contact.ToEntityReference(), associateContact.ToEntityReference() }));

            //Mock the IBingMap
            var mockBingServices = InitializeBingMapMockService();
            var mockTripActivityService = new Lifecycle.TripActivityService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService(), mockBingServices.Object, new Mock<ILanguageService>().Object);
            mockTripActivityService.AssociateDisassociateStaffmembers(mockExecutionContext.Object);

            #endregion

            #region ASSERT

            var fetchXml = $@"
                            <fetch>
                              <entity name='cmc_trip'>
                                <all-attributes />
                                <link-entity name='cmc_trip_contact' from='cmc_tripid' to='cmc_tripid' link-type='outer' intersect='true' alias='student' visible='true'>
                                  <attribute name='contactid' />
                                </link-entity>
                                <link-entity name='cmc_trip_systemuser' from='cmc_tripid' to='cmc_tripid' link-type='outer' alias='staff' visible='true'>
                                  <attribute name='systemuserid' />
                                </link-entity>
                               <link-entity name='cmc_tripactivity' from='cmc_trip' to='cmc_tripid'>
                                  <filter>
                                    <condition attribute='cmc_tripactivityid' operator='eq' value='{cmcTripActivity.Id}'/>
                                  </filter>
                                </link-entity>
                              </entity>
                            </fetch>";
            var lstCmcTrip = xrmFakedContext.GetFakedOrganizationService().RetrieveMultiple(new FetchExpression(fetchXml))?.Entities?.Select(r => r.ToEntity<cmc_trip>()).ToList();
            var lstTripcontact = lstCmcTrip?.Select(r => r.GetAliasedAttributeValue<Guid>("student.cmc_trip_contactid")).FirstOrDefault();
            Assert.AreEqual(lstTripcontact.Value, Guid.Empty);

            #endregion
        }

        [TestMethod]
        [TestCategory("Plugin"), TestCategory("Positive")]
        public void AssociateDisassociateStaffmembers_Dissociate_CmcTripContact_StaffmembersTripActivity_UsesSameContact()
        {
            #region ACT
            var owner = PrepareSystemUser();
            var appointment = CreateAppointment();
            var cmcTrip = CreateCmcTrip(owner.Id);
            var cmcTripActivity = CreateTripActivityAppointment(owner.Id, cmcTrip, appointment);
            var cmcTripActivity1 = CreateTripActivityAppointment(owner.Id, cmcTrip, appointment);

            var contact = CreateContact();
            var associateContact = CreateContact();

            var associateContactForTrip = new Entity("cmc_trip_contact", Guid.NewGuid())
            {
                ["contactid"] = contact.Id,
                ["cmc_tripid"] = cmcTrip.Id
            };
            var associateStaffMemberForTrip = new Entity("cmc_trip_systemuser", Guid.NewGuid())
            {
                ["cmc_tripid"] = cmcTrip.Id,
                ["systemuserid"] = owner.Id
            };

            var associateContactForTripActivity = new Entity("cmc_tripactivity_contact", Guid.NewGuid())
            {
                ["cmc_tripactivityid"] = cmcTripActivity.Id,
                ["contactid"] = contact.Id
            };
            var associateContactForTripActivity1 = new Entity("cmc_tripactivity_contact", Guid.NewGuid())
            {
                ["cmc_tripactivityid"] = cmcTripActivity1.Id,
                ["contactid"] = contact.Id
            };
            var associateSystemUserForTripActivity = new Entity("cmc_tripactivity_systemuser", Guid.NewGuid())
            {
                ["cmc_tripactivityid"] = cmcTripActivity.Id,
                ["systemuserid"] = owner.Id
            };
            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>()
            {
                associateContactForTrip,
                associateStaffMemberForTrip,
                associateContactForTripActivity,
                associateSystemUserForTripActivity,
                associateContactForTripActivity1,
                cmcTripActivity1,
                owner,
                contact,
                associateContact,
                appointment,
                cmcTrip,
                cmcTripActivity
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
            xrmFakedContext.AddRelationship("cmc_trip_contact", new XrmFakedRelationship
            {
                IntersectEntity = "cmc_trip_contact",
                RelationshipType = XrmFakedRelationship.enmFakeRelationshipType.ManyToMany,
                Entity1LogicalName = cmcTrip.LogicalName,
                Entity1Attribute = "cmc_tripid",
                Entity2LogicalName = contact.LogicalName,
                Entity2Attribute = "contactid"
            });
            xrmFakedContext.AddRelationship("cmc_tripactivity_contact", new XrmFakedRelationship
            {
                IntersectEntity = "cmc_tripactivity_contact",
                RelationshipType = XrmFakedRelationship.enmFakeRelationshipType.ManyToMany,
                Entity1LogicalName = cmcTripActivity.LogicalName,
                Entity1Attribute = "cmc_tripactivityid",
                Entity2LogicalName = contact.LogicalName,
                Entity2Attribute = "contactid"
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

            var mockServiceProvider = InitializeMockService(xrmFakedContext, null, Operation.Disassociate);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);

            #endregion

            #region ARRANGE

            var mockLogger = new Mock<ILogger>();
            AddInputParameters(mockServiceProvider, "Target", cmcTripActivity.ToEntityReference());
            AddInputParameters(mockServiceProvider, "Relationship", new Relationship("cmc_tripactivity_contact"));
            AddInputParameters(mockServiceProvider, "RelatedEntities", new EntityReferenceCollection(new List<EntityReference>() { contact.ToEntityReference() }));

            //Mock the IBingMap
            var mockBingServices = InitializeBingMapMockService();
            var mockTripActivityService = new Lifecycle.TripActivityService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService(), mockBingServices.Object, new Mock<ILanguageService>().Object);
            mockTripActivityService.AssociateDisassociateStaffmembers(mockExecutionContext.Object);

            #endregion

            #region ASSERT

            var fetchXml = $@"
                            <fetch>
                              <entity name='cmc_trip'>
                                <all-attributes />
                                <link-entity name='cmc_trip_contact' from='cmc_tripid' to='cmc_tripid' link-type='outer' intersect='true' alias='student' visible='true'>
                                  <attribute name='contactid' />
                                </link-entity>
                                <link-entity name='cmc_trip_systemuser' from='cmc_tripid' to='cmc_tripid' link-type='outer' alias='staff' visible='true'>
                                  <attribute name='systemuserid' />
                                </link-entity>
                               <link-entity name='cmc_tripactivity' from='cmc_trip' to='cmc_tripid'>
                                  <filter>
                                    <condition attribute='cmc_tripactivityid' operator='eq' value='{cmcTripActivity.Id}'/>
                                  </filter>
                                </link-entity>
                              </entity>
                            </fetch>";
            var lstCmcTrip = xrmFakedContext.GetFakedOrganizationService().RetrieveMultiple(new FetchExpression(fetchXml))?.Entities?.Select(r => r.ToEntity<cmc_trip>()).ToList();
            var lstTripcontact = lstCmcTrip?.Select(r => r.GetAliasedAttributeValue<Guid>("student.cmc_trip_contactid")).FirstOrDefault();
            Assert.AreEqual(lstTripcontact.Value, associateContactForTrip.Id);

            #endregion
        }

        [TestMethod]
        [TestCategory("Plugin"), TestCategory("Negative")]
        public void AssociateDisassociateStaffmembers_Dissociate_CmcTripContact_StaffmembersTripActivity_NotAssociated()
        {
            #region ACT
            var owner = PrepareSystemUser();
            var appointment = CreateAppointment();
            var cmcTrip = CreateCmcTrip(owner.Id);
            var cmcTripActivity = CreateTripActivityAppointment(owner.Id, cmcTrip, appointment);

            var contact = CreateContact();
            var associateContact = CreateContact();

            var associateStaffMemberForTrip = new Entity("cmc_trip_systemuser", Guid.NewGuid())
            {

                ["cmc_tripid"] = cmcTrip.Id,
                ["systemuserid"] = owner.Id
            };
            var associateContactForTripActivity = new Entity("cmc_tripactivity_contact", Guid.NewGuid())
            {

                ["cmc_tripactivityid"] = cmcTripActivity.Id,
                ["contactid"] = contact.Id
            };
            var associateSystemUserForTripActivity = new Entity("cmc_tripactivity_systemuser", Guid.NewGuid())
            {

                ["cmc_tripactivityid"] = cmcTripActivity.Id,
                ["systemuserid"] = owner.Id
            };
            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>()
            {
                //associateContactForTrip,
                associateStaffMemberForTrip,
                associateContactForTripActivity,
                associateSystemUserForTripActivity,
                owner,
                contact,
                associateContact,
                appointment,
                cmcTrip,
                cmcTripActivity
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
            xrmFakedContext.AddRelationship("cmc_trip_contact", new XrmFakedRelationship
            {
                IntersectEntity = "cmc_trip_contact",
                RelationshipType = XrmFakedRelationship.enmFakeRelationshipType.ManyToMany,
                Entity1LogicalName = cmcTrip.LogicalName,
                Entity1Attribute = "cmc_tripid",
                Entity2LogicalName = contact.LogicalName,
                Entity2Attribute = "contactid"
            });
            xrmFakedContext.AddRelationship("cmc_tripactivity_contact", new XrmFakedRelationship
            {
                IntersectEntity = "cmc_tripactivity_contact",
                RelationshipType = XrmFakedRelationship.enmFakeRelationshipType.ManyToMany,
                Entity1LogicalName = cmcTripActivity.LogicalName,
                Entity1Attribute = "cmc_tripactivityid",
                Entity2LogicalName = contact.LogicalName,
                Entity2Attribute = "contactid"
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

            var mockServiceProvider = InitializeMockService(xrmFakedContext, null, Operation.Disassociate);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);

            #endregion

            #region ARRANGE

            var mockLogger = new Mock<ILogger>();
            AddInputParameters(mockServiceProvider, "Target", cmcTripActivity.ToEntityReference());
            AddInputParameters(mockServiceProvider, "Relationship", new Relationship("cmc_tripactivity_contact"));
            AddInputParameters(mockServiceProvider, "RelatedEntities", new EntityReferenceCollection(new List<EntityReference>() { contact.ToEntityReference() }));

            //Mock the IBingMap
            var mockBingServices = InitializeBingMapMockService();
            var mockTripActivityService = new Lifecycle.TripActivityService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService(), mockBingServices.Object, new Mock<ILanguageService>().Object);
            mockTripActivityService.AssociateDisassociateStaffmembers(mockExecutionContext.Object);

            #endregion

            #region ASSERT

            var fetchXml = $@"
                            <fetch>
                              <entity name='cmc_trip'>
                                <all-attributes />
                                <link-entity name='cmc_trip_contact' from='cmc_tripid' to='cmc_tripid' link-type='outer' intersect='true' alias='student' visible='true'>
                                  <attribute name='contactid' />
                                </link-entity>
                                <link-entity name='cmc_trip_systemuser' from='cmc_tripid' to='cmc_tripid' link-type='outer' alias='staff' visible='true'>
                                  <attribute name='systemuserid' />
                                </link-entity>
                               <link-entity name='cmc_tripactivity' from='cmc_trip' to='cmc_tripid'>
                                  <filter>
                                    <condition attribute='cmc_tripactivityid' operator='eq' value='{cmcTripActivity.Id}'/>
                                  </filter>
                                </link-entity>
                              </entity>
                            </fetch>";
            var lstCmcTrip = xrmFakedContext.GetFakedOrganizationService().RetrieveMultiple(new FetchExpression(fetchXml))?.Entities?.Select(r => r.ToEntity<cmc_trip>()).ToList();
            var lstTripcontact = lstCmcTrip?.Select(r => r.GetAliasedAttributeValue<Guid>("student.cmc_trip_contactid")).FirstOrDefault();
            Assert.AreEqual(lstTripcontact.Value, Guid.Empty);

            #endregion
        }

        [TestMethod]
        [TestCategory("Plugin"), TestCategory("Positive")]
        public void AssociateDisassociateStaffmembers_Dissociate_CmcTripSystemuser_StaffmembersTripActivity()
        {
            #region ACT
            var owner = PrepareSystemUser();
            var appointment = CreateAppointment();
            var cmcTrip = CreateCmcTrip(owner.Id);
            var cmcTripActivity = CreateTripActivityAppointment(owner.Id, cmcTrip, appointment);
            var contact = CreateContact();
            var associateOwner = PrepareSystemUser();

            var associateContactForTrip = new Entity("cmc_trip_contact", Guid.NewGuid())
            {

                ["contactid"] = contact.Id,
                ["cmc_tripid"] = cmcTrip.Id
            };
            var associateStaffMemberForTrip = new Entity("cmc_trip_systemuser", Guid.NewGuid())
            {

                ["cmc_tripid"] = cmcTrip.Id,
                ["systemuserid"] = owner.Id
            };

            var associateContactForTripActivity = new Entity("cmc_tripactivity_contact", Guid.NewGuid())
            {

                ["cmc_tripactivityid"] = cmcTripActivity.Id,
                ["contactid"] = contact.Id
            };
            var associateSystemUserForTripActivity = new Entity("cmc_tripactivity_systemuser", Guid.NewGuid())
            {

                ["cmc_tripactivityid"] = cmcTripActivity.Id,
                ["systemuserid"] = owner.Id
            };
            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>()
            {
                associateContactForTrip,
                associateStaffMemberForTrip,
                associateContactForTripActivity,
                associateSystemUserForTripActivity,

                owner,
                associateOwner,
                appointment,
                cmcTrip,
                cmcTripActivity
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
            xrmFakedContext.AddRelationship("cmc_trip_contact", new XrmFakedRelationship
            {
                IntersectEntity = "cmc_trip_contact",
                RelationshipType = XrmFakedRelationship.enmFakeRelationshipType.ManyToMany,
                Entity1LogicalName = cmcTrip.LogicalName,
                Entity1Attribute = "cmc_tripid",
                Entity2LogicalName = contact.LogicalName,
                Entity2Attribute = "contactid"
            });
            xrmFakedContext.AddRelationship("cmc_tripactivity_contact", new XrmFakedRelationship
            {
                IntersectEntity = "cmc_tripactivity_contact",
                RelationshipType = XrmFakedRelationship.enmFakeRelationshipType.ManyToMany,
                Entity1LogicalName = cmcTripActivity.LogicalName,
                Entity1Attribute = "cmc_tripactivityid",
                Entity2LogicalName = contact.LogicalName,
                Entity2Attribute = "contactid"
            });
            xrmFakedContext.AddRelationship("cmc_tripactivity_systemuser", new XrmFakedRelationship
            {
                IntersectEntity = "cmc_tripactivity_systemuser",
                RelationshipType = XrmFakedRelationship.enmFakeRelationshipType.ManyToMany,
                Entity1LogicalName = cmcTripActivity.LogicalName,
                Entity1Attribute = "cmc_tripactivityid",
                Entity2LogicalName = associateOwner.LogicalName,
                Entity2Attribute = "systemuserid"
            });
            var mockServiceProvider = InitializeMockService(xrmFakedContext, null, Operation.Disassociate);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);

            #endregion

            #region ARRANGE

            var mockLogger = new Mock<ILogger>();
            AddInputParameters(mockServiceProvider, "Target", cmcTripActivity.ToEntityReference());
            AddInputParameters(mockServiceProvider, "Relationship", new Relationship("cmc_tripactivity_systemuser"));
            AddInputParameters(mockServiceProvider, "RelatedEntities", new EntityReferenceCollection(new List<EntityReference>() { owner.ToEntityReference(), associateOwner.ToEntityReference() }));

            //Mock the IBingMap
            var mockBingServices = InitializeBingMapMockService();
            var mockTripActivityService = new Lifecycle.TripActivityService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService(), mockBingServices.Object, new Mock<ILanguageService>().Object);
            mockTripActivityService.AssociateDisassociateStaffmembers(mockExecutionContext.Object);

            #endregion

            #region ASSERT

            var fetchXml = $@"
                            <fetch>
                              <entity name='cmc_trip'>
                                <all-attributes />
                                <link-entity name='cmc_trip_contact' from='cmc_tripid' to='cmc_tripid' link-type='outer' intersect='true' alias='student' visible='true'>
                                  <attribute name='contactid' />
                                </link-entity>
                                <link-entity name='cmc_trip_systemuser' from='cmc_tripid' to='cmc_tripid' link-type='outer' alias='staff' visible='true'>
                                  <attribute name='systemuserid' />
                                </link-entity>
                               <link-entity name='cmc_tripactivity' from='cmc_trip' to='cmc_tripid'>
                                  <filter>
                                    <condition attribute='cmc_tripactivityid' operator='eq' value='{cmcTripActivity.Id}'/>
                                  </filter>
                                </link-entity>
                              </entity>
                            </fetch>";
            var lstCmcTrip = xrmFakedContext.GetFakedOrganizationService().RetrieveMultiple(new FetchExpression(fetchXml))?.Entities?.Select(r => r.ToEntity<cmc_trip>()).ToList();
            var lstTripuser = lstCmcTrip?.Select(r => r.GetAliasedAttributeValue<Guid>("staff.cmc_trip_systemuserid")).FirstOrDefault();
            Assert.AreEqual(lstTripuser.Value, Guid.Empty);

            #endregion
        }

        [TestMethod]
        [TestCategory("Plugin"), TestCategory("Positive")]
        public void AssociateDisassociateStaffmembers_Dissociate_CmcTripSystemuser_StaffmembersTripActivity_UsesSameUser()
        {
            #region ACT
            var owner = PrepareSystemUser();
            var appointment = CreateAppointment();
            var cmcTrip = CreateCmcTrip(owner.Id);
            var cmcTripActivity = CreateTripActivityAppointment(owner.Id, cmcTrip, appointment);
            var cmcTripActivity1 = CreateTripActivityAppointment(owner.Id, cmcTrip, appointment);

            var contact = CreateContact();
            var associateOwner = PrepareSystemUser();

            var associateContactForTrip = new Entity("cmc_trip_contact", Guid.NewGuid())
            {

                ["contactid"] = contact.Id,
                ["cmc_tripid"] = cmcTrip.Id
            };
            var associateStaffMemberForTrip = new Entity("cmc_trip_systemuser", Guid.NewGuid())
            {
                //["cmc_tripactivity_systemuserid"] = Guid.NewGuid(),
                ["cmc_tripid"] = cmcTrip.Id,
                ["systemuserid"] = owner.Id
            };

            var associateContactForTripActivity = new Entity("cmc_tripactivity_contact", Guid.NewGuid())
            {

                ["cmc_tripactivityid"] = cmcTripActivity.Id,
                ["contactid"] = contact.Id
            };
            var associateSystemUserForTripActivity = new Entity("cmc_tripactivity_systemuser", Guid.NewGuid())
            {
                //["cmc_tripactivity_systemuserid"] = Guid.NewGuid(),
                ["cmc_tripactivityid"] = cmcTripActivity.Id,
                ["systemuserid"] = owner.Id
            };
            var associateSystemUserForTripActivity1 = new Entity("cmc_tripactivity_systemuser", Guid.NewGuid())
            {

                ["cmc_tripactivityid"] = cmcTripActivity1.Id,
                ["systemuserid"] = owner.Id
            };

            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>()
            {
                associateContactForTrip,
                associateStaffMemberForTrip,
                associateContactForTripActivity,
                associateSystemUserForTripActivity,
                associateSystemUserForTripActivity1,
                owner,
                cmcTripActivity1,
                associateOwner,
                appointment,
                cmcTrip,
                cmcTripActivity
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
            xrmFakedContext.AddRelationship("cmc_trip_contact", new XrmFakedRelationship
            {
                IntersectEntity = "cmc_trip_contact",
                RelationshipType = XrmFakedRelationship.enmFakeRelationshipType.ManyToMany,
                Entity1LogicalName = cmcTrip.LogicalName,
                Entity1Attribute = "cmc_tripid",
                Entity2LogicalName = contact.LogicalName,
                Entity2Attribute = "contactid"
            });
            xrmFakedContext.AddRelationship("cmc_tripactivity_contact", new XrmFakedRelationship
            {
                IntersectEntity = "cmc_tripactivity_contact",
                RelationshipType = XrmFakedRelationship.enmFakeRelationshipType.ManyToMany,
                Entity1LogicalName = cmcTripActivity.LogicalName,
                Entity1Attribute = "cmc_tripactivityid",
                Entity2LogicalName = contact.LogicalName,
                Entity2Attribute = "contactid"
            });
            xrmFakedContext.AddRelationship("cmc_tripactivity_systemuser", new XrmFakedRelationship
            {
                IntersectEntity = "cmc_tripactivity_systemuser",
                RelationshipType = XrmFakedRelationship.enmFakeRelationshipType.ManyToMany,
                Entity1LogicalName = cmcTripActivity.LogicalName,
                Entity1Attribute = "cmc_tripactivityid",
                Entity2LogicalName = associateOwner.LogicalName,
                Entity2Attribute = "systemuserid"
            });
            var mockServiceProvider = InitializeMockService(xrmFakedContext, null, Operation.Disassociate);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);

            #endregion

            #region ARRANGE

            var mockLogger = new Mock<ILogger>();
            AddInputParameters(mockServiceProvider, "Target", cmcTripActivity.ToEntityReference());
            AddInputParameters(mockServiceProvider, "Relationship", new Relationship("cmc_tripactivity_systemuser"));
            AddInputParameters(mockServiceProvider, "RelatedEntities", new EntityReferenceCollection(new List<EntityReference>() { owner.ToEntityReference(), associateOwner.ToEntityReference() }));

            //Mock the IBingMap
            var mockBingServices = InitializeBingMapMockService();
            var mockTripActivityService = new Lifecycle.TripActivityService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService(), mockBingServices.Object, new Mock<ILanguageService>().Object);
            mockTripActivityService.AssociateDisassociateStaffmembers(mockExecutionContext.Object);

            #endregion

            #region ASSERT

            var fetchXml = $@"
                            <fetch>
                              <entity name='cmc_trip'>
                                <all-attributes />
                                <link-entity name='cmc_trip_contact' from='cmc_tripid' to='cmc_tripid' link-type='outer' intersect='true' alias='student' visible='true'>
                                  <attribute name='contactid' />
                                </link-entity>
                                <link-entity name='cmc_trip_systemuser' from='cmc_tripid' to='cmc_tripid' link-type='outer' alias='staff' visible='true'>
                                  <attribute name='systemuserid' />
                                </link-entity>
                               <link-entity name='cmc_tripactivity' from='cmc_trip' to='cmc_tripid'>
                                  <filter>
                                    <condition attribute='cmc_tripactivityid' operator='eq' value='{cmcTripActivity.Id}'/>
                                  </filter>
                                </link-entity>
                              </entity>
                            </fetch>";
            var lstCmcTrip = xrmFakedContext.GetFakedOrganizationService().RetrieveMultiple(new FetchExpression(fetchXml))?.Entities?.Select(r => r.ToEntity<cmc_trip>()).ToList();
            var lstTripuser = lstCmcTrip?.Select(r => r.GetAliasedAttributeValue<Guid>("staff.cmc_trip_systemuserid")).FirstOrDefault();
            Assert.AreEqual(lstTripuser.Value, associateStaffMemberForTrip.Id);


            #endregion
        }

        [TestMethod]
        [TestCategory("Plugin"), TestCategory("Negative")]
        public void AssociateDisassociateStaffmembers_Dissociate_CmcTripSystemuser_StaffmembersTripActivity_NotAssociated()
        {
            #region ACT
            var owner = PrepareSystemUser();
            var appointment = CreateAppointment();
            var cmcTrip = CreateCmcTrip(owner.Id);
            var cmcTripActivity = CreateTripActivityAppointment(owner.Id, cmcTrip, appointment);

            var contact = CreateContact();
            var associateContact = CreateContact();

            var associateContactForTrip = new Entity("cmc_trip_contact", Guid.NewGuid())
            {

                ["contactid"] = contact.Id,
                ["cmc_tripid"] = cmcTrip.Id
            };
            //var associateStaffMemberForTrip = new Entity("cmc_trip_systemuser", Guid.NewGuid())
            //{
            //    ["cmc_tripactivity_systemuserid"] = Guid.NewGuid(),
            //    ["cmc_tripid"] = cmcTrip.Id,
            //    ["systemuserid"] = owner.Id
            //};
            var associateContactForTripActivity = new Entity("cmc_tripactivity_contact", Guid.NewGuid())
            {

                ["cmc_tripactivityid"] = cmcTripActivity.Id,
                ["contactid"] = contact.Id
            };
            var associateSystemUserForTripActivity = new Entity("cmc_tripactivity_systemuser", Guid.NewGuid())
            {

                ["cmc_tripactivityid"] = cmcTripActivity.Id,
                ["systemuserid"] = owner.Id
            };
            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.Initialize(new List<Entity>()
            {
                associateContactForTrip,
                //associateStaffMemberForTrip,
                associateContactForTripActivity,
                associateSystemUserForTripActivity,
                owner,
                contact,
                associateContact,
                appointment,
                cmcTrip,
                cmcTripActivity
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
            xrmFakedContext.AddRelationship("cmc_trip_contact", new XrmFakedRelationship
            {
                IntersectEntity = "cmc_trip_contact",
                RelationshipType = XrmFakedRelationship.enmFakeRelationshipType.ManyToMany,
                Entity1LogicalName = cmcTrip.LogicalName,
                Entity1Attribute = "cmc_tripid",
                Entity2LogicalName = contact.LogicalName,
                Entity2Attribute = "contactid"
            });
            xrmFakedContext.AddRelationship("cmc_tripactivity_contact", new XrmFakedRelationship
            {
                IntersectEntity = "cmc_tripactivity_contact",
                RelationshipType = XrmFakedRelationship.enmFakeRelationshipType.ManyToMany,
                Entity1LogicalName = cmcTripActivity.LogicalName,
                Entity1Attribute = "cmc_tripactivityid",
                Entity2LogicalName = contact.LogicalName,
                Entity2Attribute = "contactid"
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

            var mockServiceProvider = InitializeMockService(xrmFakedContext, null, Operation.Disassociate);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);

            #endregion

            #region ARRANGE

            var mockLogger = new Mock<ILogger>();
            AddInputParameters(mockServiceProvider, "Target", cmcTripActivity.ToEntityReference());
            AddInputParameters(mockServiceProvider, "Relationship", new Relationship("cmc_tripactivity_systemuser"));
            AddInputParameters(mockServiceProvider, "RelatedEntities", new EntityReferenceCollection(new List<EntityReference>() { owner.ToEntityReference() }));

            //Mock the IBingMap
            var mockBingServices = InitializeBingMapMockService();
            var mockTripActivityService = new Lifecycle.TripActivityService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService(), mockBingServices.Object, new Mock<ILanguageService>().Object);
            mockTripActivityService.AssociateDisassociateStaffmembers(mockExecutionContext.Object);

            #endregion

            #region ASSERT

            var fetchXml = $@"
                            <fetch>
                              <entity name='cmc_trip'>
                                <all-attributes />
                                <link-entity name='cmc_trip_contact' from='cmc_tripid' to='cmc_tripid' link-type='outer' intersect='true' alias='student' visible='true'>
                                  <attribute name='contactid' />
                                </link-entity>
                                <link-entity name='cmc_trip_systemuser' from='cmc_tripid' to='cmc_tripid' link-type='outer' alias='staff' visible='true'>
                                  <attribute name='systemuserid' />
                                </link-entity>
                               <link-entity name='cmc_tripactivity' from='cmc_trip' to='cmc_tripid'>
                                  <filter>
                                    <condition attribute='cmc_tripactivityid' operator='eq' value='{cmcTripActivity.Id}'/>
                                  </filter>
                                </link-entity>
                              </entity>
                            </fetch>";
            var lstCmcTrip = xrmFakedContext.GetFakedOrganizationService().RetrieveMultiple(new FetchExpression(fetchXml))?.Entities?.Select(r => r.ToEntity<cmc_trip>()).ToList();
            var lstTripuser = lstCmcTrip?.Select(r => r.GetAliasedAttributeValue<Guid>("staff.cmc_trip_systemuserid")).FirstOrDefault();
            Assert.AreEqual(lstTripuser.Value, Guid.Empty);

            #endregion
        }

        [TestMethod]
        [TestCategory("Plugin"), TestCategory("Positive")]
        public void AssociateDisassociateStaffmembers_Dissociate_CmcTripContact_StaffmembersTrip()
        {
            #region ACT
            var owner = PrepareSystemUser();
            var appointment = CreateAppointment();
            var cmcTrip = CreateCmcTrip(owner.Id);
            var cmcTrip1 = CreateCmcTrip(owner.Id);
            var usersetting = PreparingUserSetiing(owner);

            var cmcTripActivity = CreateTripActivityAppointment(owner.Id, cmcTrip, appointment);
            var contact = CreateContact();
            var associateContact = CreateContact();

            var associateContactForTrip = new Entity("cmc_trip_contact", Guid.NewGuid())
            {

                ["contactid"] = contact.Id,
                ["cmc_tripid"] = cmcTrip1.Id
            };

            var associateContactForTrip1 = new Entity("cmc_trip_contact", Guid.NewGuid())
            {

                ["contactid"] = associateContact.Id,
                ["cmc_tripid"] = cmcTrip.Id
            };

            var associateStaffMemberForTrip = new Entity("cmc_trip_systemuser", Guid.NewGuid())
            {
                ["cmc_trip_systemuser"] = Guid.NewGuid(),
                ["cmc_tripid"] = cmcTrip.Id,
                ["systemuserid"] = owner.Id
            };

            var associateContactForTripActivity = new Entity("cmc_tripactivity_contact", Guid.NewGuid())
            {

                ["cmc_tripactivityid"] = cmcTripActivity.Id,
                ["contactid"] = associateContact.Id
            };
            var associateSystemUserForTripActivity = new Entity("cmc_tripactivity_systemuser", Guid.NewGuid())
            {

                ["cmc_tripactivityid"] = cmcTripActivity.Id,
                ["systemuserid"] = owner.Id
            };


            var xrmFakedContext = new XrmFakedContext();

            xrmFakedContext.ProxyTypesAssembly = Assembly.GetAssembly(typeof(UserSettings));

            xrmFakedContext.AddRelationship("cmc_trip_systemuser", new XrmFakedRelationship
            {
                IntersectEntity = "cmc_trip_systemuser",
                RelationshipType = XrmFakedRelationship.enmFakeRelationshipType.ManyToMany,
                Entity1LogicalName = cmcTrip.LogicalName,
                Entity1Attribute = "cmc_tripid",
                Entity2LogicalName = owner.LogicalName,
                Entity2Attribute = "systemuserid"
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
            xrmFakedContext.AddRelationship("cmc_tripactivity_contact", new XrmFakedRelationship
            {
                IntersectEntity = "cmc_tripactivity_contact",
                RelationshipType = XrmFakedRelationship.enmFakeRelationshipType.ManyToMany,
                Entity1LogicalName = cmcTripActivity.LogicalName,
                Entity1Attribute = "cmc_tripactivityid",
                Entity2LogicalName = associateContact.LogicalName,
                Entity2Attribute = "contactid"
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
            xrmFakedContext.Initialize(new List<Entity>()
            {
                associateContactForTrip,
                associateContactForTrip1,
                associateStaffMemberForTrip,
                associateContactForTripActivity,
                associateSystemUserForTripActivity,

                owner,
                usersetting,
                associateContact,
                contact,
                appointment,
                cmcTrip,
                cmcTrip1,
                cmcTripActivity,

            });

            
            var mockServiceProvider = InitializeMockService(xrmFakedContext, null, Operation.Disassociate);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);

            #endregion

            #region ARRANGE

            var mockLogger = new Mock<ILogger>();
            AddInputParameters(mockServiceProvider, "Target", cmcTripActivity.ToEntityReference());
            AddInputParameters(mockServiceProvider, "Relationship", new Relationship("cmc_trip_contact"));
            AddInputParameters(mockServiceProvider, "RelatedEntities", new EntityReferenceCollection(new List<EntityReference>() { associateContact.ToEntityReference() }));

            //Mock the IBingMap
            var mockBingServices = InitializeBingMapMockService();
            var mockLanguageService = new LanguageService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
            var mockTripActivityService = new Lifecycle.TripActivityService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService(), mockBingServices.Object, mockLanguageService);
            //mockTripActivityService.AssociateDisassociateStaffmembers(mockExecutionContext.Object);

            #endregion

            #region ASSERT

            Assert.ThrowsException<InvalidPluginExecutionException>(() => mockTripActivityService.AssociateDisassociateStaffmembers(mockExecutionContext.Object));

            #endregion
        }

        [TestMethod]
        [TestCategory("Plugin"), TestCategory("Positive")]
        public void AssociateDisassociateStaffmembers_Dissociate_CmcTripSystemuser_StaffmembersTrip_TripActivityUsesSameStaff()
        {
            #region ACT
            var owner = PrepareSystemUser();
            var owner1 = PrepareSystemUser();

            var appointment = CreateAppointment();
            var cmcTrip = CreateCmcTrip(owner.Id);
            var cmcTrip1 = CreateCmcTrip(owner1.Id);
            var usersetting = PreparingUserSetiing(owner);

            var cmcTripActivity = CreateTripActivityAppointment(owner.Id, cmcTrip, appointment);
            var contact = CreateContact();

            var associateContactForTrip = new Entity("cmc_trip_contact", Guid.NewGuid())
            {

                ["contactid"] = contact.Id,
                ["cmc_tripid"] = cmcTrip.Id
            };
            var associateStaffMemberForTrip = new Entity("cmc_trip_systemuser", Guid.NewGuid())
            {
                ["cmc_trip_systemuser"] = Guid.NewGuid(),
                ["cmc_tripid"] = cmcTrip.Id,
                ["systemuserid"] = owner.Id
            };
            var associateStaffMemberForTrip1 = new Entity("cmc_trip_systemuser", Guid.NewGuid())
            {
                ["cmc_trip_systemuser"] = Guid.NewGuid(),
                ["cmc_tripid"] = cmcTrip1.Id,
                ["systemuserid"] = owner1.Id
            };
            var associateContactForTripActivity = new Entity("cmc_tripactivity_contact", Guid.NewGuid())
            {

                ["cmc_tripactivityid"] = cmcTripActivity.Id,
                ["contactid"] = contact.Id
            };
            var associateSystemUserForTripActivity = new Entity("cmc_tripactivity_systemuser", Guid.NewGuid())
            {

                ["cmc_tripactivityid"] = cmcTripActivity.Id,
                ["systemuserid"] = owner.Id
            };
            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.ProxyTypesAssembly = Assembly.GetAssembly(typeof(UserSettings));

            xrmFakedContext.Initialize(new List<Entity>()
            {
                associateContactForTrip,
                associateStaffMemberForTrip,
                associateContactForTripActivity,
                associateSystemUserForTripActivity,
                associateStaffMemberForTrip1,

                owner,
                owner1,
                usersetting,
                appointment,
                cmcTrip,
                cmcTripActivity,
                contact
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
            xrmFakedContext.AddRelationship("cmc_trip_contact", new XrmFakedRelationship
            {
                IntersectEntity = "cmc_trip_contact",
                RelationshipType = XrmFakedRelationship.enmFakeRelationshipType.ManyToMany,
                Entity1LogicalName = cmcTrip.LogicalName,
                Entity1Attribute = "cmc_tripid",
                Entity2LogicalName = contact.LogicalName,
                Entity2Attribute = "contactid"
            });
            xrmFakedContext.AddRelationship("cmc_tripactivity_contact", new XrmFakedRelationship
            {
                IntersectEntity = "cmc_tripactivity_contact",
                RelationshipType = XrmFakedRelationship.enmFakeRelationshipType.ManyToMany,
                Entity1LogicalName = cmcTripActivity.LogicalName,
                Entity1Attribute = "cmc_tripactivityid",
                Entity2LogicalName = contact.LogicalName,
                Entity2Attribute = "contactid"
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
            var mockServiceProvider = InitializeMockService(xrmFakedContext, null, Operation.Disassociate);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);

            #endregion

            #region ARRANGE

            var mockLogger = new Mock<ILogger>();
            AddInputParameters(mockServiceProvider, "Target", cmcTripActivity.ToEntityReference());
            AddInputParameters(mockServiceProvider, "Relationship", new Relationship("cmc_trip_systemuser"));
            AddInputParameters(mockServiceProvider, "RelatedEntities", new EntityReferenceCollection(new List<EntityReference>() { owner.ToEntityReference() }));

            //Mock the IBingMap
            var mockBingServices = InitializeBingMapMockService();
            var mockLanguageService = new LanguageService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
            var mockTripActivityService = new Lifecycle.TripActivityService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService(), mockBingServices.Object, mockLanguageService);
            #endregion

            #region ASSERT

            Assert.ThrowsException<InvalidPluginExecutionException>(() => mockTripActivityService.AssociateDisassociateStaffmembers(mockExecutionContext.Object));

            #endregion
        }

        [TestMethod]
        [TestCategory("Plugin"), TestCategory("Positive")]
        public void AssociateDisassociateStaffmembers_Dissociate_CmcTripSystemuser_StaffmembersTrip_TripActivityUsesOwner()
        {
            #region ACT
            var owner = PrepareSystemUser();
            var owner1 = PrepareSystemUser();

            var appointment = CreateAppointment();
            var cmcTrip = CreateCmcTrip(owner.Id);
            var usersetting = PreparingUserSetiing(owner);

            var cmcTripActivity = CreateTripActivityAppointment(owner.Id, cmcTrip, appointment);
            var cmcTripActivity1 = CreateTripActivityAppointment(owner1.Id, cmcTrip, appointment);

            var contact = CreateContact();

            var associateContactForTrip = new Entity("cmc_trip_contact", Guid.NewGuid())
            {

                ["contactid"] = contact.Id,
                ["cmc_tripid"] = cmcTrip.Id
            };
            var associateStaffMemberForTrip = new Entity("cmc_trip_systemuser", Guid.NewGuid())
            {
                ["cmc_trip_systemuser"] = Guid.NewGuid(),
                ["cmc_tripid"] = cmcTrip.Id,
                ["systemuserid"] = owner1.Id
            };
            var associateContactForTripActivity = new Entity("cmc_tripactivity_contact", Guid.NewGuid())
            {

                ["cmc_tripactivityid"] = cmcTripActivity.Id,
                ["contactid"] = contact.Id
            };
            var associateSystemUserForTripActivity = new Entity("cmc_tripactivity_systemuser", Guid.NewGuid())
            {

                ["cmc_tripactivityid"] = cmcTripActivity.Id,
                ["systemuserid"] = owner1.Id
            };
            var associateSystemUserForTripActivity1 = new Entity("cmc_tripactivity_systemuser", Guid.NewGuid())
            {

                ["cmc_tripactivityid"] = cmcTripActivity1.Id,
                ["systemuserid"] = owner1.Id
            };
            var xrmFakedContext = new XrmFakedContext();
            xrmFakedContext.ProxyTypesAssembly = Assembly.GetAssembly(typeof(UserSettings));

            xrmFakedContext.Initialize(new List<Entity>()
            {
                associateContactForTrip,
                associateStaffMemberForTrip,
                associateContactForTripActivity,
                associateSystemUserForTripActivity,
                associateSystemUserForTripActivity1,
                owner,
                owner1,
                usersetting,
                appointment,
                cmcTrip,
                cmcTripActivity,
                cmcTripActivity1,
                contact
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
            xrmFakedContext.AddRelationship("cmc_trip_contact", new XrmFakedRelationship
            {
                IntersectEntity = "cmc_trip_contact",
                RelationshipType = XrmFakedRelationship.enmFakeRelationshipType.ManyToMany,
                Entity1LogicalName = cmcTrip.LogicalName,
                Entity1Attribute = "cmc_tripid",
                Entity2LogicalName = contact.LogicalName,
                Entity2Attribute = "contactid"
            });
            xrmFakedContext.AddRelationship("cmc_tripactivity_contact", new XrmFakedRelationship
            {
                IntersectEntity = "cmc_tripactivity_contact",
                RelationshipType = XrmFakedRelationship.enmFakeRelationshipType.ManyToMany,
                Entity1LogicalName = cmcTripActivity.LogicalName,
                Entity1Attribute = "cmc_tripactivityid",
                Entity2LogicalName = contact.LogicalName,
                Entity2Attribute = "contactid"
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
            var mockServiceProvider = InitializeMockService(xrmFakedContext, null, Operation.Disassociate);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);

            #endregion

            #region ARRANGE

            var mockLogger = new Mock<ILogger>();
            AddInputParameters(mockServiceProvider, "Target", cmcTrip.ToEntityReference());
            AddInputParameters(mockServiceProvider, "Relationship", new Relationship("cmc_trip_systemuser"));
            AddInputParameters(mockServiceProvider, "RelatedEntities", new EntityReferenceCollection(new List<EntityReference>() { owner.ToEntityReference() }));

            //Mock the IBingMap
            var mockBingServices = InitializeBingMapMockService();
            var mockLanguageService = new LanguageService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
            var mockTripActivityService = new Lifecycle.TripActivityService(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService(), mockBingServices.Object, mockLanguageService);
            #endregion

            #region ASSERT

            Assert.ThrowsException<InvalidPluginExecutionException>(() => mockTripActivityService.AssociateDisassociateStaffmembers(mockExecutionContext.Object));

            #endregion
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
        private UserSettings PreparingUserSetiing(Entity userInstance)
        {
            var userSettingInstance = new UserSettings()
            {
                SystemUserId = userInstance.Id,
                UILanguageId = 1033
            };
            return userSettingInstance;
        }

        #endregion DATA PREPARATION

    }
}
