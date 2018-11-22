using Cmc.Core.Xrm.ServerExtension.Core;
using Cmc.Engage.Models;
using IExecutionContext = Cmc.Core.Xrm.ServerExtension.Core.IExecutionContext;
using Microsoft.Xrm.Sdk;

namespace Cmc.Engage.Retention
{
	public interface IStaffSurveyService
	{
		/// <summary>
		/// Creates/updates the Staff Survey template questions
		/// </summary>
		/// <param name="context"></param>
		void SaveStaffSurveyTemplate(IExecutionContext context);

		/// <summary>
		/// Validates if the survey instance is valid  or not
		/// </summary>
		/// <param name="context"></param>
		void ValidateSurveyInstance(IExecutionContext context);

		/// <summary>
		/// Creates the Staff Survey Question Responses
		/// </summary>
		/// <param name="context"></param>
		void CreateStaffSurveyQuestionResponses(IExecutionContext context);

		/// <summary>
		/// Performs the post operation logic of the Staff Survey
		/// </summary>
		/// <param name="context"></param>
		void PerformStaffSurveyPostOperationLogic(IExecutionContext context);
		/// <summary>
		/// Creates the Staff Survey from Survey Template
		/// </summary>
		/// <param name="context"></param>
		void CreateStaffSurveyFromTemplate(IExecutionContext context);
        /// <summary>
        /// Create Copy of StaffSurvey Template
        /// </summary>
        /// <param name="context"></param>
		void CopyStaffSurveyTemplate(IExecutionContext context);
        /// <summary>
        /// StaffSurvey Completed or Cancellation Dates are updated based on the survey state
        /// </summary>
        /// <param name="context"></param>
        void UpdateStaffSurveyCompletedCancellationDate(IExecutionContext context);
        /// <summary>
        /// Send Email For StaffSurvey
        /// </summary>
	    void SendStaffSurveyReminderEmail();
        /// <summary>
        /// Azure Function to update Staffsurveys Crossed Duedate
        /// </summary>
        void UpdateSurveysCrossedDuedate();
    }
}
