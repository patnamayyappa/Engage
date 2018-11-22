using System;
using System.Collections.Generic;
using System.Linq;
using Cmc.Core.Xrm.ServerExtension.Logging;
using Cmc.Engage.Models;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace Cmc.Engage.Common
{
    public class RetrieveStaffDetailsLogic
    {
        private IOrganizationService _orgService;
        private ILogger _trace;

        public RetrieveStaffDetailsLogic(ILogger trace, IOrganizationService orgService)
        {
            _trace = trace ?? throw new ArgumentNullException(nameof(trace));
            _orgService = orgService ?? throw new ArgumentNullException(nameof(orgService));
        }       
        public StaffMemberDetails GetHours(Guid studentId, Guid successNetworkId, int nextWeeks = 6)
        {
            var successNetworkResult = GetSuccessNetwork(studentId, successNetworkId);
            if (successNetworkResult == null)
            {
                //student and staff are not in same success network
                return null;
            }

            var hourResults = GetStaffHours(successNetworkResult.cmc_staffmemberid.Id, (nextWeeks));

            var weekRanges = GetWeekRanges(DateTime.UtcNow.Date, nextWeeks);

            var weekHours = new List<Week>();

            var hourResultsGroupByLocation = hourResults.GroupBy(x => x.cmc_userlocationid.Id);

            var firstWeekStart = weekRanges.First().StartDate;
            var lastWeekEnd = weekRanges.Last().EndDate;

            //get each locations daily availability based on week range to avoid edge cases of office hour weeks not lining up.
            var locationAvailabilities = GetLocationDays(hourResultsGroupByLocation, firstWeekStart, lastWeekEnd);

            foreach (var weekRange in weekRanges)
            {
                var locationsInWeekRange = locationAvailabilities.Values.Where(x => x.Availability.Any(y => weekRange.StartDate <= y.Key && weekRange.EndDate >= y.Key));

                var locationsOutput = new List<Dictionary<string, object>>();
                foreach (var locationInWeekRange in locationsInWeekRange)
                {
                    var availabilityInWeekRange = locationInWeekRange.Availability.Where(x => weekRange.StartDate <= x.Key && weekRange.EndDate >= x.Key).ToList();

                    //remove this if each location should show under each week even if it has no availability for the week.
                    if (!availabilityInWeekRange.Any(x => x.Value.Count() > 0))
                    {
                        continue;
                    }

                    var locationDict = new Dictionary<string, object>()
                    {
                        {"Campus", locationInWeekRange.Campus},
                        {"Location", locationInWeekRange.LocationName}
                    };

                    foreach (var a in availabilityInWeekRange)
                    {
                        locationDict.Add(a.Key.DayOfWeek.ToString(), a.Value);
                    }

                    locationsOutput.Add(locationDict);
                }

                weekHours.Add(new Week(weekRange, locationsOutput));
            }

            var locations = RetrieveStaffLocations(successNetworkResult.cmc_staffmemberid.Id);

            return new StaffMemberDetails(successNetworkResult, weekHours, locations);

        }

        private Dictionary<Guid, LocationAvailability> GetLocationDays(IEnumerable<IGrouping<Guid, cmc_officehours>> hourResultsGroupByLocation,
            DateTime firstWeekStart, DateTime lastWeekEnd)
        {
            var currentDate = firstWeekStart;
            var locations = new Dictionary<Guid, LocationAvailability>();
            while (currentDate <= lastWeekEnd)
            {
                foreach (var hourResultGroupByLocation in hourResultsGroupByLocation)
                {
                    var account = ((EntityReference)hourResultGroupByLocation.FirstOrDefault()?.GetAttributeValue<AliasedValue>("cmc_userlocation.cmc_accountid").Value);
                    var details = ((string)hourResultGroupByLocation.FirstOrDefault()?.GetAttributeValue<AliasedValue>("cmc_userlocation.cmc_details")?.Value);
                    LocationAvailability location;
                    if (!locations.TryGetValue(hourResultGroupByLocation.Key, out location))
                    {
                        location = new LocationAvailability(account.Name, details, account.Id);
                        locations.Add(hourResultGroupByLocation.Key, location);
                    }

                    //is current date in range and is that day checked as true.
                    var hoursInRange = hourResultGroupByLocation.Where(x => x.cmc_startdate <= currentDate && (x.cmc_enddate == null || x.cmc_enddate >= currentDate) && (bool)x[$"cmc_{currentDate.DayOfWeek.ToString().ToLower()}"] == true);
                    var timeRanges = hoursInRange.Select(x => new TimeRange(x.cmc_starttime.Value.TimeOfDay, x.cmc_endtime.Value.TimeOfDay)).ToList();
                    location.Availability.Add(currentDate, timeRanges);
                }
                currentDate = currentDate.AddDays(1);
            }

            return locations;
        }

        private cmc_successnetwork GetSuccessNetwork(Guid studentId, Guid successNetworkId)
        {
            var staffDetailFetch = $@"<fetch top='1'>
					<entity name='cmc_successnetwork'>
						<attribute name='cmc_staffroleid'/>
						<attribute name='cmc_staffmemberid'/>						
						<link-entity name='systemuser' from='systemuserid' to='cmc_staffmemberid' alias='systemuser'>
							<attribute name='internalemailaddress'/>
							<attribute name='cmc_bio'/>
							<attribute name='address1_telephone1'/>
                            <attribute name='cmc_departmentid' />
						</link-entity>						
						<filter>
							<condition attribute='cmc_studentid' operator='eq' value='{studentId}'/>
							<condition attribute='cmc_successnetworkid' operator='eq' value='{successNetworkId}'/>
							<condition attribute='statecode' operator='eq' value='0'/>
						</filter>
					</entity>
				</fetch>";

            return _orgService.RetrieveMultiple(new FetchExpression(staffDetailFetch)).Entities.FirstOrDefault()?.ToEntity<cmc_successnetwork>();
        }

        private IEnumerable<cmc_officehours> GetStaffHours(Guid staffMemberId, int numFutureWeeks)
        {
            //break out from first query to not replicate all of the user information on each result coming back
            var hoursFetch = $@"<fetch>
					<entity name='cmc_officehours'>
							<attribute name='cmc_starttime'/>
							<attribute name='cmc_endtime'/>
							<attribute name='cmc_startdate'/>
							<attribute name='cmc_enddate'/>
							<attribute name='cmc_duration'/>
							<attribute name='cmc_monday'/>
							<attribute name='cmc_tuesday'/>
							<attribute name='cmc_wednesday'/>
							<attribute name='cmc_thursday'/>
							<attribute name='cmc_friday'/>
							<attribute name='cmc_saturday'/>
							<attribute name='cmc_sunday'/>
							<attribute name='cmc_userlocationid'/>
							<filter>
								<condition attribute='ownerid' operator='eq' value='{staffMemberId}'/>
								<condition attribute='statecode' operator='eq' value='0'/>
								<filter type='or'>
									<filter>
										<condition attribute='cmc_startdate' operator='on-or-before' value='{DateTime.UtcNow}'/>
										<condition attribute='cmc_enddate' operator='on-or-after' value='{DateTime.UtcNow}'/>
									</filter>
                                    <filter>
										<condition attribute='cmc_startdate' operator='on-or-before' value='{DateTime.UtcNow}'/>
										<condition attribute='cmc_enddate' operator='null'/>
									</filter>
									<condition attribute='cmc_startdate' operator='next-x-weeks' value='{numFutureWeeks}'/>
								</filter>								
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
                                  </filter>
                                </link-entity>
							</link-entity>
					</entity>
				</fetch>";
            return _orgService.RetrieveMultiple(new FetchExpression(hoursFetch)).Entities.Select(x => x.ToEntity<cmc_officehours>());
        }

        private IEnumerable<EntityReference> RetrieveStaffLocations(Guid staffMemberId)
        {
            var locationFetch = $@"<fetch distinct='true'>
                  <entity name='account'>
                    <attribute name='name' />
                    <attribute name='accountid' />
                    <order attribute='name' descending='false' />
                    <filter type='and'>
                      <condition attribute='mshied_accounttype' operator='eq' value='{(int)mshied_account_mshied_accounttype.Campus}' />
                      <condition attribute='statecode' operator='eq' value='0' />
                    </filter>
                    <link-entity name='cmc_userlocation' from='cmc_accountid' to='accountid' >
                      <filter type='and'>
                        <condition attribute='cmc_userid' operator='eq' value='{staffMemberId}' />
                        <condition attribute='statecode' operator='eq' value='0' />
                      </filter>
                      <link-entity name='cmc_officehours' from='cmc_userlocationid' to='cmc_userlocationid'>
                        <filter type='and'>
                          <condition attribute='statecode' operator='eq' value='0' />
                        </filter>
                      </link-entity>
                    </link-entity>
                  </entity>
                </fetch>";

            // Build Entity Reference manually so Name gets set
            return _orgService.RetrieveMultiple(new FetchExpression(locationFetch)).Entities.Select(
                record => new EntityReference(record.LogicalName, record.Id)
                {
                    Name = record.GetAttributeValue<string>("name")
                });
        }

        public IList<WeekRange> GetWeekRanges(DateTime startDate, int numberOfWeeks)
        {
            var day = startDate.DayOfWeek;
            //get to most recent Sunday
            startDate = startDate.AddDays((int)startDate.DayOfWeek * -1);

            var ret = new List<WeekRange>();

            for (var i = 0; i < numberOfWeeks; i++)
            {
                ret.Add(new WeekRange(startDate.AddDays(i * 7), startDate.AddDays(6).AddDays(i * 7)));
            }
            return ret;
        }

             

        public class LocationAvailability
        {
            public string Campus { get; private set; }
            public string LocationName { get; private set; }
            public Guid LocationId { get; private set; }
            public Dictionary<DateTime, IEnumerable<TimeRange>> Availability { get; private set; }

            public LocationAvailability(string campus, string locationName, Guid locationId)
            {
                Campus = campus;
                LocationName = locationName;
                LocationId = locationId;
                Availability = new Dictionary<DateTime, IEnumerable<TimeRange>>();
            }
        }

        public class TimeRange
        {
            public TimeSpan StartTime { get; private set; }
            public TimeSpan EndTime { get; private set; }

            public TimeRange(TimeSpan startTime, TimeSpan endTime)
            {
                StartTime = startTime;
                EndTime = endTime;
            }
        }

        
    }
}
