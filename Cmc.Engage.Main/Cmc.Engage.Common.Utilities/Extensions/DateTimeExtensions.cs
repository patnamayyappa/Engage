using System;
using System.Globalization;
using System.Linq;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace Cmc.Engage.Common.Utilities
{
    public static class DateTimeExtensions
    {
        public static string ToIso8601Date(this DateTime date)
        {
            return date.ToString("yyyy-MM-ddTHH:mm:ffZ", CultureInfo.InvariantCulture);
        }

        public static DateTime FromIso8601Date(this string date)
        {
            DateTime parsedDate;

            if (!DateTime.TryParse(date.Replace("T", " "), out parsedDate))
            {
                throw new InvalidPluginExecutionException($"Unable to parse iso8601 date to DateTime object: {date}");
            }

            return parsedDate;
        }
        public static DateTime RetrieveLocalTimeFromUtc(IOrganizationService orgService, DateTime utcDate, int? timeZoneCode = null)
        {
            if (!timeZoneCode.HasValue)
            {
                timeZoneCode = RetrieveTimeZoneCode(orgService);
            }

            var request = new LocalTimeFromUtcTimeRequest
            {
                TimeZoneCode = timeZoneCode.Value,
                UtcTime = utcDate
            };

            var response = (LocalTimeFromUtcTimeResponse)orgService.Execute(request);
            return response.LocalTime;
        }

        public static int RetrieveTimeZoneCode(IOrganizationService orgService)
        {
            var fetchXml =
                $@"<fetch top='1'>
			    <entity name='usersettings'>
					<attribute name='localeid' />
					<attribute name='timezonecode' />
					<filter>
						<condition attribute='systemuserid' operator='eq-userid' />
					</filter>
			    </entity>
		    </fetch>";

            var userSetting = orgService.RetrieveMultiple(new FetchExpression(fetchXml)).Entities.FirstOrDefault();
            return userSetting.GetAttributeValue<int>("timezonecode");
        }
    }

}
