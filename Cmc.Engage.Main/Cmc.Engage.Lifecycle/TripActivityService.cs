using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cmc.Core.Xrm.ServerExtension.Core;
using Cmc.Core.Xrm.ServerExtension.Logging;
using Cmc.Engage.Common;
using Cmc.Engage.Models;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using Newtonsoft.Json;
using IExecutionContext = Cmc.Core.Xrm.ServerExtension.Core.IExecutionContext;

namespace Cmc.Engage.Lifecycle
{
    /// <summary>
    /// trip activity service.
    /// </summary>
    public class TripActivityService : ITripActivityService
    {
        private IOrganizationService _orgService;
        private readonly ILogger _logger;
        private readonly IBingMapService _bingMapService;
        private ILanguageService _languageService;

        /// <summary>
        /// constructor for the trip activity service.
        /// </summary>
        /// <param name="organizationService">organization service</param>
        /// <param name="logger">logger</param>
        /// <param name="bingMapService">bing map service.</param>
        /// <param name="languageService">Langauge service</param>
        public TripActivityService(ILogger logger, IOrganizationService organizationService,
            IBingMapService bingMapService, ILanguageService languageService)
        {
            _logger = logger;
            _orgService = organizationService;
            _bingMapService = bingMapService;
            _languageService = languageService;
        }

        /// <summary>
        /// to update the latitude and longitude for give address in trip activity update or create.
        /// </summary>
        public void TripActivityUpdateLatitudeLongitude(
            Core.Xrm.ServerExtension.Core.IExecutionContext executionContext)
        {
            _logger.Info($"Entered into TripActivityUpdateLatitudeLongitude");

            var serviceProvider = executionContext.XrmServiceProvider;
            var pluginContext = serviceProvider.GetPluginExecutionContext();
            cmc_tripactivity targeTripactivity = null;
            var updateTripCoordinates = false;
            _logger.Info(
                $"TripActivityUpdateLatitudeLongitude : Message type is :{pluginContext.MessageName.ToLower()}");

            Guid tripid = Guid.Empty;
            if (pluginContext.MessageName.ToLower() == "create")
            {
                targeTripactivity = pluginContext.GetTargetEntity<cmc_tripactivity>();
                updateTripCoordinates = true;

                tripid = targeTripactivity.cmc_trip.Id;

                AutoPopulateTripActivityNameCreate(targeTripactivity);

                _logger.Info($"Started associating owner to trip :{targeTripactivity.cmc_trip.Name}");
                // ower to be added as part of the trip
                AssociateStaffmembersTripActivity(targeTripactivity.cmc_trip,
                    new Relationship("cmc_tripactivity_systemuser"),
                    new EntityReferenceCollection(new List<EntityReference>() { targeTripactivity.OwnerId }));

                if (!IsEventEntityAvilable("cmc_tripactivity") ||
                    !targeTripactivity.Attributes.Keys.Contains("cmc_linkedtoevent"))
                {
                    _logger.Info("Event related things not available ");
                }
                else
                {
                    AssociateEventsToTripActivity(targeTripactivity);
                }



            }
            else if (pluginContext.MessageName.ToLower() == "update")
            {

                _logger.Info($"Entered into update ");
                var preImage = pluginContext.GetPreEntityImage<cmc_tripactivity>("PreImage");
                targeTripactivity = pluginContext.GetTargetEntity<cmc_tripactivity>();
                if (preImage.cmc_location != targeTripactivity.cmc_location)
                {
                    updateTripCoordinates = true;
                }

                tripid = preImage.cmc_trip.Id;
                if (targeTripactivity.cmc_activitytype != null && (int)preImage.cmc_activitytype.Value !=
                    (int)targeTripactivity?.cmc_activitytype.Value)
                {
                    _logger.Info($"Before Entered to AutoPopulateTripActivityNameUpdate ");
                    AutoPopulateTripActivityNameUpdate(targeTripactivity, preImage);
                }
                else
                {
                    //On Chnage Of Appointment
                    if (targeTripactivity.cmc_LinkedToAppointment != null && preImage.cmc_LinkedToAppointment != null &&
                        preImage.cmc_LinkedToAppointment.Id != targeTripactivity.cmc_LinkedToAppointment.Id)
                    {
                        _logger.Info(
                            $"Before Entered to AutoPopulateTripActivityNameOnUpdateOfAppointmentOrEvent type of Appointment ");
                        AutoPopulateTripActivityNameOnUpdateOfAppointmentOrEvent(targeTripactivity, preImage,
                            "appointment");
                    }

                    //On Chnage Of Event
                    if (IsEventEntityAvilable("cmc_tripactivity"))
                    {
                        _logger.Info($"Event lookup avilable for cmc_tripactivity");
                        if (targeTripactivity.Attributes.Keys.Contains("cmc_linkedtoevent") && preImage.Attributes.Keys.Contains("cmc_linkedtoevent"))
                        {
                            EntityReference entityPreImageEvent = (EntityReference)preImage["cmc_linkedtoevent"];
                            EntityReference entityTargetEvent = (EntityReference)targeTripactivity["cmc_linkedtoevent"];
                            if (entityPreImageEvent != null && entityTargetEvent != null &&
                                entityPreImageEvent.Id != entityTargetEvent.Id)
                            {
                                _logger.Info(
                                    $"Before Entered to AutoPopulateTripActivityNameOnUpdateOfAppointmentOrEvent type of Event ");
                                AutoPopulateTripActivityNameOnUpdateOfAppointmentOrEvent(targeTripactivity, preImage,
                                    "event");
                            }
                        }
                    }
                }


                // ower to be added as part of the trip

                if (targeTripactivity.OwnerId != null && preImage.OwnerId.Id != targeTripactivity.OwnerId.Id)
                {
                    _logger.Info($"Started change associating owner to trip :{preImage.cmc_trip.Name}");

                    AssociateStaffmembersTripActivity(preImage.cmc_trip,
                        new Relationship("cmc_tripactivity_systemuser"),
                        new EntityReferenceCollection(new List<EntityReference>() { targeTripactivity.OwnerId }));
                }
            }
            else if (pluginContext.MessageName.ToLower() == "delete")
            {
                targeTripactivity = pluginContext.GetPreEntityImage<cmc_tripactivity>("PreImage");
                tripid = targeTripactivity.cmc_trip.Id;
            }

            // initialise the up the values for cmc trip instance.
            var trip = new Entity("cmc_trip", tripid)
            {
                Attributes =
                {
                    ["cmc_estexpense"] = new Money(0),
                    ["cmc_actexpense"] = new Money(0)
                }
            };
            // update the trip with latest attribute values.
            _orgService.Update(trip);

            // Refreshing cmc_estexpense of trip activity
            _logger.Info($"Entered into TripUpdateRollupFields calculating estimated expense {tripid}");
            CalculateRollupFieldRequest crfreq_tripestexp = new CalculateRollupFieldRequest
            {
                Target = new EntityReference("cmc_trip", tripid),
                FieldName = "cmc_estexpense"
            };
            CalculateRollupFieldResponse crfresp_tripestexp =
                (CalculateRollupFieldResponse)_orgService.Execute(crfreq_tripestexp);
            if (crfresp_tripestexp != null && crfresp_tripestexp.Entity != null)
            {
                var tripinstance = crfresp_tripestexp.Entity;
                var latestestexpval = tripinstance.GetAttributeValue<Money>("cmc_estexpense").Value;
                _logger.Info($"TripUpdateRollupFields : updated est expense is :{latestestexpval}");

            }


            // Refreshing cmc_actexpense of trip activity
            _logger.Info($"Entered into TripUpdateRollupFields calculating actual expense");
            CalculateRollupFieldRequest crfreq_tripactexp = new CalculateRollupFieldRequest
            {
                Target = new EntityReference("cmc_trip", tripid),
                FieldName = "cmc_actexpense"
            };
            CalculateRollupFieldResponse crfresp_tripactexp =
                (CalculateRollupFieldResponse)_orgService.Execute(crfreq_tripactexp);
            if (crfresp_tripactexp != null && crfresp_tripactexp.Entity != null)
            {
                var tripinstance = crfresp_tripactexp.Entity;
                var latestactexpval = tripinstance.GetAttributeValue<Money>("cmc_actexpense").Value;
                _logger.Info($"TripUpdateRollupFields : updated act expense is :{latestactexpval}");

            }


            if (targeTripactivity == null || string.IsNullOrEmpty(targeTripactivity.cmc_location))
            {
                _logger.Error($"Either Trip Activity Entity or Location is Null.");
                return;
            }


            #region Get coordinates for the location and update the longitude and latitude.

            if (updateTripCoordinates)
            {
                // get the details for the address.
                var coordinatesFromAddress = _bingMapService.GetCoordinatesFromAddress(targeTripactivity.cmc_location);
                _logger.Info(
                    $"Completed updating the TripActivity Update Latitude {coordinatesFromAddress?.Latitude} Longitude {coordinatesFromAddress?.Longitude} for address of {targeTripactivity?.cmc_location}");

                targeTripactivity.cmc_tripactivitylatitude = coordinatesFromAddress?.Latitude;
                targeTripactivity.cmc_tripactivitylongitude = coordinatesFromAddress?.Longitude;

                //_orgService.Update(targeTripactivity);
            }

            #endregion

        }

        /// <summary>
        /// to associate and diassociate the staff members when changes happened at trip activity
        /// </summary>
        /// <param name="context">plugin context</param>
        public void AssociateDisassociateStaffmembers(IExecutionContext context)
        {
            _logger.Info($"Entered into AssociateDissociateStaffmembers");

            var serviceProvider = context.XrmServiceProvider;
            var pluginContext = serviceProvider.GetPluginExecutionContext();
            _logger.Info($"AssociateDissociateStaffmembers : Message type is :{pluginContext.MessageName.ToLower()}");

            if (pluginContext.InputParameters.Contains("Target"))
            {
                var targetEntity = (EntityReference)pluginContext.InputParameters["Target"];
                Relationship relationship = null;
                EntityReferenceCollection relatedEntityReferenceCollection = null;
                _logger.Info(
                    $"AssociateDissociateStaffmembers : Target Id is :{targetEntity.Id} and schema name is {targetEntity.LogicalName}");
                // get the relationship schemaname
                if (pluginContext.InputParameters.Contains("Relationship"))
                {
                    relationship = (Relationship)pluginContext.InputParameters["Relationship"];
                    _logger.Info($"AssociateDissociateStaffmembers : Relationship name is :{relationship.SchemaName}");
                }

                // related entities collection
                if (pluginContext.InputParameters.Contains("RelatedEntities"))
                {
                    relatedEntityReferenceCollection =
                        (EntityReferenceCollection)pluginContext.InputParameters["RelatedEntities"];
                    _logger.Info(
                        $"AssociateDissociateStaffmembers : Related Entities Count :{relatedEntityReferenceCollection.Count} and related instance {relatedEntityReferenceCollection.FirstOrDefault()?.LogicalName}");
                }

                //type of the association
                switch (pluginContext.MessageName.ToLower())
                {
                    case "associate":
                        if (targetEntity.LogicalName == cmc_tripactivity.EntityLogicalName)
                        {
                            AssociateStaffmembersTripActivity(targetEntity, relationship,
                                relatedEntityReferenceCollection);
                        }

                        break;
                    case "disassociate":
                        if (targetEntity.LogicalName == cmc_tripactivity.EntityLogicalName)
                        {
                            DiassociateStaffmembersTripActivity(targetEntity, relationship,
                                relatedEntityReferenceCollection);
                        }
                        else if (targetEntity.LogicalName == cmc_trip.EntityLogicalName)
                        {
                            DiassociateStaffmembersTrip(targetEntity, relationship, relatedEntityReferenceCollection);
                        }

                        break;
                }
            }

            _logger.Info($" Completed AssociateDissociateStaffmembers");
        }




        /// <summary>
        /// Diassociate the trip activity staff members
        /// </summary>
        /// <param name="targetEntity">target Entity</param>
        /// <param name="relationship">Relationship name</param>
        /// <param name="relatedEntityReferenceCollection">associated related entities</param>
        private void DiassociateStaffmembersTrip(EntityReference targetEntity, Relationship relationship,
            EntityReferenceCollection relatedEntityReferenceCollection)
        {
            _logger.Info($" Entered into DiassociateStaffmembersTripActivity");

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
                                <filter>
                                <condition attribute='cmc_tripid' operator='eq' value='{targetEntity.Id}'/>
                                </filter>
                              </entity>
                            </fetch>";

            var lstCmcTrip = _orgService.RetrieveMultiple(new FetchExpression(fetchXml))?.Entities?.Select(x => x.ToEntity<cmc_trip>()).ToList();

            DeAssociateTripActivitiesMembers(targetEntity, lstCmcTrip, relationship, relatedEntityReferenceCollection);
            _logger.Info($" Completed DiassociateStaffmembersTripActivity");
        }


        private void DeAssociateTripActivitiesMembers(EntityReference target, List<cmc_trip> lstCmcTrip,
            Relationship relationship, EntityReferenceCollection relatedEntityReferenceCollection)
        {
            _logger.Info($" Entered into DeAssociateTripActivitiesMembers");

            var cmcTrip = lstCmcTrip?.FirstOrDefault();
            _logger.Info($" cmc_Trip name is {cmcTrip?.cmc_tripname} and record count is {lstCmcTrip?.Count}");

            if (cmcTrip != null)
            {
                var tripActivityfetchXml = $@"<fetch>
                                                <entity name='cmc_tripactivity'>
                                                <attribute name='ownerid' />
                                                <filter>
                                                    <condition attribute='cmc_trip' operator='eq' value='{
                        cmcTrip.Id
                    }'/>
                                                </filter>
                                                <link-entity name='cmc_tripactivity_contact' from='cmc_tripactivityid' to='cmc_tripactivityid' link-type='outer' alias='student' intersect='true'>
                                                    <attribute name='contactid' />
                                                </link-entity>
                                                <link-entity name='cmc_tripactivity_systemuser' from='cmc_tripactivityid' to='cmc_tripactivityid' link-type='outer' alias='staff' intersect='true'>
                                                    <attribute name='systemuserid' />
                                                </link-entity>
                                                </entity>
                                            </fetch>";

                var lstCmcTripActivity = _orgService.RetrieveMultiple(new FetchExpression(tripActivityfetchXml))?.Entities?.Select(x => x.ToEntity<cmc_tripactivity>()).ToList();
                _logger.Info($" cmc Trip activity record count is {lstCmcTripActivity?.Count}");
                var isSafeDelete = true;
                // check the relationship schema name
                IEnumerable<Guid> lstTripActivitystudents;
                switch (relationship.SchemaName)
                {
                    case "cmc_tripactivity_contact":

                        var lstTripstudents =
                            lstCmcTrip.Select(r => r.GetAliasedAttributeValue<Guid>("student.contactid"));
                        if ((bool)lstTripstudents?.Any(c =>
                           relatedEntityReferenceCollection.Select(r => r.Id).Contains(c)))
                        {
                            _logger.Info(
                                $" Already Student related entity associate as part of trip {JsonConvert.SerializeObject(relatedEntityReferenceCollection)}");
                            // verify any other trip activity exists have the same record.
                            if (lstCmcTripActivity != null)
                            {
                                // apart from target entity, any other trip depeNdent
                                lstTripActivitystudents = lstCmcTripActivity.Where(r => r.Id != target.Id)
                                    .Select(r => r.GetAliasedAttributeValue<Guid>("student.contactid"));
                                if ((bool)lstTripActivitystudents?.Any(c =>
                                   relatedEntityReferenceCollection.Select(r => r.Id).Contains(c)))
                                {
                                    _logger.Info("identified the other trip activity uses same volunter.");
                                    isSafeDelete = false;
                                }

                            }

                            if (isSafeDelete)
                            {
                                // Create an object that defines the relationship between the contact and trip.
                                Relationship triprelationship = new Relationship("cmc_trip_contact");
                                DiassociateRelatedEntityToEntity(cmcTrip, triprelationship,
                                    relatedEntityReferenceCollection);
                            }

                        }
                        else
                        {
                            _logger.Info($" No associated related entity");
                        }

                        break;

                    case "cmc_tripactivity_systemuser":
                        var lstTripstaff =
                            lstCmcTrip.Select(r => r.GetAliasedAttributeValue<Guid>("staff.systemuserid"));
                        if ((bool)lstTripstaff?.Any(
                            c => relatedEntityReferenceCollection.Select(r => r.Id).Contains(c)))
                        {
                            _logger.Info(
                                $" Already System User related entity associate as part of trip {JsonConvert.SerializeObject(relatedEntityReferenceCollection)}");

                            _logger.Info(
                                $" Already Student related entity associate as part of trip {JsonConvert.SerializeObject(relatedEntityReferenceCollection)}");
                            // verify any other trip activity exists have the same record.
                            if (lstCmcTripActivity != null)
                            {
                                // apart from target entity, any other trip deepedent
                                lstTripActivitystudents = lstCmcTripActivity.Where(r => r.Id != target.Id)
                                    .Select(r => r.GetAliasedAttributeValue<Guid>("staff.systemuserid"));
                                if ((bool)lstTripActivitystudents?.Any(c =>
                                   relatedEntityReferenceCollection.Select(r => r.Id).Contains(c)))
                                {
                                    _logger.Info("identified the other trip activity uses same staff.");
                                    isSafeDelete = false;
                                }

                                // apart from target entity, any other trip depeNdent
                                lstTripActivitystudents = lstCmcTripActivity.Where(r => r.Id != target.Id)
                                    .Select(r => r.OwnerId.Id);
                                _logger.Info($"trip owners are {JsonConvert.SerializeObject(lstTripActivitystudents)}");
                                if ((bool)lstTripActivitystudents?.Any(c =>
                                   relatedEntityReferenceCollection.Select(r => r.Id).Contains(c)))
                                {
                                    _logger.Info("identified the other trip activity uses owner.");
                                    isSafeDelete = false;
                                }
                            }

                            if (isSafeDelete)
                            {
                                // Create an object that defines the relationship between the contact and trip.
                                Relationship triprelationship = new Relationship("cmc_trip_systemuser");
                                DiassociateRelatedEntityToEntity(cmcTrip, triprelationship,
                                    relatedEntityReferenceCollection);
                            }
                        }
                        else
                        {
                            _logger.Info($" No Associating related entity collection");
                        }

                        break;
                    case "cmc_trip_contact":
                        // verify any other trip activity exists have the same record.
                        if (lstCmcTripActivity != null)
                        {
                            lstTripActivitystudents = lstCmcTripActivity.Select(r =>
                                r.GetAliasedAttributeValue<Guid>("student.contactid"));
                            if ((bool)lstTripActivitystudents?.Any(c =>
                               relatedEntityReferenceCollection.Select(r => r.Id).Contains(c)))
                            {
                                _logger.Info("identified the other trip activity uses same volunter.");
                                // error string for more than one dependecy
                                throw new InvalidPluginExecutionException(_languageService.Get("Trip_Delete_Student"));
                            }

                        }

                        break;
                    case "cmc_trip_systemuser":

                        lstTripActivitystudents =
                            lstCmcTripActivity?.Select(r => r.GetAliasedAttributeValue<Guid>("staff.systemuserid"));
                        if (lstTripActivitystudents != null && (bool)lstTripActivitystudents?.Any(c =>
                               relatedEntityReferenceCollection.Select(r => r.Id).Contains(c)))
                        {
                            _logger.Info("identified the other trip activity uses same staff.");
                            // error string for more than one dependecy
                            throw new InvalidPluginExecutionException(_languageService.Get("Trip_Delete_Staff"));
                        }

                        lstTripActivitystudents = lstCmcTripActivity?.Select(r => r.OwnerId.Id);
                        if ((bool)lstTripActivitystudents?.Any(c =>
                           relatedEntityReferenceCollection.Select(r => r.Id).Contains(c)))
                        {
                            _logger.Info("identified the other trip activity uses owner.");
                            // error string for more than one dependecy
                            throw new InvalidPluginExecutionException(_languageService.Get("Trip_Delete_Owner"));
                        }

                        break;
                }
            }

            _logger.Info($"completed DeAssociateTripActivitiesMembers");
        }


        /// <summary>
        /// Diassociate the trip activity staff members
        /// </summary>
        /// <param name="targetEntity">target Entity</param>
        /// <param name="relationship">Relationship name</param>
        /// <param name="relatedEntityReferenceCollection">associated related entities</param>
        private void DiassociateStaffmembersTripActivity(EntityReference targetEntity, Relationship relationship,
            EntityReferenceCollection relatedEntityReferenceCollection)
        {
            _logger.Info($" Entered into DiassociateStaffmembersTripActivity");

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
                                    <condition attribute='cmc_tripactivityid' operator='eq' value='{targetEntity.Id}'/>
                                  </filter>
                                </link-entity>
                              </entity>
                            </fetch>";

            var lstCmcTrip = _orgService.RetrieveMultiple(new FetchExpression(fetchXml))?.Entities?.Select(x => x.ToEntity<cmc_trip>()).ToList();
            DeAssociateTripActivitiesMembers(targetEntity, lstCmcTrip, relationship, relatedEntityReferenceCollection);

            _logger.Info($" Completed DiassociateStaffmembersTripActivity");
        }

        /// <summary>
        /// diassociate the relationship to main entity
        /// </summary>
        /// <param name="relatedEntity">related Entity to remove the association</param>
        /// <param name="relationship">relationship detail</param>
        /// <param name="relatedEntityReferenceCollection">related entites collection</param>
        private void DiassociateRelatedEntityToEntity(Entity relatedEntity, Relationship relationship,
            EntityReferenceCollection relatedEntityReferenceCollection)
        {
            _logger.Info($" Associating related entity collection : {JsonConvert.SerializeObject(relationship)}");
            //Associate the contact with the 3 accounts.
            if (relatedEntity != null)
            {
                _orgService.Disassociate(relatedEntity.LogicalName, relatedEntity.Id, relationship,
                    relatedEntityReferenceCollection);
            }
        }

        /// <summary>
        /// Associate the relationship to main entity
        /// </summary>
        /// <param name="relatedEntity">related Entity to add the association</param>
        /// <param name="relationship">relationship detail</param>
        /// <param name="relatedEntityReferenceCollection">related entites collection</param>
        private void AssociateRelatedEntityToEntity(Entity relatedEntity, Relationship relationship,
            EntityReferenceCollection relatedEntityReferenceCollection)
        {
            _logger.Info($" Associating related entity collection : {JsonConvert.SerializeObject(relationship)}");
            //Associate the contact with the 3 accounts.
            if (relatedEntity != null)
            {
                _orgService.Associate(relatedEntity.LogicalName, relatedEntity.Id, relationship,
                    relatedEntityReferenceCollection);
            }
        }

        /// <summary>
        /// Associate the trip activity staff members
        /// </summary>
        /// <param name="targetEntity">target Entity</param>
        /// <param name="relationship">Relationship name</param>
        /// <param name="relatedEntityReferenceCollection">associated related entities</param>
        private void AssociateStaffmembersTripActivity(EntityReference targetEntity, Relationship relationship,
            EntityReferenceCollection relatedEntityReferenceCollection)
        {
            _logger.Info($" Entered into AssociateStaffmembersTripActivity");
            string fetchXml;
            if (targetEntity.LogicalName == cmc_trip.EntityLogicalName)
            {
                fetchXml = $@"
                            <fetch>
                              <entity name='cmc_trip'>
                                <all-attributes />
                                <link-entity name='cmc_trip_contact' from='cmc_tripid' to='cmc_tripid' link-type='outer' intersect='true' alias='student' visible='true'>
                                  <attribute name='contactid' />
                                </link-entity>
                                <link-entity name='cmc_trip_systemuser' from='cmc_tripid' to='cmc_tripid' link-type='outer' alias='staff' visible='true'>
                                  <attribute name='systemuserid' />
                                </link-entity>
                                <filter>
                                <condition attribute='cmc_tripid' operator='eq' value='{targetEntity.Id}'/>
                                </filter>
                              </entity>
                            </fetch>";
            }
            else
            {

                fetchXml = $@"
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
                                    <condition attribute='cmc_tripactivityid' operator='eq' value='{targetEntity.Id}'/>
                                  </filter>
                                </link-entity>
                              </entity>
                            </fetch>";
            }

            var lstCmcTrip = _orgService.RetrieveMultiple(new FetchExpression(fetchXml))?.Entities?.Select(r => r.ToEntity<cmc_trip>()).ToList();
            var cmcTrip = lstCmcTrip?.FirstOrDefault();
            _logger.Info($" cmc_Trip name is {cmcTrip?.cmc_tripname} and record count is {lstCmcTrip?.Count()}");

            // check the relationship schema name
            switch (relationship.SchemaName)
            {
                case "cmc_tripactivity_contact":

                    var lstTripstudents =
                        lstCmcTrip?.Select(r => r.GetAliasedAttributeValue<Guid>("student.contactid"));
                    if (lstTripstudents != null && (bool)lstTripstudents?.Any(c =>
                           relatedEntityReferenceCollection.Select(r => r.Id).Contains(c)))
                    {
                        _logger.Info(
                            $" Already Student related entity associate as part of trip {JsonConvert.SerializeObject(relatedEntityReferenceCollection)}");
                    }
                    else
                    {
                        _logger.Info($" Entered into Associating related entity collection");
                        // Create an object that defines the relationship between the contact and account.
                        Relationship triprelationship = new Relationship("cmc_trip_contact");
                        AssociateRelatedEntityToEntity(cmcTrip, triprelationship, relatedEntityReferenceCollection);
                    }

                    break;

                case "cmc_tripactivity_systemuser":
                    var lstTripstaff = lstCmcTrip?.Select(r => r.GetAliasedAttributeValue<Guid>("staff.systemuserid"));
                    if (lstTripstaff != null && (bool)lstTripstaff?.Any(c =>
                           relatedEntityReferenceCollection.Select(r => r.Id).Contains(c)))
                    {
                        _logger.Info(
                            $" Already System User related entity associate as part of trip {JsonConvert.SerializeObject(relatedEntityReferenceCollection)}");
                    }
                    else
                    {
                        _logger.Info($" Entered into Associating related entity collection");
                        // Create an object that defines the relationship between the contact and account.
                        Relationship triprelationship = new Relationship("cmc_trip_systemuser");
                        AssociateRelatedEntityToEntity(cmcTrip, triprelationship, relatedEntityReferenceCollection);
                    }

                    break;
            }

            _logger.Info($" Completed AssociateStaffmembersTripActivity");
        }


        /// <summary>
        /// Auto Populate TripActivity Name For Create 
        /// </summary>
        /// <param name="targetTripActivity">Target Entity</param>
        private void AutoPopulateTripActivityNameCreate(cmc_tripactivity targetTripActivity)
        {
            _logger.Info($"Entered In AutoPopulateTripActivityNameCreate");
            cmc_trip trip = _orgService.Retrieve<cmc_trip>(targetTripActivity.cmc_trip, new ColumnSet(true));
            if ((int)targetTripActivity.cmc_activitytype.Value == (int)cmc_tripactivity_cmc_activitytype.Appointment)
            {
                _logger.Info($"Entered Into Appoinment");
                Appointment appointment = _orgService.Retrieve<Appointment>(targetTripActivity.cmc_LinkedToAppointment,
                    new ColumnSet(true));

                targetTripActivity.cmc_name = appointment.Subject + " - " + trip.cmc_tripname;
                _logger.Info($"Auto Populated Name Is: {appointment.Subject + " - " + trip.cmc_tripname}");
            }
            ///
            ///This code should we move to Marketing Plugin
            ///
            else if (targetTripActivity.cmc_activitytype.Value == cmc_tripactivity_cmc_activitytype.Event)
            {
                if (IsEventEntityAvilable("cmc_tripactivity"))
                {
                    _logger.Info($"Entered Into Event");
                    EntityReference entityReference = targetTripActivity["cmc_linkedtoevent"] != null
                        ? (EntityReference)targetTripActivity["cmc_linkedtoevent"]
                        : null;
                    if (entityReference != null)
                    {
                        Entity eventEntity = _orgService.Retrieve<Entity>(entityReference, new ColumnSet(true));
                        targetTripActivity.cmc_name = eventEntity["msevtmgt_name"] + " - " + trip.cmc_tripname;
                        _logger.Info(
                            $"Auto Populated Name Is: {eventEntity["msevtmgt_name"] + " - " + trip.cmc_tripname}");
                    }
                }
                else
                {
                    _logger.Info($"Event Entity Not Found");
                }
            }
            else
            {
                _logger.Info($"Entered Into Other");
                if (targetTripActivity.cmc_name == null || targetTripActivity.cmc_name.Trim() == String.Empty)
                {
                    targetTripActivity.cmc_name = trip.cmc_tripname;
                }

                _logger.Info($"Auto Populated Name Is: {trip.cmc_tripname}");
            }

        }

        /// <summary>
        /// Auto Populate TripActivity Name For Update
        /// </summary>
        /// <param name="targetTripActivity">Target Entity </param>
        /// <param name="preImage"> Pre Image Entity </param>
        private void AutoPopulateTripActivityNameUpdate(cmc_tripactivity targetTripActivity, cmc_tripactivity preImage)
        {
            _logger.Info($"Entered In AutoPopulateTripActivityNameUpdate");
            cmc_trip trip = _orgService.Retrieve<cmc_trip>(preImage.cmc_trip, new ColumnSet(true));
            if ((int)targetTripActivity.cmc_activitytype.Value == (int)cmc_tripactivity_cmc_activitytype.Appointment)
            {
                _logger.Info($"Entered Into Appoinment");
                Appointment appointment = _orgService.Retrieve<Appointment>(targetTripActivity.cmc_LinkedToAppointment,
                    new ColumnSet(true));

                targetTripActivity.cmc_name = appointment.Subject + " - " + trip.cmc_tripname;
                _logger.Info($"Auto Populated Name Is: {appointment.Subject + " - " + trip.cmc_tripname}");
            }
            //
            // This code should we move to Marketing Plugin
            //
            else if (targetTripActivity.cmc_activitytype.Value == cmc_tripactivity_cmc_activitytype.Event)
            {
                if (IsEventEntityAvilable("cmc_tripactivity"))
                {
                    _logger.Info($"Entered Into Event");
                    EntityReference entityReference = targetTripActivity["cmc_linkedtoevent"] != null
                        ? (EntityReference)targetTripActivity["cmc_linkedtoevent"]
                        : null;
                    if (entityReference != null)
                    {
                        Entity eventEntity = _orgService.Retrieve<Entity>(entityReference, new ColumnSet(true));
                        targetTripActivity.cmc_name = eventEntity["msevtmgt_name"] + " - " + trip.cmc_tripname;
                        _logger.Info(
                            $"Auto Populated Name Is: {eventEntity["msevtmgt_name"] + " - " + trip.cmc_tripname}");
                    }
                }
            }
            else
            {
                _logger.Info($"Entered Into Other");
                if (targetTripActivity.cmc_name == null || targetTripActivity.cmc_name.Trim() == String.Empty)
                {
                    targetTripActivity.cmc_name = trip.cmc_tripname;
                }

                _logger.Info($"Auto Populated Name Is: {trip.cmc_tripname}");
            }

        }

        /// <summary>
        /// Auto Populate TripActivity Name On Update of Appointment Or Event
        /// </summary>
        /// <param name="targetTripActivity">Target Entity </param>
        /// <param name="preImage"> Pre Image Entity </param>
        private void AutoPopulateTripActivityNameOnUpdateOfAppointmentOrEvent(cmc_tripactivity targetTripActivity,
            cmc_tripactivity preImage, string updateType)
        {
            _logger.Info($"Entered In AutoPopulateTripActivityNameOnUpdateOfAppointmentOrEvent type of - {updateType}");
            cmc_trip trip = _orgService.Retrieve<cmc_trip>(preImage.cmc_trip, new ColumnSet(true));
            if (updateType == "appointment")
            {
                _logger.Info($"Entered Into Appoinment");
                Appointment appointment = _orgService.Retrieve<Appointment>(targetTripActivity.cmc_LinkedToAppointment,
                    new ColumnSet(true));

                targetTripActivity.cmc_name = appointment.Subject + " - " + trip.cmc_tripname;
                _logger.Info($"Auto Populated Name Is: {appointment.Subject + " - " + trip.cmc_tripname}");
            }
            else if (updateType == "event")
            {
                _logger.Info($"Entered Into Event");
                EntityReference entityReference = targetTripActivity["cmc_linkedtoevent"] != null
                    ? (EntityReference)targetTripActivity["cmc_linkedtoevent"]
                    : null;
                if (entityReference != null)
                {
                    Entity eventEntity = _orgService.Retrieve<Entity>(entityReference, new ColumnSet(true));
                    targetTripActivity.cmc_name = eventEntity["msevtmgt_name"] + " - " + trip.cmc_tripname;
                    _logger.Info($"Auto Populated Name Is: {eventEntity["msevtmgt_name"] + " - " + trip.cmc_tripname}");
                }

            }
        }

        /// <summary>
        /// It will check the given entity is there or not in orgnization 
        /// </summary>
        /// <param name="entityKey">Entity name </param>
        /// <returns></returns>
        private bool IsEventEntityAvilable(string entityKey)
        {
            _logger.Info($"Entered Into IsEventEntityAvilable");
            bool? isEventAvilable = false;
            var req = new RetrieveEntityRequest()
            {
                LogicalName = entityKey,
                RetrieveAsIfPublished = true,
                EntityFilters = EntityFilters.Entity | EntityFilters.Attributes
            };
            try
            {
                _logger.Info($"Entered Into IsEventEntityAvilable try block ");
                RetrieveEntityResponse entityMetadataResponse = ((RetrieveEntityResponse)_orgService.Execute(req));
                if (entityMetadataResponse != null)
                {
                    _logger.Info($"Entered Into IsEventEntityAvilable before filter ");
                    isEventAvilable = entityMetadataResponse.EntityMetadata?.Attributes?.Any(r =>
                        r.AttributeType == AttributeTypeCode.Lookup
                        && ((LookupAttributeMetadata)r).Targets.Contains("msevtmgt_event"));
                    _logger.Info($"Entered Into IsEventEntityAvilable after filter ");
                }

                return isEventAvilable ?? false;
            }
            catch (Exception ex)
            {
                _logger.Trace($"Returning null; Error retrieving entity metadata for {entityKey}: {ex.Message}");
                return false;
            }
        }
        /// <summary>
        /// Get event details from event id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private Entity GetEventDetails(Guid id)
        {
            _logger.Info("Entered Into GetEventDetails");
            var fetch =
                $@"<fetch>
                  <entity name='msevtmgt_event' >
                    <attribute name='msevtmgt_eventenddate' />
                    <attribute name='msevtmgt_eventtimezone' />
                    <attribute name='msevtmgt_eventstartdate' />
                    <filter type='and' >
                      <condition attribute='msevtmgt_eventid' operator='eq' value='{id}' />
                    </filter>
                   </entity>
                </fetch>";
            var data = _orgService.RetrieveMultiple(new FetchExpression(fetch));
            _logger.Info("Event Count " + data.Entities.Count);
            return data.Entities.Count <= 0 ? null : data.Entities.FirstOrDefault();
        }
        /// <summary>
        /// Associate Event locationstartdatetime,locationenddatetime,locationtimezone To TripActivity
        /// </summary>
        /// <param name="tripActivity"></param>
        private void AssociateEventsToTripActivity(cmc_tripactivity tripActivity)
        {
            _logger.Info("Entered Into AssociateEventsToTripActivity");
            var entityTargetEvent = (EntityReference)tripActivity["cmc_linkedtoevent"];
            if (entityTargetEvent == null)
            {
                _logger.Info("Linked to event field is not exist");
            }
            else
            {
                var eventDetails = GetEventDetails(entityTargetEvent.Id);
                if (eventDetails != null)
                {
                    tripActivity["cmc_locationstartdatetime"] = eventDetails["msevtmgt_eventstartdate"];
                    _logger.Info($"New Trip activity locations ScheduledStart { tripActivity["cmc_locationstartdatetime"]}");

                    tripActivity["cmc_locationenddatetime"] = eventDetails["msevtmgt_eventenddate"];
                    _logger.Info($"New Trip locations ScheduledEnd {tripActivity["cmc_locationenddatetime"]}");

                    tripActivity["cmc_locationtimezone"] = eventDetails["msevtmgt_eventtimezone"];
                    _logger.Info($"New Trip  location Timezone {tripActivity["cmc_locationtimezone"]} ");
                }

            }



        }

        /// <summary>
        /// Send Email to corresponding staff memebers and volunteers with attached ics file
        /// </summary>
        /// <param name="emailId"></param>
        /// <param name="tripActivityId"></param>
        /// <param name="startDateTime"></param>
        /// <param name="endDateTime"></param>
        /// <param name="location"></param>
        /// <param name="fileName"></param>
        /// <param name="subject"></param>
        /// <param name="description"></param>
        public void AttachTripActivityICalFileActivity(EntityReference emailId, EntityReference tripActivityId, DateTime startDateTime,
            DateTime endDateTime, string location, string fileName, string subject, string description)
        {
            _logger.Info("Entered Into AttachTripActivityICalFileActivity");

            var emailDetails = _orgService.Retrieve<Email>(emailId,
                new ColumnSet("torecipients", "from", "description"));

            if (tripActivityId == null)
            {
                _logger.Info("Trip ActivityId is null");
                return;
            }
            var tripActivityDetails = _orgService.Retrieve<cmc_tripactivity>(tripActivityId,
                new ColumnSet("cmc_emailsequence"));

            var emailsequence = tripActivityDetails.cmc_emailsequence;
            if (emailsequence == null)
            {
                //First Time
                emailsequence = 0;
            }
            else
            {
                emailsequence += 1;
            }
            _logger.Info("Number of Sequences of email " + emailsequence);
            _logger.Trace($"Email Description {JsonConvert.SerializeObject(emailDetails)}");
            ActivityParty emailFromDetails;
            var emailToDetails = new List<ActivityParty>();
            if (emailDetails.From?.Any() == true)
            {
                emailFromDetails = emailDetails.From.First();
            }
            else
            {
                _logger.Info("There is no organizer for trip activity");
                return;
            }


            var assosiatedStaffMembers = GetAssosiatedStaffMembers(tripActivityId.Id);
            if (assosiatedStaffMembers == null)
            {
                _logger.Info("There is no staff Member assosiated with this trip activity " + tripActivityId.Id);
            }

            var assosiatedvolunteers = GetAssosiatedvolunteers(tripActivityId.Id);
            if (assosiatedvolunteers == null)
            {
                _logger.Info("There is no volunteers assosiated with this trip activity " + tripActivityId.Id);
            }

            if (assosiatedStaffMembers == null && assosiatedvolunteers == null)
            {
                _logger.Info("No volunteers and staff Members assosiated with this trip activity " + tripActivityId.Id);
                return;
            }

            var assosiatedTripActivityMembers = new List<Tuple<Guid, string, string>>();
            if (assosiatedvolunteers?.Any() == true)
            {
                _logger.Info("Total number of volunteers assosiated with this trip activity " + assosiatedvolunteers.Count);
                foreach (var volunteer in assosiatedvolunteers)
                {
                    assosiatedTripActivityMembers.Add(Tuple.Create(volunteer.Id, volunteer.EMailAddress1, volunteer.FullName));
                    //adding emailid to recipient
                    emailToDetails.Add(new ActivityParty
                    {
                        PartyId = new EntityReference(Contact.EntityLogicalName, volunteer.Id)
                    });
                }
            }
            if (assosiatedStaffMembers?.Any() == true)
            {
                _logger.Info("Total number of StaffMembers assosiated with this trip activity " + assosiatedStaffMembers.Count);
                foreach (var staffMember in assosiatedStaffMembers)
                {
                    assosiatedTripActivityMembers.Add(Tuple.Create(staffMember.Id, staffMember.InternalEMailAddress, staffMember.FullName));
                    // adding emailid to recipient
                    emailToDetails.Add(new ActivityParty
                    {
                        PartyId = new EntityReference(SystemUser.EntityLogicalName, staffMember.Id)
                    });
                }
            }

            var fileString = BuildICalFile(startDateTime, endDateTime, location, subject, description, false, assosiatedTripActivityMembers, _logger, tripActivityId.Id, emailFromDetails, emailsequence.Value);
            _logger.Trace("Creating the Attachment.");

            if (string.IsNullOrEmpty(fileName))
            {
                fileName = "TripActivityAppointment.ics";
            }
            else
            {
                fileName = fileName + ".ics";
            }
            var mimeAttachment = new ActivityMimeAttachment
            {
                FileName = fileName,
                MimeType = "text/calendar",
                Body = fileString,
                ObjectId = emailId,
                ObjectTypeCode = emailId.LogicalName
            };
            _orgService.Create(mimeAttachment);
            if (emailToDetails.Any())
            {
                emailDetails.To = emailToDetails;
                emailDetails.DirectionCode = true;
                _orgService.Update(emailDetails);
            }
            _orgService.Execute(new SendEmailRequest
            {
                EmailId = emailId.Id,
                IssueSend = true
            });
            _orgService.Update(new cmc_tripactivity
            {
                Id = tripActivityId.Id,
                cmc_emailsequence = emailsequence
            });
            _logger.Trace("Email Sent");

        }
        /// <summary>
        /// Get Assosiated  StaffMembers for trip activity
        /// </summary>
        /// <param name="tripActivityId"></param>
        /// <returns></returns>
        private List<SystemUser> GetAssosiatedStaffMembers(Guid tripActivityId)
        {
            _logger.Info("Entered Into GetAssosiatedStaffMembers");
            var fetch = $@"<fetch>
                          <entity name='systemuser'>
                            <attribute name='internalemailaddress'/>
                            <attribute name='systemuserid'/>
                            <attribute name='fullname'/>
                            <link-entity name='cmc_tripactivity_systemuser' from='systemuserid' to='systemuserid' intersect='true'> 
                              <filter type='and'>
                                <condition attribute='cmc_tripactivityid' operator='eq' value='{ tripActivityId}'/>                
                              </filter>
                            </link-entity>
                          </entity>
                        </fetch>";
            var staffMembers = _orgService.RetrieveMultipleAll(fetch);
            var data = staffMembers.Entities.Count <= 0 ? null : staffMembers.Entities.Cast<SystemUser>().ToList();
            return data;
        }
        /// <summary>
        /// Get Assosiated volunteers for trip activity
        /// </summary>
        /// <param name="tripActivityId"></param>
        /// <returns></returns>
        private List<Contact> GetAssosiatedvolunteers(Guid tripActivityId)
        {
            _logger.Info("Entered Into GetAssosiatedvolunteers");
            var fetch = $@"<fetch>
                          <entity name='contact'>
                            <attribute name='emailaddress1'/>
                          <attribute name='contactid'/>
                          <attribute name='fullname'/>
                            <link-entity name='cmc_tripactivity_contact' from='contactid' to='contactid' intersect='true'>
                              <filter type='and' >
                               <condition attribute='cmc_tripactivityid' operator='eq' value='{ tripActivityId}'/>       
                              </filter>
                            </link-entity>
                          </entity>
                        </fetch>";
            var volunteers = _orgService.RetrieveMultipleAll(fetch);
            var data = volunteers.Entities.Count <= 0 ? null : volunteers.Entities.Cast<Contact>().ToList();
            return data;
        }

        private string BuildICalFile(DateTime startDateTime,
            DateTime endDateTime, string location, string subject, string description, bool isAllDayEvent, List<Tuple<Guid, string, string>> assosiatedTripActivityMembers, ILogger tracer, Guid tripActivityId, ActivityParty from, int emailsequence)
        {
            tracer.Trace("Building ICal file.");
            var organizer = $"ORGANIZER;{BuildCommonName(from.PartyId)}ROLE=REQ-PARTICIPANT:mailto:{from.AddressUsed}";
            var attendees = new List<string>();

            tracer.Trace("Adding attendees.");
            foreach (var record in assosiatedTripActivityMembers)
            {
                attendees.Add($"ATTENDEE;{BuildCommonName(record.Item3)}ROLE=REQ-PARTICIPANT:mailto:{record.Item2}");
            }

            // Timezone information we are setting to GMT/UTC +000 , because we are saving dates in the trip activities at UTC.
            // Timezone information showed up when open up the calendar.
            // This text must not have any leading whitespace on each line otherwise the ICS file will be invalid
            /*to display description in HTML format*/
            var calendar = $@"BEGIN:VCALENDAR
PRODID:-//Campus Management//Appointment Generation//EN
METHOD:REQUEST
VERSION:2.0
BEGIN:VTIMEZONE
TZID:UTC
BEGIN:STANDARD
TZOFFSETFROM:0000
TZOFFSETTO:0000
TZNAME:Coordinated Universal Time
END:STANDARD
END:VTIMEZONE
BEGIN:VEVENT
{string.Join(Environment.NewLine, attendees)}
X-ALT-DESC;FMTTYPE=text/html:{description}<span style='color: rgb(255, 102, 0); font - family: &quot; Segoe UI&quot;, &quot; Helvetica Neue&quot;, Helvetica, Arial, Verdana; font - size: x - small; '>**This meeting has been adjusted to reflect you current time zone.</span> 
DTEND{BuildDateTime(endDateTime, isAllDayEvent)}
DTSTAMP:{DateTime.UtcNow.ToString("yyyyMMddTHHmmssZ")}
DTSTART{BuildDateTime(startDateTime, isAllDayEvent)}
{organizer}
SEQUENCE:{emailsequence}
SUMMARY:{subject}
LOCATION:{location}
UID:{tripActivityId}
END:VEVENT
END:VCALENDAR";

            return Convert.ToBase64String(Encoding.UTF8.GetBytes(calendar));
        }

        private string BuildCommonName(string fullName)
        {
            return !string.IsNullOrEmpty(fullName)
                ? $"CN={fullName};"
                : null;
        }
        private string BuildDateTime(DateTime value, bool isAllDayEvent)
        {
            return isAllDayEvent
                ? $";VALUE=DATE:{value.ToUniversalTime().ToString("yyyyMMdd")}"
                : $":{value.ToUniversalTime().ToString("yyyyMMddTHHmmssZ")}";
        }
        private string BuildCommonName(EntityReference partyId)
        {
            return partyId != null
                ? $"CN={partyId.Name};"
                : null;
        }

        /// <summary>
        /// Get Trip activity Timezone information from the executing user.
        /// </summary>
        /// <param name="tripActivityId">trip activity id</param>
        /// <param name="userId">user id</param>
        /// <returns>Timezone name</returns>
        public string GetTripActivityTimezoneInformationActivity(EntityReference tripActivityId, Guid? userId)
        {
            var tripActivityDetails = _orgService.Retrieve<cmc_tripactivity>(tripActivityId,
                   new ColumnSet("cmc_activitytype"));

            _logger.Trace($"Trip Activity type {tripActivityDetails.cmc_activitytype}");
            object locationtimezoneCode = null;
            string fetchXml = string.Empty;
            // for event we have timezone field at trip activity level , need to pick up the timezone information from the field.
            if (tripActivityDetails.cmc_activitytype.HasValue && tripActivityDetails.cmc_activitytype == cmc_tripactivity_cmc_activitytype.Event)
            {
                fetchXml = $@"<fetch>
                                    <entity name='cmc_tripactivity'>
                                    <attribute name='cmc_locationtimezone' />
                                    <filter>
                                      <condition attribute='cmc_tripactivityid' operator='eq' value='{tripActivityDetails.cmc_tripactivityId}'/>
                                    </filter>
                                    </entity>
                                </fetch>";
                locationtimezoneCode = _orgService.RetrieveMultipleAll(fetchXml)?.Entities?.FirstOrDefault()?.Attributes["cmc_locationtimezone"];
            }
            else // appointment and other should get the timezone from the logged user/ user who ever sending the trip activity mail.
            {
                fetchXml = $@"<fetch>
                              <entity name='usersettings'>
                                <attribute name='timezonecode' />
                                <filter>
                                  <condition attribute='systemuserid'  operator='eq' value='{userId}'/>
                                </filter>
                              </entity>
                            </fetch>";
                locationtimezoneCode = _orgService.RetrieveMultipleAll(fetchXml)?.Entities?.FirstOrDefault()?.Attributes["timezonecode"];
            }

            // when no timezone information found. // something wrong in the configurations.
            if (locationtimezoneCode == null)
            {
                _logger.Error("No Timezone Information found.");
                return string.Empty;
            }

            // get the standrad timezone name from the system.
            _logger.Debug($"Timezone code Information : { JsonConvert.SerializeObject(locationtimezoneCode)}");
            fetchXml = $@"<fetch>
                              <entity name='timezonedefinition'>
                                <attribute name='standardname' />
                                <filter>
                                  <condition attribute='timezonecode' operator='eq' value='{locationtimezoneCode}'/>
                                </filter>
                              </entity>
                            </fetch>";
            var timezoneName = _orgService.RetrieveMultipleAll(fetchXml)?.Entities?.FirstOrDefault()?.Attributes["standardname"];
            _logger.Info($"Trip Timezone name {timezoneName}");

            return timezoneName.ToString();
        }
    }
}


