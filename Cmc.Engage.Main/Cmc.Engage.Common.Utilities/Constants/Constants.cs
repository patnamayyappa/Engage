

using Cmc.Engage.Models;
using System.Collections.Generic;

namespace Cmc.Engage.Common.Utilities.Constants
{
    public static class Constants
    {
        public const int TotalMinutesForTimeOut = 5;
        public static string Aborted = "Aborted";
        public static string Pending = "Pending";
        public static string Location = "Location";
        public static string Get = "GET";
        public static string Post = "POST";
        public static string Xml = "xml";
        public static string Status = "Status";
        public static string Succeeded = "succeeded";
        public static string Failed = "failed";
        public static string Output = "output";
        public static string Link = "Link";
        public static string Success = "Success";
        public static string DataflowJobStatus = "Dataflow Job Status:";
        public static string GeocodeDataFlowUrl = "http://spatial.virtualearth.net";
        public static string UrlPath = "/REST/v1/dataflows/geocode";
        public static string ContentType = "application/xml";
        public static string GeocodeFeed = "GeocodeFeed";
        public static string Version = "Version";
        public static string GeocodeEntity = "GeocodeEntity";
        public static string GeocodeRequest = "GeocodeRequest";
        public static string Id = "ID";
        public static string Query = "Query";
        public static string VersionNumber = "2.0";
        public static string InputEqualTo = "input=";
        public static string KeyEqualTo = "key=";
        public static string AndSymbol = "&";
        public static string OutputEqualToXml = "&output=xml";
        public static string QuestionMarkSymbol = "?";
        public static string Exception = "Exception :";
        //public static string BingMapApiKey = "BingMapApiKey";
        //public static string BatchGeocodeSize = "BatchGeocodeSize";
        public static string Create = "create";
        public static string Update = "update";
        //public static string StaffSurveyEmailWorkflowName = "StaffSurveyEmailWorkflowName";
        //public static string StaffSurveySendReminderEmailNumberOfDays = "StaffSurveySendReminderEmailNumberOfDays";

        /* DOM Engine Constants */
        //public static string PostDomAssignmentConfigName = "PostDOMAssignment";
        public static string LookupAttributeType = "Lookup";
        public static string OwnerAttributeType = "Owner";
        public static string CustomerAttributeType = "Customer";
        public static string OptionSetAttributeType = "Picklist";
        public static string BooleanAttributeType = "Boolean";
        public static string DateTimeAttributeType = "DateTime";
        public enum ContactType
        {
            Student = 494280011
        }
        public static readonly List<string> ValidEntityLogicalNames = new List<string>() {
            Contact.EntityLogicalName,
            Account.EntityLogicalName,
            Lead.EntityLogicalName,
            Opportunity.EntityLogicalName
        };
    }
}
