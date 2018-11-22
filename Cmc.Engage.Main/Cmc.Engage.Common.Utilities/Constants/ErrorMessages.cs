namespace Cmc.Engage.Common.Utilities
{
    public static class ErrorMessages
    {
        public static string PluginIsNotConfiguredcorrectly = "Plugin is not configured correctly";
        public static string HttpErrorOnCreatingJob =
            "An HTTP error status code was encountered when creating the geocode job.";

        public static string HttpErrorOndownloadingResult =
            "An HTTP error status code was encountered when downloading results.";

        public static string HttpErrorOnCheckingJobStatus =
            "An HTTP error status code was encountered when checking job status.";

        public static string LocationHeaderError =
            " The 'Location' header is missing from the HTTP response when creating a goecode job.";

        public static string ErrorMessgae = "Job was aborted due to an error.";

        public static string InvalidEntityTypeErrorMessage = "Error: Invalid entity type - expecting entity logical name of contact, account, or lead.";
        public static string InvalidConnectionStringErrorMessage = "Error: Connection string is invalid.";
        public static string CouldNotFindPostDomAssignmentConfigRecordErrorMessage = "Error: could not find configuration record with name of Post DOM Assignment";
        public static string CouldNotFindCreatedFromCodeForLogicalNameErrorMessage = "Error: could not find a corresponding createdfromcode in map for logical name of ";
        public static string NoDomDefinitionLogicsRelatedToDomMastersWarningMessage = "There are no DOM Definition Logic records related to any of the retrieved DOM Masters, or there are none that match any of the attribute names in the retrieved Execution Orders. Exiting.";
		public static string StaffSurveyValidationError = "Staff Surveys are not assigned as : \r\n -Existing Staff Surveys with matching dates or with dates within the matching dates are already assigned. \r\n - The start date of the Staff Survey is prior to the current date.";
	}

}
