using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cmc.Core.Xrm.ServerExtension.Core;
using Cmc.Core.Xrm.ServerExtension.Logging;
using Cmc.Engage.Common;
using Cmc.Engage.Common.Utilities;
using Cmc.Engage.Models;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Workflow;
using Newtonsoft.Json;
using IExecutionContext = Cmc.Core.Xrm.ServerExtension.Core.IExecutionContext;

namespace Cmc.Engage.Communication
{
    public class AppointmentService : IAppointmentService
    {
        private ILogger _tracer;
        private IOrganizationService _orgService;
        ILanguageService _retrieveMultiLingualValues;
        public AppointmentService(ILogger tracer, IOrganizationService orgService, ILanguageService retrieveMultiLingualValues)
        {
            _tracer = tracer ?? throw new ArgumentNullException(nameof(tracer));
            _orgService = orgService;
            _retrieveMultiLingualValues = retrieveMultiLingualValues;
        }
        #region create Staff Appointment.
        public void CreateStaffAppointment(IExecutionContext context)
        {
            var serviceProvider = context.XrmServiceProvider;
            var pluginContext = serviceProvider.GetPluginExecutionContext();
            _tracer.Trace($"Entered plugin: {nameof(AppointmentService)}");
            Guid? contactId = pluginContext.ParseGuidInput("ContactId");
            Guid userId = pluginContext.ParseGuidInput("UserId").Value;
            DateTime startDate = pluginContext.ParseIso8601DateInput("StartDate");
            DateTime endDate = pluginContext.ParseIso8601DateInput("EndDate");
            Guid locationId = pluginContext.ParseGuidInput("LocationId").Value;
            string title = pluginContext.InputParameters.Contains("Title") ? (string)pluginContext.InputParameters["Title"] : null;
            string description = pluginContext.InputParameters.Contains("Description") ? (string)pluginContext.InputParameters["Description"] : null;
            string staffAppointmentsJson = CreateStaffAppointment(contactId, userId, locationId, startDate, endDate, title, description);
            pluginContext.OutputParameters["StaffAppointmentsJson"] = staffAppointmentsJson;

        }

        public string CreateStaffAppointment(Guid? contactId, Guid userId, Guid locationId, DateTime startDate, DateTime endDate, string title, string description)
        {
            _tracer.Trace($"Entered: {nameof(CreateStaffAppointment)}");

            Entity userParty = new Entity("activityparty") { Attributes = { { "partyid", new EntityReference("systemuser", userId) } } };

            EntityCollection requiredAttendeesCollection = new EntityCollection();

            requiredAttendeesCollection.Entities.Add(userParty);
            if (contactId.HasValue)
            {
                Entity contactParty = new Entity("activityparty") { Attributes = { { "partyid", new EntityReference("contact", contactId.Value) } } };
                requiredAttendeesCollection.Entities.Add(contactParty);
            }

            EntityCollection organizer = new EntityCollection();
            organizer.Entities.Add(new Entity("activityparty") { Attributes = { { "partyid", userParty["partyid"] } } });

            _tracer.Trace($"Creating staff appointment with title {title} for user with Id {userId} at location from {startDate.ToLongDateString()}, {startDate.ToLongTimeString()} to {endDate.ToLongDateString()}, {endDate.ToLongTimeString()}");

            Entity staffAppointment = new Entity("appointment");
            staffAppointment["scheduledstart"] = startDate;
            staffAppointment["scheduledend"] = endDate;
            staffAppointment["subject"] = title;
            staffAppointment["description"] = description;
            staffAppointment["ownerid"] = new EntityReference("systemuser", userId);
            staffAppointment["requiredattendees"] = requiredAttendeesCollection;
            // to show up the regarding object.
            if (contactId.HasValue)
                staffAppointment["regardingobjectid"] = new EntityReference("contact", contactId.Value);
            staffAppointment["organizer"] = organizer;
            staffAppointment["cmc_userlocationid"] = new EntityReference("cmc_userlocation", locationId);
            staffAppointment["statuscode"] = new OptionSetValue((int)appointment_statuscode.Tentative);

            Guid createdAppointmentId = _orgService.Create(staffAppointment);
            staffAppointment.Id = createdAppointmentId;

            _tracer.Trace($"Created appointment ID: {createdAppointmentId.ToString()}");

            StaffAppointment staffAppointmentResult = new StaffAppointment(staffAppointment);

            return JsonConvert.SerializeObject(new List<StaffAppointment>() { staffAppointmentResult });
        }

        #endregion
        #region Delete Staff Appointment
        public void DeleteStaffAppointment(IExecutionContext context)
        {
            var serviceProvider = context.XrmServiceProvider;
            var pluginContext = serviceProvider.GetPluginExecutionContext();
            _tracer.Trace($"Entered plugin: {nameof(AppointmentService)} Test");
            Guid appointmentId = pluginContext.ParseGuidInput("AppointmentId").Value;
            string staffAppointmentsJson = DeleteStaffAppointment(appointmentId);
            pluginContext.OutputParameters["StaffAppointmentsJson"] = staffAppointmentsJson;

        }
        public string DeleteStaffAppointment(Guid appointmentId)
        {
            _tracer.Trace($"Entered: {nameof(DeleteStaffAppointment)}");

            _tracer.Trace($"Retrieving appointmentId with id: {appointmentId}");
            Entity crmAppointment = _orgService.Retrieve("appointment", appointmentId, new ColumnSet(new string[] { "scheduledstart", "scheduledend", "subject", "statuscode", "ownerid" }));

            _tracer.Trace($"Deleting appointment...");
            _orgService.Delete("appointment", appointmentId);

            StaffAppointment staffAppointment = new StaffAppointment(crmAppointment);

            // Need to return json of deleted appointment for kenda dataSource purposes
            return JsonConvert.SerializeObject(new List<StaffAppointment>() { staffAppointment });
        }
        #endregion


        #region Retrieve Staff course
        public void RetrieveStaffAppointments(IExecutionContext context)
        {

            var serviceProvider = context.XrmServiceProvider;
            var pluginContext = serviceProvider.GetPluginExecutionContext();

            _tracer.Trace($"Entered plugin: {nameof(AppointmentService)}");

            Guid? contactId = pluginContext.ParseGuidInput("ContactId");
            Guid? userId = pluginContext.ParseGuidInput("UserId");

            if (contactId == null && userId == null)
            {
                throw new InvalidPluginExecutionException(_retrieveMultiLingualValues.Get("staffAppointments_EmptyParameters"));
            }


            string staffAppointmentsJson = RetrieveStaffAppointmentsJson(contactId, userId);

            pluginContext.OutputParameters["StaffAppointmentsJson"] = staffAppointmentsJson;


        }

        public string RetrieveStaffAppointmentsJson(Guid? contactId, Guid? userId)
        {
            _tracer.Trace($"Entered: {nameof(RetrieveStaffAppointmentsJson)}");

            List<Entity> staffAppointments = RetrieveStaffAppointmentsByUser(userId.Value).Entities.ToList();

            if (contactId.HasValue)
            {
                _tracer.Trace($"Filtering appointment list by contactId: {contactId.Value}");
                staffAppointments = staffAppointments.Where(x => DoesAppointmentRequiredAttendeesContainContact(x, contactId.Value)).ToList();
            }

            _tracer.Trace($"Converting {staffAppointments.Count} CRM staff appointments with contact to serializable objects that the calendar can use");
            List<StaffAppointment> staffAppointmentObjects = staffAppointments.Select(x => new StaffAppointment(x)).ToList();

            _tracer.Trace("Serializing staff appointments list");
            return JsonConvert.SerializeObject(staffAppointmentObjects);
        }

        private EntityCollection RetrieveStaffAppointmentsByUser(Guid userId)
        {
            _tracer.Trace($"Entered: {nameof(RetrieveStaffAppointmentsByUser)}");

            return _orgService.RetrieveMultiple(new FetchExpression($@"
                <fetch distinct='false' mapping='logical' output-format='xml-platform' version='1.0'>						
                    <entity name='appointment'>
                        <attribute name='ownerid'/>
				        <attribute name='scheduledstart'/>
				        <attribute name='scheduledend'/>
				        <attribute name='subject'/>
                        <attribute name='description'/>
				        <attribute name='statuscode'/>
                        <attribute name='requiredattendees'/>
			            <link-entity name='activityparty' from='activityid' to='activityid' alias='ac'>
			                <filter type='and'>
			                    <condition attribute='participationtypemask' operator='eq' value='5' />
			                </filter>
			                <link-entity name='systemuser' from='systemuserid' to='partyid' alias='ad'>
			                    <filter type='and'>
			            	        <condition attribute='systemuserid' operator='eq' uitype='systemuser' value='{userId}' />
			        	        </filter>
			                </link-entity>
			            </link-entity>
                    </entity>
                </fetch>"));
        }

        private bool DoesAppointmentRequiredAttendeesContainContact(Entity appointment, Guid contactId)
        {
            var required = appointment.GetAttributeValue<EntityCollection>("requiredattendees");

            return required.Entities.Any(x =>
            {
                var partyId = x.GetAttributeValue<EntityReference>("partyid");
                return partyId.Id == contactId;
            });
        }

        private StaffAppointment ConvertCrmAppointmentToStaffAppointment(Entity staffAppointment)
        {
            return new StaffAppointment(staffAppointment);
        }


        public void RetrieveStaffAvailability(IExecutionContext context)
        {
            var serviceProvider = context.XrmServiceProvider;
            var pluginContext = serviceProvider.GetPluginExecutionContext();

            _tracer.Trace($"Entered plugin: {nameof(AppointmentService)}");

            Guid userId = pluginContext.ParseGuidInput("UserId").Value;
            Guid accountId = pluginContext.ParseGuidInput("AccountId").Value;


            string staffAvailabilityJson = RetrieveStaffAvailability(userId, accountId);

            pluginContext.OutputParameters["StaffAvailabilityJson"] = staffAvailabilityJson;
        }
        public string RetrieveStaffAvailability(Guid userId, Guid accountId)
        {
            _tracer.Trace($"Entered: {nameof(RetrieveStaffAvailability)}");

            List<Entity> officeHours = RetrieveUserOfficeHours(userId, accountId);
            List<StaffAvailability> availabilities = new List<StaffAvailability>();
            officeHours.ForEach(x => availabilities.Add(ExtractAvailabilitiesFromOfficeHours(x)));
            return JsonConvert.SerializeObject(availabilities);
        }

        private StaffAvailability ExtractAvailabilitiesFromOfficeHours(Entity officeHours)
        {
            _tracer.Trace($"Entered: {nameof(ExtractAvailabilitiesFromOfficeHours)}");

            List<DayOfWeek> days = GetDaysOfWeekAvailable(officeHours);
            List<DateTime> daysOfWeekDates = GetDatesForEachDayOfWeekForOfficeHours(days, officeHours);
            List<Entity> existingUserAppointments = RetrieveExistingUserAppointmentsWithinOfficeHours(officeHours);

            _tracer.Trace($"Found {existingUserAppointments.Count} overlapping user appointments");

            StaffAvailability availabilities = CalculateStaffAvailabilityFromDayDates(daysOfWeekDates, existingUserAppointments, officeHours);
            return availabilities;
        }

        private List<DateTime> GetDatesForEachDayOfWeekForOfficeHours(List<DayOfWeek> daysOfWeek, Entity officeHours)
        {
            _tracer.Trace($"Entered: {nameof(GetDatesForEachDayOfWeekForOfficeHours)}");

            List<DateTime> daysOfWeekDates = new List<DateTime>();
            DateTime startDate = officeHours.GetAttributeValue<DateTime>("cmc_startdate");
            DateTime endDate = officeHours.GetAttributeValue<DateTime>("cmc_enddate");
            TimeSpan dateDiff = endDate.Subtract(startDate);

            _tracer.Trace("For each DayOfWeek selected in Office Hours, finding corresponding dates within the office hours start and end date");
            foreach (DayOfWeek day in daysOfWeek)
            {
                for (int i = 0; i <= dateDiff.Days; i++)
                {
                    if (startDate.Date.AddDays(i).DayOfWeek == day)
                    {
                        DateTime dayDate = startDate.Date.AddDays(i);
                        daysOfWeekDates.Add(dayDate);
                    }
                }
            }

            return daysOfWeekDates;
        }

        private StaffAvailability CalculateStaffAvailabilityFromDayDates(List<DateTime> days, List<Entity> existingAppointments, Entity officeHours)
        {
            _tracer.Trace($"Entered: {nameof(CalculateStaffAvailabilityFromDayDates)}");

            DateTime officeHoursStartTime = officeHours.GetAttributeValue<DateTime>("cmc_starttime");
            DateTime officeHoursEndTime = officeHours.GetAttributeValue<DateTime>("cmc_endtime");
            EntityReference ownerId = officeHours.GetAttributeValue<EntityReference>("ownerid");
            EntityReference userLocationid = officeHours.GetAttributeValue<EntityReference>("cmc_userlocationid");
            decimal duration = officeHours.GetAttributeValue<decimal>("cmc_duration");
            StaffAvailability availability = new StaffAvailability(ownerId.Id, userLocationid.Id, duration);

            _tracer.Trace("Adding date ranges in Iso8601 string format which represent ranges where the user is NOT available, including ranges representing overlapping appointments");

            foreach (DateTime dayDate in days)
            {
                DateTime start = new DateTime(dayDate.Year, dayDate.Month, dayDate.Day, officeHoursStartTime.Hour, officeHoursStartTime.Minute, 0);
                DateTime end = new DateTime(dayDate.Year, dayDate.Month, dayDate.Day, officeHoursEndTime.Hour, officeHoursEndTime.Minute, 0);

                List<DateRange> rangesUnavailableForDay = GetAvailabilityRangesForOverlappingAppointments(start, end, existingAppointments);

                availability.AddDateRanges(rangesUnavailableForDay);
            }

            _tracer.Trace($"Added {availability.DateRanges.Count} ranges where user is unavailable");

            return availability;
        }

        private List<DateRange> GetAvailabilityRangesForOverlappingAppointments(DateTime start, DateTime end, List<Entity> existingAppointments)
        {
            _tracer.Trace($"Entered: {nameof(GetAvailabilityRangesForOverlappingAppointments)}");

            List<DateRange> rangesUnavailableForDay = new List<DateRange>();

            DateTime zeroHour = new DateTime(start.Year, start.Month, start.Day, 0, 0, 0);
            DateTime lastHour = new DateTime(start.Year, start.Month, start.Day, 0, 0, 0).AddDays(1);

            rangesUnavailableForDay.Add(new DateRange
            {
                Start = zeroHour.ToIso8601Date(),
                End = start.ToIso8601Date()
            });

            rangesUnavailableForDay.Add(new DateRange
            {
                Start = end.ToIso8601Date(),
                End = lastHour.ToIso8601Date()
            });

            foreach (Entity appointment in existingAppointments)
            {
                var appointmentStart = appointment.GetAttributeValue<DateTime>("scheduledstart");
                var appointmentEnd = appointment.GetAttributeValue<DateTime>("scheduledend");

                if (IsRangeOverlappingWithExistingAppointment(start, end, appointmentStart, appointmentEnd))
                {
                    rangesUnavailableForDay.Add(new DateRange() { Start = appointmentStart.ToIso8601Date(), End = appointmentEnd.ToIso8601Date(), IsConflictingAppointmentRange = true });
                }
            }

            return rangesUnavailableForDay;
        }

        private bool IsRangeOverlappingWithExistingAppointment(DateTime rangeStart, DateTime rangeEnd, DateTime appointmentStart, DateTime appointmentEnd)
        {
            return appointmentStart < rangeEnd && appointmentEnd > rangeStart;
        }

        private List<DayOfWeek> GetDaysOfWeekAvailable(Entity officeHours)
        {
            _tracer.Trace($"Entered: {nameof(GetDaysOfWeekAvailable)}");

            List<DayOfWeek> daysOfWeek = new List<DayOfWeek>();
            Dictionary<string, DayOfWeek> checkboxNameToDayOfWeekMap = new Dictionary<string, DayOfWeek>()
            {
                { "cmc_sunday", DayOfWeek.Sunday },
                { "cmc_monday", DayOfWeek.Monday },
                { "cmc_tuesday", DayOfWeek.Tuesday },
                { "cmc_wednesday", DayOfWeek.Wednesday },
                { "cmc_thursday", DayOfWeek.Thursday },
                { "cmc_friday", DayOfWeek.Friday},
                { "cmc_saturday", DayOfWeek.Saturday }
            };

            foreach (string fieldName in checkboxNameToDayOfWeekMap.Keys)
            {
                bool dayOfWeekChecked = officeHours.GetAttributeValue<bool>(fieldName);
                if (dayOfWeekChecked)
                {
                    daysOfWeek.Add(checkboxNameToDayOfWeekMap[fieldName]);
                }
            }

            return daysOfWeek;
        }

        private List<Entity> RetrieveUserOfficeHours(Guid userId, Guid accountId)
        {

            _tracer.Trace($"Entered: {nameof(RetrieveUserOfficeHours)}");

            _tracer.Trace($"Retrieving office hours for user with Id {userId} at location (Campus account) with Id {accountId} with start date before today and end date after today");
            return _orgService.RetrieveMultiple(new FetchExpression($@"
                <fetch distinct='false' mapping='logical' output-format='xml-platform' version='1.0'>						
                    <entity name='cmc_officehours'>
		                <attribute name='cmc_monday'/>
				        <attribute name='cmc_tuesday'/>
				        <attribute name='cmc_wednesday'/>
				        <attribute name='cmc_thursday'/>
				        <attribute name='cmc_friday'/>
				        <attribute name='cmc_saturday'/>
				        <attribute name='cmc_sunday'/>
				        <attribute name='cmc_startdate'/>
				        <attribute name='cmc_enddate'/>
				        <attribute name='cmc_starttime'/>
                        <attribute name='cmc_endtime'/>
				        <attribute name='cmc_duration'/>
                        <attribute name='cmc_userlocationid'/>
				        <attribute name='ownerid'/>
		                <filter type='and'>
		                    <condition attribute='ownerid' operator='eq' value='{userId}' />
							<condition attribute='cmc_enddate' operator='on-or-after' value='{DateTime.UtcNow}' />
		                </filter>
						<link-entity name='cmc_userlocation' from='cmc_userlocationid' to='cmc_userlocationid' alias='cmc_userlocation'>
							<attribute name='cmc_accountid'/>
							<attribute name='cmc_details'/>
                            <filter type='and'>
                                <condition attribute='statecode' operator='eq' value='0' />
                            </filter>
                            <link-entity name='account' to='cmc_accountid' from='accountid' >
                                <filter type='and'>
                                <condition attribute='mshied_accounttype' operator='eq' value='{(int)mshied_account_mshied_accounttype.Campus}' />
                                <condition attribute='statecode' operator='eq' value='0' />
								<condition attribute='accountid' operator='eq' value='{accountId}' />
                                </filter>
                            </link-entity>
						</link-entity>
                    </entity>
                </fetch>")).Entities.ToList();
        }

        private List<Entity> RetrieveExistingUserAppointmentsWithinOfficeHours(Entity officeHours)
        {
            _tracer.Trace($"Entered: {nameof(RetrieveExistingUserAppointmentsWithinOfficeHours)}");

            EntityReference ownerId = officeHours.GetAttributeValue<EntityReference>("ownerid");
            DateTime startDate = officeHours.GetAttributeValue<DateTime>("cmc_startdate");
            DateTime endDate = officeHours.GetAttributeValue<DateTime>("cmc_enddate");

            _tracer.Trace("Retrieving all appointments where user is required attendee that overlap with office hour dates");
            return _orgService.RetrieveMultiple(new FetchExpression($@"
                <fetch distinct='false' mapping='logical' output-format='xml-platform' version='1.0'>						
                    <entity name='appointment'>
                        <attribute name='ownerid'/>
				        <attribute name='scheduledstart'/>
				        <attribute name='scheduledend'/>
				        <attribute name='subject'/>
				        <attribute name='statuscode'/>
						<filter type='and'>
			                <condition attribute='scheduledstart' operator='on-or-after' value='{startDate}' />
							<condition attribute='scheduledend' operator='on-or-before' value='{endDate}' />
			            </filter>
			            <link-entity name='activityparty' from='activityid' to='activityid' alias='ac'>
			                <filter type='and'>
			                    <condition attribute='participationtypemask' operator='eq' value='5' />
			                </filter>
			                <link-entity name='systemuser' from='systemuserid' to='partyid' alias='ad'>
			                    <filter type='and'>
			            	        <condition attribute='systemuserid' operator='eq' uitype='contact' value='{ownerId.Id}' />
			        	        </filter>
			                </link-entity>
			            </link-entity>
                    </entity>
                </fetch>")).Entities.ToList();
        }


        #endregion


        #region Workflow Activities
        public ActivityMimeAttachment AttachAppointmentICalFileService(EntityReference emilId,
           EntityReference appointmntId)
        {
            var emailId = emilId;
            var appointmentId = appointmntId;

            _tracer.Trace($"Creating an ICal field for email {emailId.Id} and Appointment {appointmentId.Id}.");

            _tracer.Trace("Retrieving Appointment.");
            var appointment = _orgService.Retrieve<Appointment>(appointmentId,
                new ColumnSet("scheduledstart", "scheduledend", "subject", "description",
                    "location", "isalldayevent"));

            _tracer.Trace("Retrieiving Activity Parties");
            var activityParties = _orgService.RetrieveMultipleAll(
                $@"<fetch count='5000' aggregate='false' distinct='false' mapping='logical'>
                     <entity name='activityparty'>
                       <attribute name='addressused' />
                       <attribute name='participationtypemask' />
                       <attribute name='partyid' />
                       <filter>
                         <condition attribute='participationtypemask' operator='in'>
                           <value>5</value>
                           <value>6</value>
                           <value>7</value>
                         </condition>
                         <condition attribute='activityid' operator='eq' value='{appointmentId.Id}' />
                       </filter>
                     </entity>
                   </fetch>");

            var fileString = BuildICalFile(appointment, activityParties, _tracer);

            _tracer.Trace("Creating the Attachment.");
            var mimeAttachment = new ActivityMimeAttachment()
            {
                FileName = "Appointment.ics",
                MimeType = "text/calendar",
                Body = fileString,
                ObjectId = emailId,
                ObjectTypeCode = emailId.LogicalName
            };

            return mimeAttachment;
        }
        private string BuildICalFile(Appointment appointment, EntityCollection activityParties, ILogger tracer)
        {
            tracer.Trace("Building ICal file.");
            string organizer = null;
            List<string> attendees = new List<string>();

            tracer.Trace("Adding attendees.");
            foreach (var record in activityParties.Entities)
            {
                var activityParty = record.ToEntity<ActivityParty>();
                switch (activityParty.ParticipationTypeMask)
                {
                    case activityparty_participationtypemask.Organizer:
                        organizer = $"ORGANIZER;{BuildCommonName(activityParty.PartyId)}:mailto:{activityParty.AddressUsed}";
                        break;
                    case activityparty_participationtypemask.Requiredattendee:
                        attendees.Add($"ATTENDEE;{BuildCommonName(activityParty.PartyId)}ROLE=REQ-PARTICIPANT:mailto:{activityParty.AddressUsed}");
                        break;
                    case activityparty_participationtypemask.Optionalattendee:
                        attendees.Add($"ATTENDEE;{BuildCommonName(activityParty.PartyId)}ROLE=OPT-PARTICIPANT:mailto:{activityParty.AddressUsed}");
                        break;
                }
            }

            // This text must not have any leading whitespace on each line otherwise the ICS file will be invalid
            var calendar = $@"BEGIN:VCALENDAR
PRODID:-//Campus Management//Appointment Generation//EN
VERSION:2.0
BEGIN:VEVENT
{string.Join(Environment.NewLine, attendees)}
DESCRIPTION:{appointment.Description}
DTEND{BuildDateTime(appointment.ScheduledEnd.Value, appointment.IsAllDayEvent ?? false)}
DTSTAMP:{DateTime.UtcNow.ToString("yyyyMMddTHHmmssZ")}
DTSTART{BuildDateTime(appointment.ScheduledStart.Value, appointment.IsAllDayEvent ?? false)}
{organizer ?? ""}
SEQUENCE:0
SUMMARY:{appointment.Subject}
LOCATION:{appointment.Location}
UID:{Guid.NewGuid()}
END:VEVENT
END:VCALENDAR";

            return Convert.ToBase64String(Encoding.UTF8.GetBytes(calendar));
        }
        private string BuildCommonName(EntityReference partyId)
        {
            return partyId != null
                   ? $"CN={partyId.Name};"
                   : null;
        }
        private string BuildDateTime(DateTime value, bool isAllDayEvent)
        {
            return isAllDayEvent
                   ? $";VALUE=DATE:{value.ToUniversalTime().ToString("yyyyMMdd")}"
                   : $":{value.ToUniversalTime().ToString("yyyyMMddTHHmmssZ")}";
        }


        public string RetrieveDepartmentPhoneNumberService(List<object> departmentId)
        {
            _tracer.Trace("Determining if their is a Department Phone Number to Retrieve.");
            EntityReference dptid = (EntityReference)departmentId.FirstOrDefault();
            if (dptid == null)
            {
                _tracer.Trace("No Department. Returning without setting a Phone Number");
                return null;
            }
            else
            {
                _tracer.Trace("Retreiving Department Phone Number");
                var department = _orgService.Retrieve(dptid, new ColumnSet("cmc_phonenumber")).ToEntity<cmc_department>();
                _tracer.Trace($"Setting Phone Number to {department.cmc_phonenumber}");
                return department.cmc_phonenumber;
            }
        }

        #endregion

        #region Update Trip Activity Appointment Details
        /// <summary>
        /// On Update of Appointment Subject,ScheduledStart,cmc_EndDateTime and Location this method will update all related Trip Activity details.
        /// </summary>
        /// <param name="executionContext"></param>
        public void UpdateTripActivityAppointmentDetails(IExecutionContext executionContext)
        {
            _tracer.Info($"Entered into UpdateTripActivityAppointmentDetails");

            var serviceProvider = executionContext.XrmServiceProvider;
            var pluginContext = serviceProvider.GetPluginExecutionContext();
            _tracer.Info($"UpdateTripActivityAppointmentDetails : Message type is :{pluginContext.MessageName.ToLower()}");
            if (pluginContext.MessageName.ToLower() == "update")
            {
                _tracer.Info($"Entered into UpdateTripActivityAppointmentDetails update ");
                var preImageAppointment = pluginContext.GetPreEntityImage<Appointment>("PreImage");
                var postImageAppointment = pluginContext.GetPostEntityImage<Appointment>("PostImage");

                List<cmc_tripactivity> lstTripActivity = GetAssocatedTripActivityForAppointment(postImageAppointment.Id);
                _tracer.Info($"{lstTripActivity.Count} TripActivity fetched");
                if (lstTripActivity.Count>0)
                {
                    foreach (cmc_tripactivity tripActivity in lstTripActivity)
                    {
                        if(preImageAppointment.Subject != postImageAppointment.Subject)
                        {
                            String tripName = tripActivity.GetAliasedAttributeValue<String>("trip.cmc_tripname");
                            if (!String.IsNullOrEmpty(tripName))
                                tripActivity.cmc_name = postImageAppointment.Subject + " - " + tripName;                           
                            _tracer.Info($"New Trip Name {tripActivity.cmc_name} for Trip Activity ID {tripActivity.Id}");
                        }
                        if (preImageAppointment.ScheduledStart != postImageAppointment.ScheduledStart)
                        {
                            tripActivity.cmc_StartDateTime = postImageAppointment.ScheduledStart;
                            _tracer.Info($"New Trip ScheduledStart {tripActivity.cmc_StartDateTime} for Trip Activity {tripActivity.cmc_name}");
                        }
                        if (preImageAppointment.ScheduledEnd != postImageAppointment.ScheduledEnd)
                        {
                            tripActivity.cmc_EndDateTime = postImageAppointment.ScheduledEnd;
                            _tracer.Info($"New Trip ScheduledEnd {tripActivity.cmc_EndDateTime} for Trip Activity {tripActivity.cmc_name}");
                        }                        
                        if (preImageAppointment.Location != postImageAppointment.Location)
                        {
                            tripActivity.cmc_location = postImageAppointment.Location;
                            _tracer.Info($"New Trip Location {tripActivity.cmc_location} for Trip Activity  {tripActivity.cmc_name}");
                        }
                        _orgService.Update(tripActivity);
                    }
                    //ExecuteBulkEntities.BulkUpdateBatch(_orgService, lstTripActivity.Cast<Entity>().ToList());
                    _tracer.Info($"{lstTripActivity.Count} TripActivity Updated");
                }

            }
        }

        private List<cmc_tripactivity> GetAssocatedTripActivityForAppointment(Guid guid)
        {
            return _orgService.RetrieveMultipleAll($@"
                <fetch distinct='false' >
                <entity name = 'cmc_tripactivity' >
                <attribute name = 'cmc_tripactivityid'/>
                <attribute name = 'cmc_name'/>
                <attribute name = 'cmc_startdatetime'/>
                <attribute name = 'cmc_enddatetime'/>
                <attribute name = 'cmc_location'/>
                <link-entity name = 'appointment' from = 'activityid' to = 'cmc_linkedtoappointment' link-type = 'inner' alias = 'ac' >
                <filter type = 'and' >
                    <condition attribute = 'activityid' operator= 'eq' uitype = 'appointment' value = '{guid}' />
                </filter >
                </link-entity >
                <link-entity name = 'cmc_trip' from = 'cmc_tripid' to = 'cmc_trip' link-type = 'inner' alias = 'trip' >                                  
                        <attribute name = 'cmc_tripname'/>                
                </link-entity >
                </entity >
                </fetch >").Entities?.Cast<cmc_tripactivity>()?.ToList();
        }       
        #endregion
    }
}