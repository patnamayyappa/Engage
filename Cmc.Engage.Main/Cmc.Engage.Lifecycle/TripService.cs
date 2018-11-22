using System;
using System.Collections.Generic;
using System.Linq;
using Cmc.Core.Xrm.ServerExtension.Core;
using Cmc.Core.Xrm.ServerExtension.Logging;
using Cmc.Engage.Common.Utilities;
using Cmc.Engage.Models;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;


namespace Cmc.Engage.Lifecycle
{
    /// <summary>
    /// TripService 
    /// </summary>
    public class TripService : ITripService
    {
        private IOrganizationService _orgService;
        private readonly ILogger _logger;
        private const string Complete = "Complete";
        private const string TripCompletedorCancelled = "Trip_Completed_Cancelled";
        private const string TripNotApproved = "Trip_Not_Approved";
        private const string TripactivityNotCompleted = "TripActivity_Not_Completed";
        private const string TripactivityNotCancelled = "TripActivity_Not_Cancelled";
        /// <summary>
        /// Constructor method for TripService 
        /// </summary>
        /// <param name="organizationService">organization service</param>
        /// <param name="logger">logger</param>
        public TripService(ILogger logger, IOrganizationService organizationService)
        {
            _logger = logger;
            _orgService = organizationService;
        }
        /// <summary>
        /// CreateUpdateTripService
        /// </summary>
        /// <param name="executionContext"> execution context </param>
        public void CreateUpdateTripService(Core.Xrm.ServerExtension.Core.IExecutionContext executionContext)
        {
            _logger.Info($"Entered into CreateUpdateTrip");

            var serviceProvider = executionContext.XrmServiceProvider;
            var pluginContext = serviceProvider.GetPluginExecutionContext();

            if (pluginContext.MessageName.ToLower() == "create")
            {
                _logger.Info($"Entered into Trip Create");
                cmc_trip targetTrip = null;
                targetTrip = pluginContext.GetTargetEntity<cmc_trip>();
                _logger.Info($"Trip Name : {targetTrip.cmc_tripname}");
                EntityReference ownerId = targetTrip.GetAttributeValue<EntityReference>("ownerid");
                cmc_department department = GetOwnerDepartment(ownerId.Id);
                targetTrip.cmc_Department = department?.ToEntityReference();
                _logger.Info($"Setting Department {department?.cmc_departmentname}");
                // Adding 23:59 Hours in trip End Date for fixed trip activity validation.
                targetTrip.cmc_EndDate = targetTrip.cmc_EndDate.Value.AddMinutes(1439);
                _logger.Info($"Setting EndDate Time {targetTrip.cmc_EndDate}");
            }
            else if (pluginContext.MessageName.ToLower() == "update")
            {
                _logger.Info($"Entered into Trip Update ");
                cmc_trip preImageTrip = null;
                Entity targetTrip = null;
                if (pluginContext.PreEntityImages.Contains("PreImage"))
                {
                    _logger.Info($"Entered into Update PreImage");
                    preImageTrip = pluginContext.PreEntityImages["PreImage"].ToEntity<cmc_trip>();
                }
                if (pluginContext.InputParameters.Contains("Target"))
                {
                    _logger.Info($"Entered into Update Target");
                    targetTrip = (Entity)pluginContext.InputParameters["Target"];
                }
                _logger.Info($"Trip Name : {preImageTrip.cmc_tripname}");
                EntityReference ownerId = targetTrip.GetAttributeValue<EntityReference>("ownerid");

                if (ownerId != null && ownerId != preImageTrip.OwnerId)
                {
                    _logger.Info($"Owner {ownerId.Id}");
                    cmc_department department = GetOwnerDepartment(ownerId.Id);
                    _logger.Info($"Owner's department is {department?.cmc_departmentname}");
                    targetTrip["cmc_department"] = department?.ToEntityReference();
                }

                if (targetTrip.GetAttributeValue<DateTime>("cmc_enddate").Year > 0001 && targetTrip.GetAttributeValue<DateTime>("cmc_enddate") != preImageTrip.cmc_EndDate)
                {
                    // Adding 23:59 Hours in trip End Date for fixed trip activity validation.
                    targetTrip["cmc_enddate"] = targetTrip.GetAttributeValue<DateTime>("cmc_enddate").AddMinutes(1439);
                    _logger.Info($"Updated End Date Time after adding 23:59 Hours : {targetTrip.GetAttributeValue<DateTime>("cmc_enddate")}");
                }

                if (targetTrip.GetAttributeValue<string>("cmc_tripname") != null && targetTrip.GetAttributeValue<string>("cmc_tripname") != preImageTrip.cmc_tripname)
                {
                    _logger.Info($"Entered  Update Trip Activity");
                    //On Update of trip name all child trip activity name will get changed 
                    UpdateTripActivityName(preImageTrip, targetTrip.ToEntity<cmc_trip>());
                }
            }
        }
        /// <summary>
        /// GetOwnerDepartment is method to Get Owner Department.
        /// </summary>
        /// <param name="ownerid">Owner Id</param>
        /// <returns>Department Entity</returns>
        private cmc_department GetOwnerDepartment(Guid ownerid)
        {
            _logger.Info($"Entered into GetOwnerDepartment");

            return (_orgService.RetrieveMultipleAll($@"<fetch distinct='true' mapping='logical' output-format='xml-platform' version='1.0' >
                    <entity name='cmc_department' >
                    <attribute name='cmc_departmentid' />     
                    <attribute name='cmc_departmentname' />  
                    <link-entity name='systemuser' alias='ae' link-type='inner' to='cmc_departmentid' from='cmc_departmentid' >
                    <filter type='and' >
                          <condition attribute='systemuserid' value='{ownerid}' uitype='systemuser' operator='eq' />
                    </filter>
                    </link-entity>
                    </entity>
                    </fetch>")?.Entities).Select(r => r.ToEntity<cmc_department>())?.FirstOrDefault();//.Entities).Cast<cmc_department>()?.FirstOrDefault();

        }


        /// <summary>
        /// Action to Completes the Trip.
        /// </summary>
        /// <param name="executionContext">execution context </param>
        public void CompleteOrCancelTrip(Core.Xrm.ServerExtension.Core.IExecutionContext executionContext)
        {
            _logger.Info($"Entered into CompleteTrip");
            var lstUpdate = new List<Entity>();
            var serviceProvider = executionContext.XrmServiceProvider;
            var pluginContext = serviceProvider.GetPluginExecutionContext();
            var tripId = pluginContext.GetInputParameter<string>("TripId");
            var actiontype = (pluginContext.GetInputParameter<string>("ActionType"));
            var isCompleted = (actiontype == Complete) ? true : false;
            _logger.Trace($"Reading inputparameter : trip Id { tripId}  and actiontype {actiontype}");

            _logger.Trace("Fetching the Trip details");
            var cmcTrip = _orgService.Retrieve(cmc_trip.EntityLogicalName, Guid.Parse(tripId), new ColumnSet(true)).ToEntity<cmc_trip>();

            _logger.Trace($"trip name is { cmcTrip.cmc_tripname}");

            if (cmcTrip.statecode != null && (cmcTrip.cmc_Status == cmc_trip_cmc_status.Completed ||
                                              cmcTrip.statecode.Value == (cmc_tripState)cmc_trip_statuscode.Inactive ||
                                              cmcTrip.cmc_Status == cmc_trip_cmc_status.Canceled))
            {

                pluginContext.OutputParameters["Response"] = TripCompletedorCancelled;
                _logger.Trace($"current Trip status is {cmcTrip.cmc_Status}");
                return;
            }


            _logger.Trace($"current Trip status is {cmcTrip.cmc_ApprovalStatus}");
            if (isCompleted)
            {
                if (cmcTrip.cmc_Status != cmc_trip_cmc_status.Approved)
                {
                    pluginContext.OutputParameters["Response"] = TripNotApproved;
                    return;
                }
            }

            var fetchXml = $@"<fetch>
                              <entity name='cmc_tripactivity'>
                                <attribute name='cmc_tripactivitystatus' />     
                                <filter>
                                  <condition attribute='cmc_trip' operator='eq' value='{tripId}'/>
                                </filter>
                              </entity>
                            </fetch>";
            var tripActivities = _orgService.RetrieveMultiple(new FetchExpression(fetchXml))?.Entities?.Select(x => x.ToEntity<cmc_tripactivity>()).ToList();
            _logger.Trace($"found {tripActivities?.Count()} trip activities related to trip {tripId}");

            if (tripActivities != null && tripActivities.Any())
            {
                if (isCompleted)
                {
                    if (tripActivities.Any(r =>((
                        r.cmc_TripActivityStatus == cmc_tripactivity_cmc_tripactivitystatus.Planned)|| (r.cmc_TripActivityStatus == cmc_tripactivity_cmc_tripactivitystatus.Confirmed) || (r.cmc_TripActivityStatus == cmc_tripactivity_cmc_tripactivitystatus.Requested))))
                    {
                        pluginContext.OutputParameters["Response"] = TripactivityNotCompleted;
                        return;
                    }
                }
                else
                {
                    if (tripActivities.Any(r =>
                        r.cmc_TripActivityStatus == cmc_tripactivity_cmc_tripactivitystatus.Completed))
                    {
                        pluginContext.OutputParameters["Response"] = TripactivityNotCancelled;
                        return;
                    }
                }
            }

            _logger.Trace($"updating the trip status as {(isCompleted ? cmc_trip_cmc_status.Completed : cmc_trip_cmc_status.Canceled)}");
            cmcTrip.cmc_Status = isCompleted ? cmc_trip_cmc_status.Completed : cmc_trip_cmc_status.Canceled;

            lstUpdate.Add(new cmc_trip() { Id = cmcTrip.Id, cmc_Status = cmcTrip.cmc_Status });
            ExecuteBulkEntities.BulkUpdateBatch(_orgService, lstUpdate);
            _logger.Trace($"updating the trip status completed");

            pluginContext.OutputParameters["Response"] = string.Empty;
            _logger.Trace($"completed execution of CompleteTrip");
        }

        #region Update Trip Activity Name On Update of Trip Name
        /// <summary>
        /// On Update of Trip Name this method will update all related Trip Activity Name.
        /// </summary>
        /// <param name="executionContext"></param>
        private void UpdateTripActivityName(cmc_trip preImage, cmc_trip target)
        {
            _logger.Trace($"Entered into UpdateTripActivityName");

            bool isEventAvilable = IsEventEntityAvilable("cmc_tripactivity");
            List<cmc_tripactivity> lstTripActivity = GetAssocatedTripActivityForTrip(target.Id, isEventAvilable);
            _logger.Trace($"{lstTripActivity.Count} TripActivity fetched");
            if (lstTripActivity.Count > 0)
            {
                foreach (cmc_tripactivity tripActivity in lstTripActivity)
                {
                    if ((int)tripActivity.cmc_activitytype == (int)cmc_tripactivity_cmc_activitytype.Appointment)
                    {
                        String appointmentName = tripActivity.GetAliasedAttributeValue<String>("appointment.subject");
                        if (!String.IsNullOrEmpty(appointmentName))
                            tripActivity.cmc_name = appointmentName + " - " + target.cmc_tripname;
                        _logger.Trace($"New Trip Name {tripActivity.cmc_name} for Trip Activity ID {tripActivity.Id}");
                        _orgService.Update(tripActivity);
                    }
                    else if ((int)tripActivity.cmc_activitytype == (int)cmc_tripactivity_cmc_activitytype.Event)
                    {
                        if (isEventAvilable)
                        {
                            String eventName = tripActivity.GetAliasedAttributeValue<String>("event.msevtmgt_name");
                            if (!String.IsNullOrEmpty(eventName))
                                tripActivity.cmc_name = eventName + " - " + target.cmc_tripname;
                            //tripActivity.cmc_name = ReplaceLastOccurrence(tripActivity.cmc_name, preImage.cmc_tripname, target.cmc_tripname);
                            _logger.Trace($"New Trip Name {tripActivity.cmc_name} for Trip Activity ID {tripActivity.Id}");
                            _orgService.Update(tripActivity);
                        }
                    }

                }
                // ExecuteBulkEntities.BulkUpdateBatch(_orgService, lstTripActivity.Cast<Entity>().ToList());
                _logger.Trace($"{lstTripActivity.Count} TripActivity Updated");
            }
        }
        /// <summary>
        /// It will find all assocated tripactivity for parent trip.
        /// </summary>
        /// <param name="guid">Trip ID</param>
        /// <param name="withEvent">Is Event Avilable</param>
        /// <returns></returns>
        private List<cmc_tripactivity> GetAssocatedTripActivityForTrip(Guid guid, bool withEvent)
        {
            if (withEvent)
            {
                return _orgService.RetrieveMultipleAll($@"
                <fetch distinct='false' >
                <entity name = 'cmc_tripactivity' >
                <attribute name = 'cmc_tripactivityid'/>
                <attribute name = 'cmc_name'/>   
                <attribute name = 'cmc_activitytype'/>    
                <filter type = 'and' >                    
                    <condition attribute='cmc_activitytype' operator='ne' value='{(int) cmc_tripactivity_cmc_activitytype.Other}' />
                </filter >
                <link-entity name = 'cmc_trip' from = 'cmc_tripid' to = 'cmc_trip' link-type = 'inner' alias = 'ac' >
                <filter type = 'and' >
                    <condition attribute = 'cmc_tripid' operator= 'eq' uitype = 'cmc_trip' value = '{guid}' />
                </filter >
                </link-entity >
                <link-entity name='appointment' from='activityid' to='cmc_linkedtoappointment' link-type='outer' alias='appointment'>
                    <attribute name='subject' />
                </link-entity>
                <link-entity name='msevtmgt_event' from='msevtmgt_eventid' to='cmc_linkedtoevent' link-type='outer' alias='event'>
                    <attribute name='msevtmgt_name' />
                </link-entity>
                </entity >
                </fetch >").Entities?.Select(x => x.ToEntity<cmc_tripactivity>()).ToList();//Entities?.Cast<cmc_tripactivity>()?.ToList(); }
            } 
            else
            {
                return _orgService.RetrieveMultipleAll($@"
                <fetch distinct='false' >
                <entity name = 'cmc_tripactivity' >
                <attribute name = 'cmc_tripactivityid'/>
                <attribute name = 'cmc_name'/>   
                <attribute name = 'cmc_activitytype'/>    
                <filter type = 'and' >                    
                    <condition attribute='cmc_activitytype' operator='ne' value='{(int) cmc_tripactivity_cmc_activitytype.Other}' />
                </filter >
                <link-entity name = 'cmc_trip' from = 'cmc_tripid' to = 'cmc_trip' link-type = 'inner' alias = 'ac' >
                <filter type = 'and' >
                    <condition attribute = 'cmc_tripid' operator= 'eq' uitype = 'cmc_trip' value = '{guid}' />
                </filter >
                </link-entity >
                <link-entity name='appointment' from='activityid' to='cmc_linkedtoappointment' link-type='outer' alias='appointment'>
                    <attribute name='subject' />
                </link-entity>               
                </entity >
                </fetch >").Entities?.Select(x => x.ToEntity<cmc_tripactivity>()).ToList();
            } //.Entities?.Cast<cmc_tripactivity>()?.ToList(); }

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
        #endregion
    }
}


