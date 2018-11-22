using Cmc.Core.Xrm.ServerExtension.Core;
using Cmc.Core.Xrm.ServerExtension.Logging;
using Cmc.Engage.Common;
using Cmc.Engage.Common.Utilities;
using Cmc.Engage.Models;
using Cmc.Engage.Models.Extn;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cmc.Engage.Common.Utilities.Constants;
using Microsoft.Crm.Sdk.Messages;
using IExecutionContext = Cmc.Core.Xrm.ServerExtension.Core.IExecutionContext;

namespace Cmc.Engage.Marketing
{
    /// <summary>
    /// Event service
    /// </summary>
    public class EventService : IEventService
    {
        private ILogger _tracer;
        private IOrganizationService _orgService;
        ILanguageService _retrieveMultiLingualValues;
        public EventService(ILogger tracer, IOrganizationService orgService, ILanguageService retrieveMultiLingualValues)
        {
            _tracer = tracer ?? throw new ArgumentNullException(nameof(tracer));
            _orgService = orgService;
            _retrieveMultiLingualValues = retrieveMultiLingualValues;
        }

      
        #region Update Trip Activity Event Details
        /// <summary>
        /// On Update of Event Subject,ScheduledStart,cmc_EndDateTime and Location this method will update all related Trip Activity details.
        /// </summary>
        /// <param name="executionContext"></param>        
        public void UpdateTripActivityEventDetails(IExecutionContext executionContext)
        {
            _tracer.Info($"Entered into UpdateTripActivityEventDetails");

            var serviceProvider = executionContext.XrmServiceProvider;
            var pluginContext = serviceProvider.GetPluginExecutionContext();
            _tracer.Info($"UpdateTripActivityEventDetails : Message type is :{pluginContext.MessageName.ToLower()}");
            if (pluginContext.MessageName.ToLower() == "update")
            {
                _tracer.Info($"Entered into UpdateTripActivityEventDetails update ");
                var preImageEvent = pluginContext.GetPreEntityImage<msevtmgt_Event>("PreImage");
                var postImageEvent = pluginContext.GetPostEntityImage<msevtmgt_Event>("PostImage");

                List<cmc_tripactivity> lstTripActivity = GetAssociatedTripActivityForEvent(postImageEvent.Id);
                _tracer.Info($"{lstTripActivity.Count} TripActivity fetched");
                if (lstTripActivity.Count > 0)
                {
                    foreach (cmc_tripactivity tripActivity in lstTripActivity)
                    {
                        if (preImageEvent.msevtmgt_Name != postImageEvent.msevtmgt_Name)
                        {
                            String tripName = tripActivity.GetAliasedAttributeValue<String>("trip.cmc_tripname");
                            if (!String.IsNullOrEmpty(tripName))
                                tripActivity.cmc_name = postImageEvent.msevtmgt_Name + " - " + tripName;                            
                            _tracer.Info($"New Trip Name {tripActivity.cmc_name} for Trip Activity ID {tripActivity.Id}");
                        }

                        if (preImageEvent.msevtmgt_EventStartDate != postImageEvent.msevtmgt_EventStartDate &&
                            tripActivity.Attributes.Keys.Contains("cmc_locationstartdatetime"))
                        {
                            tripActivity["cmc_locationstartdatetime"] = postImageEvent.msevtmgt_EventStartDate;
                            _tracer.Info($"New Trip locations ScheduledStart { tripActivity["cmc_locationstartdatetime"]} for Trip Activity {tripActivity.cmc_name}");
                        }

                        if (preImageEvent.msevtmgt_EventEndDate != postImageEvent.msevtmgt_EventEndDate
                            && tripActivity.Attributes.Keys.Contains("cmc_locationenddatetime"))
                        {
                            tripActivity["cmc_locationenddatetime"] = postImageEvent.msevtmgt_EventEndDate;
                            _tracer.Info($"New Trip locations ScheduledEnd {tripActivity["cmc_locationenddatetime"]} for Trip Activity {tripActivity.cmc_name}");
                        }
                       
                        if (preImageEvent.cmc_startdatetime != postImageEvent.cmc_startdatetime)
                        {
                            tripActivity.cmc_StartDateTime = postImageEvent.cmc_startdatetime;
                            _tracer.Info($"New Trip ScheduledStart {tripActivity.cmc_StartDateTime} for Trip Activity {tripActivity.cmc_name}");
                        }
                        if (preImageEvent.cmc_enddatetime != postImageEvent.cmc_enddatetime)
                        {
                            tripActivity.cmc_EndDateTime = postImageEvent.cmc_enddatetime;
                            _tracer.Info($"New Trip ScheduledEnd {tripActivity.cmc_EndDateTime} for Trip Activity {tripActivity.cmc_name}");
                        }

                        if (preImageEvent.msevtmgt_EventTimeZone != postImageEvent.msevtmgt_EventTimeZone
                            && tripActivity.Attributes.Keys.Contains("cmc_locationtimezone"))
                        {
                            tripActivity["cmc_locationtimezone"] = postImageEvent.msevtmgt_EventTimeZone;
                            _tracer.Info($"New Trip  location Timezone {tripActivity["cmc_locationtimezone"]} for Trip Activity {tripActivity.cmc_name}");
                        }
                      
                        var address= GetEventPrimaryVenue(postImageEvent.Id);
                        var result = string.Equals(address, tripActivity.cmc_location);
                        if (!result)
                        {
                            tripActivity.cmc_location = address;
                            _tracer.Info($"New Trip  location  {tripActivity.cmc_location} for Trip Activity {tripActivity.cmc_name}");
                        }
                        _orgService.Update(tripActivity);
                    }
                    //ExecuteBulkEntities.BulkUpdateBatch(_orgService, lstTripActivity.Cast<Entity>().ToList());
                    _tracer.Info($"{lstTripActivity.Count} TripActivity Updated");
                }

            }
        }


        /// <summary>
        /// Get associated trip activity for event.
        /// </summary>
        /// <param name="guid">event id </param>
        /// <returns></returns>
        private List<cmc_tripactivity> GetAssociatedTripActivityForEvent(Guid guid)
        {
            _tracer.Info($"Get Associate TripActivity ");
            return _orgService.RetrieveMultipleAll($@"
                <fetch distinct='false' >
                <entity name = 'cmc_tripactivity' >
                <attribute name = 'cmc_tripactivityid'/>
                <attribute name = 'cmc_name'/>
                <attribute name = 'cmc_startdatetime'/>
                <attribute name = 'cmc_enddatetime'/>
                <attribute name = 'cmc_locationstartdatetime'/>
                <attribute name = 'cmc_locationtimezone'/>
                <attribute name = 'cmc_locationenddatetime'/>
                <attribute name = 'cmc_location'/>
                <link-entity name = 'msevtmgt_event' from = 'msevtmgt_eventid' to = 'cmc_linkedtoevent' link-type = 'inner' alias = 'ac' >
                <filter type = 'and' >
                    <condition attribute = 'msevtmgt_eventid' operator= 'eq' uitype = 'msevtmgt_event' value = '{guid}' />
                </filter >
                </link-entity >
                <link-entity name = 'cmc_trip' from = 'cmc_tripid' to = 'cmc_trip' link-type = 'inner' alias = 'trip' >                                  
                        <attribute name = 'cmc_tripname'/>                
                </link-entity >
                </entity >
                </fetch >").Entities?.Select(x => x.ToEntity<cmc_tripactivity>()).ToList(); 
        }

        /// <summary>
        /// Get associated primary venu for event
        /// </summary>
        /// <param name="guid">event id </param>
        /// <returns></returns>
        private String GetEventPrimaryVenue(Guid guid) {
            msevtmgt_Venue msevtmgtVenue = _orgService.RetrieveMultipleAll($@"<fetch >
                         <entity name='msevtmgt_venue'>                         
                             <attribute name='msevtmgt_country'/>
                             <attribute name='msevtmgt_city'/>
                             <attribute name='msevtmgt_addressline1' />
                             <attribute name='msevtmgt_addressline3' />
                             <attribute name='msevtmgt_addressline2' />
                            <attribute name='msevtmgt_venueid' />
                             <attribute name='msevtmgt_stateprovince' />
                             <attribute name='msevtmgt_postalcode' /> 
                            <link-entity name='msevtmgt_event' from='msevtmgt_primaryvenue' to='msevtmgt_venueid' link-type='inner' alias='ac'>
                                <filter type = 'and' >
                                <condition attribute = 'msevtmgt_eventid' operator= 'eq'  uitype = 'msevtmgt_event' value = '{guid}'/>
                                </filter >
                            </link-entity >
                         </entity>
                        </fetch>").Entities?.Select(x => x.ToEntity<msevtmgt_Venue>()).ToList().FirstOrDefault();
            String primaryVenu="";
            if (!String.IsNullOrEmpty(msevtmgtVenue.msevtmgt_AddressLine1))
                primaryVenu = msevtmgtVenue.msevtmgt_AddressLine1 + " ";
            if (!String.IsNullOrEmpty(msevtmgtVenue.msevtmgt_AddressLine2))
                primaryVenu = primaryVenu + msevtmgtVenue.msevtmgt_AddressLine2 + " ";
            if (!String.IsNullOrEmpty(msevtmgtVenue.msevtmgt_AddressLine3))
                primaryVenu = primaryVenu + msevtmgtVenue.msevtmgt_AddressLine3 + " ";
            if (!String.IsNullOrEmpty(msevtmgtVenue.msevtmgt_City))
                primaryVenu = primaryVenu + msevtmgtVenue.msevtmgt_City + " ";
            if (!String.IsNullOrEmpty(msevtmgtVenue.msevtmgt_StateProvince))
                primaryVenu = primaryVenu + msevtmgtVenue.msevtmgt_StateProvince + " ";
            if (!String.IsNullOrEmpty(msevtmgtVenue.msevtmgt_Country))
                primaryVenu = primaryVenu + msevtmgtVenue.msevtmgt_Country + " ";
            if (!String.IsNullOrEmpty(msevtmgtVenue.msevtmgt_PostalCode))
                primaryVenu = primaryVenu + msevtmgtVenue.msevtmgt_PostalCode ;
                       
            return primaryVenu;
        }

        #endregion

        #region Update Event Details
        /// <summary>
        /// On Update of msevtmgt_EventStartDate,msevtmgt_EventEndDate,msevtmgt_EventTimeZone of event this method will update cmc_startdatetime,cmc_enddatetime of event.
        /// </summary>
        /// <param name="executionContext"></param>
        public void UpdateEventDetails(IExecutionContext executionContext)
        {
            var serviceProvider = executionContext.XrmServiceProvider;
            var pluginContext = serviceProvider.GetPluginExecutionContext();
            _tracer.Info($"UpdateEventDetails : Message type is :{pluginContext.MessageName.ToLower()}");
            if (pluginContext.MessageName.ToLower() == Constants.Create)
            {
                var eventDetails = pluginContext.GetInputParameter<Entity>("Target").ToEntity<msevtmgt_Event>();
                CreateEvent(eventDetails);
            }
            else if (pluginContext.MessageName.ToLower() == Constants.Update)
            {
                var postImageEvent = pluginContext.GetPostEntityImage<msevtmgt_Event>("PostImage");
                var preImageEvent = pluginContext.GetPreEntityImage<msevtmgt_Event>("PreImage");
                UpdateEvent(postImageEvent, preImageEvent);
            }
        }
        /// <summary>
        /// This method will call on create of event and update cmc_startdatetime,cmc_enddatetime of event.
        /// </summary>
        /// <param name="eventDetails"></param>
        private void CreateEvent(msevtmgt_Event eventDetails)
        {
            _tracer.Info("Entered into CreateEvent");
            if (eventDetails.msevtmgt_EventTimeZone == null) return;
            if (eventDetails.msevtmgt_EventStartDate != null)
            {
                eventDetails.cmc_startdatetime = GetUtcTimeFromOtherTimezone(eventDetails.msevtmgt_EventStartDate.Value,
                    eventDetails.msevtmgt_EventTimeZone.Value);
                _tracer.Info($"{eventDetails.cmc_startdatetime} Start date for event");
            }
            if (eventDetails.msevtmgt_EventEndDate != null)
            {
                eventDetails.cmc_enddatetime = GetUtcTimeFromOtherTimezone(eventDetails.msevtmgt_EventEndDate.Value,
                    eventDetails.msevtmgt_EventTimeZone.Value);
                _tracer.Info($"{eventDetails.cmc_enddatetime} End date for event");
            }
        }
        /// <summary>
        /// This method will call on update  of msevtmgt_EventStartDate,msevtmgt_EventEndDate,msevtmgt_EventTimeZone of event and update cmc_startdatetime,cmc_enddatetime of event.
        /// </summary>
        /// <param name="eventDetails"></param>
        /// <param name="preEventDetails"></param>
        private void UpdateEvent(msevtmgt_Event eventDetails, msevtmgt_Event preEventDetails)
        {
            _tracer.Info("Entered into UpdateEvent");           
             var isValueUpdated = false;
            var updateEvent = new msevtmgt_Event
            {
                msevtmgt_EventId = eventDetails.Id
            };
            if (eventDetails.msevtmgt_EventTimeZone == null || preEventDetails.msevtmgt_EventTimeZone == null) return;
            if (eventDetails.msevtmgt_EventTimeZone.Value != preEventDetails.msevtmgt_EventTimeZone.Value)
            {
                _tracer.Info("Timezone changed for the event record");
                if (eventDetails.msevtmgt_EventStartDate != null)
                {
                    updateEvent.cmc_startdatetime = GetUtcTimeFromOtherTimezone(eventDetails.msevtmgt_EventStartDate.Value,
                        eventDetails.msevtmgt_EventTimeZone.Value);
                    isValueUpdated = true;
                    _tracer.Info($"{updateEvent.cmc_startdatetime} Start date for event");
                }
                if (eventDetails.msevtmgt_EventEndDate != null)
                {
                    updateEvent.cmc_enddatetime = GetUtcTimeFromOtherTimezone(eventDetails.msevtmgt_EventEndDate.Value,
                        eventDetails.msevtmgt_EventTimeZone.Value);
                    isValueUpdated = true;
                    _tracer.Info($"{updateEvent.cmc_enddatetime} End date for event");
                }
            }
            else
            {
                _tracer.Info("Timezone not changed for the event record");
                if (eventDetails.msevtmgt_EventStartDate != null && preEventDetails.msevtmgt_EventStartDate != null)
                {
                    var isDateChanged = DateTime.Compare(eventDetails.msevtmgt_EventStartDate.Value, preEventDetails.msevtmgt_EventStartDate.Value);
                    if (isDateChanged!=0) 
                    {
                        updateEvent.cmc_startdatetime = GetUtcTimeFromOtherTimezone(eventDetails.msevtmgt_EventStartDate.Value,
                            eventDetails.msevtmgt_EventTimeZone.Value);
                        isValueUpdated = true;
                        _tracer.Info($"{updateEvent.cmc_startdatetime} Start date for event");
                    }
                }
                if (eventDetails.msevtmgt_EventEndDate != null && preEventDetails.msevtmgt_EventEndDate != null)
                {
                    var isDateChanged = DateTime.Compare(eventDetails.msevtmgt_EventEndDate.Value, preEventDetails.msevtmgt_EventEndDate.Value);
                    if (isDateChanged != 0)
                    {
                        updateEvent.cmc_enddatetime = GetUtcTimeFromOtherTimezone(eventDetails.msevtmgt_EventEndDate.Value,
                            eventDetails.msevtmgt_EventTimeZone.Value);
                        isValueUpdated = true;
                        _tracer.Info($"{updateEvent.cmc_enddatetime} End date for event");
                    }
                }

            }

            if (isValueUpdated)
            {               
                _orgService.Update(updateEvent);
                _tracer.Info("Event Updated");
            }

        }
        private DateTime? GetUtcTimeFromOtherTimezone(DateTime localTime,int timeZoneCode)
        {
            _tracer.Info("Entered into GetUtcTimeFromLocalTime");
            var request = new UtcTimeFromLocalTimeRequest
            {
                TimeZoneCode = timeZoneCode,
                LocalTime = localTime
            };

            var response = (UtcTimeFromLocalTimeResponse)_orgService.Execute(request);
            _tracer.Info($"Utc Time :{response?.UtcTime.ToString("MM/dd/yyyy HH:mm:ss")}");
            return response?.UtcTime;
        }       
        #endregion
    }

}
